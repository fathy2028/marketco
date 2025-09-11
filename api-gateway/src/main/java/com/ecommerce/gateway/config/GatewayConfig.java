package com.ecommerce.gateway.config;

import com.ecommerce.gateway.filter.JwtAuthenticationFilter;
import org.springframework.cloud.circuitbreaker.resilience4j.ReactiveResilience4JCircuitBreakerFactory;
import org.springframework.cloud.circuitbreaker.resilience4j.Resilience4JConfigBuilder;
import org.springframework.cloud.client.circuitbreaker.Customizer;
import org.springframework.cloud.gateway.filter.ratelimit.KeyResolver;
import org.springframework.cloud.gateway.route.RouteLocator;
import org.springframework.cloud.gateway.route.builder.RouteLocatorBuilder;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.http.HttpStatus;
import org.springframework.web.server.ServerWebExchange;
import reactor.core.publisher.Mono;

import java.time.Duration;

@Configuration
public class GatewayConfig {

    private final JwtAuthenticationFilter jwtAuthenticationFilter;

    public GatewayConfig(JwtAuthenticationFilter jwtAuthenticationFilter) {
        this.jwtAuthenticationFilter = jwtAuthenticationFilter;
    }

    @Bean
    public RouteLocator customRouteLocator(RouteLocatorBuilder builder) {
        return builder.routes()
                // Auth Service Routes
                .route("auth-service", r -> r
                        .path("/api/auth/**")
                        .filters(f -> f
                                .stripPrefix(1)
                                .circuitBreaker(config -> config
                                        .setName("auth-service-cb")
                                        .setFallbackUri("forward:/fallback/auth"))
                                .requestRateLimiter(config -> config
                                        .setRateLimiter(redisRateLimiter())
                                        .setKeyResolver(userKeyResolver())))
                        .uri("lb://auth-service"))

                // Product Service Routes
                .route("product-service", r -> r
                        .path("/api/products/**")
                        .filters(f -> f
                                .stripPrefix(1)
                                .circuitBreaker(config -> config
                                        .setName("product-service-cb")
                                        .setFallbackUri("forward:/fallback/products"))
                                .requestRateLimiter(config -> config
                                        .setRateLimiter(redisRateLimiter())
                                        .setKeyResolver(userKeyResolver())))
                        .uri("lb://product-service"))

                // Order Service Routes (JWT Required)
                .route("order-service", r -> r
                        .path("/api/orders/**")
                        .filters(f -> f
                                .stripPrefix(1)
                                .filter(jwtAuthenticationFilter.apply(new JwtAuthenticationFilter.Config()))
                                .circuitBreaker(config -> config
                                        .setName("order-service-cb")
                                        .setFallbackUri("forward:/fallback/orders"))
                                .requestRateLimiter(config -> config
                                        .setRateLimiter(redisRateLimiter())
                                        .setKeyResolver(userKeyResolver())))
                        .uri("lb://order-service"))

                // Cart Service Routes (JWT Required)
                .route("cart-service", r -> r
                        .path("/api/cart/**")
                        .filters(f -> f
                                .stripPrefix(1)
                                .filter(jwtAuthenticationFilter.apply(new JwtAuthenticationFilter.Config()))
                                .circuitBreaker(config -> config
                                        .setName("cart-service-cb")
                                        .setFallbackUri("forward:/fallback/cart"))
                                .requestRateLimiter(config -> config
                                        .setRateLimiter(redisRateLimiter())
                                        .setKeyResolver(userKeyResolver())))
                        .uri("lb://cart-service"))

                // Payment Service Routes (JWT Required)
                .route("payment-service", r -> r
                        .path("/api/payments/**")
                        .filters(f -> f
                                .stripPrefix(1)
                                .filter(jwtAuthenticationFilter.apply(new JwtAuthenticationFilter.Config()))
                                .circuitBreaker(config -> config
                                        .setName("payment-service-cb")
                                        .setFallbackUri("forward:/fallback/payments"))
                                .requestRateLimiter(config -> config
                                        .setRateLimiter(redisRateLimiter())
                                        .setKeyResolver(userKeyResolver())))
                        .uri("lb://payment-service"))

                .build();
    }

    @Bean
    public Customizer<ReactiveResilience4JCircuitBreakerFactory> defaultCustomizer() {
        return factory -> factory.configureDefault(id -> new Resilience4JConfigBuilder(id)
                .circuitBreakerConfig(io.github.resilience4j.circuitbreaker.CircuitBreakerConfig.custom()
                        .slidingWindowSize(10)
                        .permittedNumberOfCallsInHalfOpenState(3)
                        .slidingWindowType(
                                io.github.resilience4j.circuitbreaker.CircuitBreakerConfig.SlidingWindowType.TIME_BASED)
                        .waitDurationInOpenState(Duration.ofSeconds(5))
                        .failureRateThreshold(50)
                        .slowCallRateThreshold(50)
                        .slowCallDurationThreshold(Duration.ofSeconds(2))
                        .build())
                .timeLimiterConfig(io.github.resilience4j.timelimiter.TimeLimiterConfig.custom()
                        .timeoutDuration(Duration.ofSeconds(3))
                        .build())
                .build());
    }

    @Bean
    public KeyResolver userKeyResolver() {
        return exchange -> {
            String authHeader = exchange.getRequest().getHeaders().getFirst("Authorization");
            if (authHeader != null && authHeader.startsWith("Bearer ")) {
                // Use JWT token as key for rate limiting
                return Mono.just(authHeader.substring(7, Math.min(authHeader.length(), 50)));
            }
            // Fallback to IP address
            return Mono.just(exchange.getRequest().getRemoteAddress().getAddress().getHostAddress());
        };
    }

    @Bean
    public org.springframework.cloud.gateway.filter.ratelimit.RedisRateLimiter redisRateLimiter() {
        return new org.springframework.cloud.gateway.filter.ratelimit.RedisRateLimiter(10, 20, 1);
        // 10 requests per second, burst capacity of 20, 1 token per request
    }
}
