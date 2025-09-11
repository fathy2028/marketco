# 🛒 Advanced E-commerce Microservices Platform

A comprehensive microservices-based e-commerce platform featuring JWT authentication, TTL cart management, RabbitMQ messaging, circuit breakers, and comprehensive monitoring.

## 🏗 Architecture Overview

This system implements an advanced microservices architecture with:

- **Spring Boot Services**: Config Server, Discovery, Auth, Product, Order, API Gateway
- **ASP.NET Core Services**: Cart (with TTL), Payment
- **Infrastructure**: RabbitMQ, Redis, PostgreSQL, MySQL, SQL Server
- **Monitoring**: Prometheus + Grafana dashboards
- **Frontend**: Angular + PrimeNG (planned)

## 🚀 Quick Start

### Prerequisites

- Docker and Docker Compose
- Java 17+ JDK
- .NET 8 SDK
- Node.js 18+ and npm

### 1. Start Infrastructure Services

```bash
# Start databases, message broker, and monitoring
docker-compose up -d mysql postgres-auth postgres-order redis rabbitmq prometheus grafana

# Wait for databases to initialize (30-60 seconds)
```

### 2. Start Core Services

```bash
# Start configuration and discovery services
docker-compose up -d config-server
# Wait 30 seconds for config server to be ready
docker-compose up -d discovery-service

# Start business services
docker-compose up -d auth-service product-service
```

### 3. Access Services

| Service                 | URL                    | Description                                      |
| ----------------------- | ---------------------- | ------------------------------------------------ |
| **Config Server**       | http://localhost:8888  | Centralized configuration                        |
| **Eureka Discovery**    | http://localhost:8761  | Service registry                                 |
| **Auth Service**        | http://localhost:8081  | User authentication + JWT                        |
| **Product Service**     | http://localhost:8082  | Product catalog                                  |
| **RabbitMQ Management** | http://localhost:15672 | Message broker (rabbitmq_user/rabbitmq_password) |
| **Grafana**             | http://localhost:3000  | Monitoring dashboards (admin/admin)              |
| **Prometheus**          | http://localhost:9090  | Metrics collection                               |

## 📋 Implementation Status

### ✅ Completed Services

1. **Infrastructure Setup** ✅

   - Docker Compose orchestration
   - Multi-stage Docker builds
   - Network and volume configuration

2. **Config Server** ✅

   - Centralized configuration management
   - Environment-specific configs
   - Git repository support

3. **Discovery Service (Eureka)** ✅

   - Service registration and discovery
   - Health monitoring
   - Load balancing support

4. **Auth Service** ✅

   - JWT token generation and validation
   - User registration and login
   - Role-based access (Customer/Admin)
   - BCrypt password hashing
   - PostgreSQL persistence

5. **Product Service** ✅

   - CRUD operations for products
   - Category management
   - Stock management with reservation
   - RabbitMQ integration for stock events
   - MySQL persistence
   - Circuit breaker configuration

6. **Monitoring Stack** ✅
   - Prometheus metrics collection
   - Grafana dashboards
   - Service health monitoring
   - Custom business metrics

### 🔄 Next Phase (To Be Implemented)

7. **Order Service** (Spring Boot + PostgreSQL)

   - Order lifecycle management
   - JWT validation
   - RabbitMQ event publishing
   - Circuit breaker integration

8. **Cart Service** (ASP.NET Core + Redis)

   - TTL-based cart management (24h/7d/∞)
   - Session management
   - Real-time cart updates

9. **Payment Service** (ASP.NET Core + SQL Server)

   - Payment processing simulation
   - Order status updates
   - Payment failure handling

10. **API Gateway** (Spring Boot + Circuit Breaker)

    - Route management
    - JWT validation
    - Rate limiting
    - Circuit breaker patterns

11. **Frontend** (Angular + PrimeNG)
    - Product browsing
    - Cart management
    - User authentication
    - Order placement

## 🔧 Technical Features

### Authentication & Security

- **JWT-based authentication** with configurable expiration
- **Role-based access control** (Customer, Admin)
- **Secure password hashing** with BCrypt
- **Token validation** at gateway and service levels

### Cart TTL Logic

- **Default TTL**: 24 hours
- **Order placed**: Extended to 7 days
- **Payment completed**: Indefinite storage
- **Redis-based** for high performance

### Messaging & Events

- **RabbitMQ** for asynchronous communication
- **Stock reservation** events
- **Order status** updates
- **Payment notifications**

### Resilience Patterns

- **Circuit breakers** on service calls
- **Health checks** and monitoring
- **Graceful degradation**
- **Retry mechanisms**

### Monitoring & Observability

- **Prometheus** for metrics collection
- **Grafana** for visualization
- **Custom business metrics**
- **Service health dashboards**

## 🧪 Testing the Services

### Test Auth Service

```bash
# Register a new user
curl -X POST http://localhost:8081/auth/signup \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@example.com","password":"password123"}'

# Login and get JWT token
curl -X POST http://localhost:8081/auth/signin \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"password123"}'
```

### Test Product Service

```bash
# Get all products
curl http://localhost:8082/products

# Create a new product
curl -X POST http://localhost:8082/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Product","description":"A test product","price":29.99,"stock":100,"category":"Electronics"}'

# Search products
curl "http://localhost:8082/products/search?name=Test"
```

## 🗂 Project Structure

```
marketco/
├── config-server/          # Centralized configuration
├── discovery-service/      # Eureka service registry
├── auth-service/           # JWT authentication
├── product-service/        # Product catalog
├── order-service/          # Order management (planned)
├── cart-service/           # TTL cart with Redis (planned)
├── payment-service/        # Payment processing (planned)
├── api-gateway/            # Gateway + circuit breaker (planned)
├── frontend/               # Angular SPA (planned)
├── monitoring/             # Prometheus & Grafana configs
└── docker-compose.yml     # Orchestration
```

## 📊 Monitoring Dashboards

Access Grafana at http://localhost:3000 (admin/admin) to view:

- **System Health**: CPU, memory, database connections
- **Business Metrics**: Orders, revenue, cart abandonment
- **Service Performance**: API latency, error rates
- **Infrastructure**: Redis hits, RabbitMQ queue depths

## 🔄 Development Workflow

1. **Infrastructure first**: Ensure all databases and message brokers are running
2. **Config Server**: Always start first to provide configuration
3. **Discovery Service**: Start second for service registration
4. **Business Services**: Start in any order (they'll register with Eureka)
5. **API Gateway**: Start last to route to all services
6. **Frontend**: Connect to API Gateway

## 🏃‍♂️ Next Steps

The foundation is solid! The next implementation phase includes:

1. **Complete Order Service** with full business logic
2. **Implement Cart Service** with TTL logic in ASP.NET Core
3. **Build Payment Service** with SQL Server integration
4. **Create API Gateway** with comprehensive routing and circuit breakers
5. **Develop Angular Frontend** with PrimeNG components
6. **Add comprehensive testing** and CI/CD pipeline

This architecture demonstrates enterprise-level microservices patterns with proper separation of concerns, resilience patterns, and observability.
