package com.ecommerce.product.config;

import org.springframework.amqp.core.*;
import org.springframework.amqp.rabbit.connection.ConnectionFactory;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.amqp.support.converter.Jackson2JsonMessageConverter;
import org.springframework.amqp.support.converter.MessageConverter;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class RabbitMQConfig {

    // Exchange
    public static final String PRODUCT_EXCHANGE = "product.exchange";

    // Queues
    public static final String STOCK_RESERVE_QUEUE = "product.stock.reserve";
    public static final String STOCK_RELEASE_QUEUE = "product.stock.release";

    // Routing Keys
    public static final String STOCK_RESERVE_ROUTING_KEY = "product.stock.reserve";
    public static final String STOCK_RELEASE_ROUTING_KEY = "product.stock.release";

    @Bean
    public DirectExchange productExchange() {
        return new DirectExchange(PRODUCT_EXCHANGE);
    }

    @Bean
    public Queue stockReserveQueue() {
        return QueueBuilder.durable(STOCK_RESERVE_QUEUE).build();
    }

    @Bean
    public Queue stockReleaseQueue() {
        return QueueBuilder.durable(STOCK_RELEASE_QUEUE).build();
    }

    @Bean
    public Binding stockReserveBinding() {
        return BindingBuilder
                .bind(stockReserveQueue())
                .to(productExchange())
                .with(STOCK_RESERVE_ROUTING_KEY);
    }

    @Bean
    public Binding stockReleaseBinding() {
        return BindingBuilder
                .bind(stockReleaseQueue())
                .to(productExchange())
                .with(STOCK_RELEASE_ROUTING_KEY);
    }

    @Bean
    public MessageConverter messageConverter() {
        return new Jackson2JsonMessageConverter();
    }

    @Bean
    public RabbitTemplate rabbitTemplate(ConnectionFactory connectionFactory) {
        RabbitTemplate template = new RabbitTemplate(connectionFactory);
        template.setMessageConverter(messageConverter());
        return template;
    }
}


