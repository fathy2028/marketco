# 🛒 MarketCo - E-commerce Microservices Platform

A comprehensive e-commerce platform built with microservices architecture, featuring user management, product catalog, shopping cart, order processing, payment handling, and admin dashboard.

## 📋 Table of Contents

- [Architecture Overview](#architecture-overview)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Microservices](#microservices)
- [Databases](#databases)
- [Frontend](#frontend)
- [Infrastructure](#infrastructure)
- [API Documentation](#api-documentation)
- [Getting Started](#getting-started)
- [Testing](#testing)
- [Monitoring](#monitoring)
- [Features](#features)
- [Contributing](#contributing)

## 🏗️ Architecture Overview

This project follows a **microservices architecture** pattern with the following key components:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   API Gateway   │    │  Discovery      │
│   (Angular)     │◄──►│  (Spring Cloud) │◄──►│  Service        │
│                 │    │                 │    │  (Eureka)       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                                ▼
        ┌─────────────────────────────────────────────────────────┐
        │                    Microservices                        │
        │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐      │
        │  │  Auth   │ │ Product │ │  Order  │ │  Cart   │      │
        │  │Service  │ │Service  │ │Service  │ │Service  │      │
        │  └─────────┘ └─────────┘ └─────────┘ └─────────┘      │
        │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐      │
        │  │Payment  │ │Config   │ │RabbitMQ │ │ Redis   │      │
        │  │Service  │ │Server   │ │Broker   │ │ Cache   │      │
        │  └─────────┘ └─────────┘ └─────────┘ └─────────┘      │
        └─────────────────────────────────────────────────────────┘
                                │
                                ▼
        ┌─────────────────────────────────────────────────────────┐
        │                    Databases                            │
        │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐      │
        │  │PostgreSQL│ │PostgreSQL│ │  MySQL  │ │SQL Server│     │
        │  │ (Auth)   │ │ (Order)  │ │(Product)│ │(Payment) │     │
        │  └─────────┘ └─────────┘ └─────────┘ └─────────┘      │
        └─────────────────────────────────────────────────────────┘
```

### Key Architectural Principles

- **Service Independence**: Each microservice is independently deployable and scalable
- **Database per Service**: Each service has its own database
- **API Gateway Pattern**: Centralized entry point for all client requests
- **Service Discovery**: Automatic service registration and discovery
- **Event-Driven Architecture**: Asynchronous communication via message queues
- **CQRS Pattern**: Command Query Responsibility Segregation for better performance

## 🛠️ Technology Stack

### Backend Services

- **Java 17** with **Spring Boot 3.x**
- **Spring Cloud Gateway** for API Gateway
- **Spring Cloud Config** for centralized configuration
- **Spring Security** with JWT authentication
- **Eureka** for service discovery
- **.NET 8** for Cart and Payment services
- **RabbitMQ** for message queuing
- **Redis** for caching

### Frontend

- **Angular 17** with TypeScript
- **PrimeNG** for UI components
- **RxJS** for reactive programming
- **Angular Material** for additional UI components

### Databases

- **PostgreSQL** for Auth and Order services
- **MySQL** for Product service
- **SQL Server** for Payment service
- **Redis** for Cart caching

### Infrastructure

- **Docker** and **Docker Compose** for containerization
- **Prometheus** for metrics collection
- **Grafana** for monitoring and visualization

## 📁 Project Structure

```
marketco/
├── frontend/                          # Angular frontend application
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/            # Angular components
│   │   │   │   ├── admin/            # Admin dashboard components
│   │   │   │   │   ├── admin-users/  # User management
│   │   │   │   │   ├── admin-products/ # Product management
│   │   │   │   │   └── admin.component.*
│   │   │   │   ├── cart/             # Shopping cart
│   │   │   │   ├── layout/           # Header, footer, navigation
│   │   │   │   ├── orders/           # Order management
│   │   │   │   ├── payment/          # Payment processing
│   │   │   │   └── products/         # Product catalog
│   │   │   ├── models/               # TypeScript interfaces
│   │   │   ├── services/             # Angular services
│   │   │   └── guards/               # Route guards
│   │   └── assets/                   # Static assets
│   ├── package.json
│   └── angular.json
├── api-gateway/                       # Spring Cloud Gateway
│   ├── src/main/java/com/ecommerce/gateway/
│   │   ├── filter/                   # JWT authentication filter
│   │   └── GatewayApplication.java
│   └── src/main/resources/
│       └── application.yml
├── auth-service/                      # Authentication microservice
│   ├── src/main/java/com/ecommerce/auth/
│   │   ├── controller/               # REST controllers
│   │   ├── service/                  # Business logic
│   │   ├── security/                 # Security configuration
│   │   ├── model/                    # JPA entities
│   │   └── repository/               # Data access layer
│   └── src/main/resources/
│       └── application.yml
├── product-service/                   # Product catalog microservice
│   ├── src/main/java/com/ecommerce/product/
│   │   ├── controller/
│   │   ├── service/
│   │   ├── model/
│   │   └── repository/
│   └── src/main/resources/
│       └── application.yml
├── order-service/                     # Order management microservice
│   ├── src/main/java/com/ecommerce/order/
│   │   ├── controller/
│   │   ├── service/
│   │   ├── model/
│   │   └── repository/
│   └── src/main/resources/
│       └── application.yml
├── cart-service/                      # Shopping cart microservice (.NET)
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   └── Program.cs
├── payment-service/                   # Payment processing microservice (.NET)
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   └── Program.cs
├── config-server/                     # Centralized configuration
│   ├── src/main/java/com/ecommerce/config/
│   └── src/main/resources/config/
├── discovery-service/                 # Eureka service discovery
│   ├── src/main/java/com/ecommerce/discovery/
│   └── src/main/resources/
│       └── application.yml
├── docker-compose.yml                 # Container orchestration
├── backend-testing-urls.json         # API testing documentation
└── README.md
```

## 🔧 Microservices

### 1. API Gateway (`api-gateway`)

**Technology**: Spring Cloud Gateway, Java 17

**Purpose**: Central entry point for all client requests, handles routing, CORS, and JWT authentication.

**Key Features**:

- Request routing to appropriate microservices
- JWT token validation and authentication
- CORS configuration
- Load balancing
- Rate limiting

**Configuration**:

- Port: 8080
- Routes: `/api/auth/*`, `/api/products/*`, `/api/orders/*`, `/api/cart/*`, `/api/payments/*`
- JWT secret synchronization across services

### 2. Auth Service (`auth-service`)

**Technology**: Spring Boot 3.x, Spring Security, JWT, PostgreSQL

**Purpose**: Handles user authentication, authorization, and user management.

**Key Features**:

- User registration and login
- JWT token generation and validation
- Password encryption with BCrypt
- Role-based access control (Admin/User)
- User profile management

**Endpoints**:

- `POST /api/auth/signup` - User registration
- `POST /api/auth/signin` - User login
- `GET /api/auth/profile` - Get user profile
- `PUT /api/auth/profile` - Update user profile

**Database**: PostgreSQL (port 5432)

- Tables: `users`, `roles`

### 3. Product Service (`product-service`)

**Technology**: Spring Boot 3.x, JPA, MySQL

**Purpose**: Manages product catalog, inventory, and product information.

**Key Features**:

- Product CRUD operations
- Category management
- Inventory tracking
- Product search and filtering
- Pagination support

**Endpoints**:

- `GET /api/products` - Get all products (paginated)
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create product (Admin only)
- `PUT /api/products/{id}` - Update product (Admin only)
- `DELETE /api/products/{id}` - Delete product (Admin only)

**Database**: MySQL (port 3306)

- Tables: `products`, `categories`

### 4. Order Service (`order-service`)

**Technology**: Spring Boot 3.x, JPA, PostgreSQL

**Purpose**: Handles order creation, management, and order history.

**Key Features**:

- Order creation and management
- Order status tracking
- Order history for users
- Integration with payment service

**Endpoints**:

- `GET /api/orders` - Get user orders
- `POST /api/orders` - Create new order
- `GET /api/orders/{id}` - Get order details
- `PUT /api/orders/{id}/status` - Update order status

**Database**: PostgreSQL (port 5432)

- Tables: `orders`, `order_items`

### 5. Cart Service (`cart-service`)

**Technology**: .NET 8, ASP.NET Core, Redis

**Purpose**: Manages shopping cart functionality and cart persistence.

**Key Features**:

- Add/remove items from cart
- Update item quantities
- Cart persistence with Redis
- Anonymous cart support
- Cart clearing after order completion

**Endpoints**:

- `GET /api/cart/{userId}` - Get user cart
- `POST /api/cart/add` - Add item to cart
- `PUT /api/cart/update` - Update item quantity
- `DELETE /api/cart/remove` - Remove item from cart
- `DELETE /api/cart/clear` - Clear cart

**Database**: Redis (port 6379)

- Key pattern: `cart:{userId}`

### 6. Payment Service (`payment-service`)

**Technology**: .NET 8, ASP.NET Core, SQL Server

**Purpose**: Handles payment processing and payment history.

**Key Features**:

- Payment processing simulation
- Payment status tracking
- Payment history
- Integration with order service
- JWT authentication

**Endpoints**:

- `POST /api/payments/process` - Process payment
- `GET /api/payments/{userId}` - Get payment history
- `GET /api/payments/{id}` - Get payment details

**Database**: SQL Server (port 1433)

- Tables: `payments`, `payment_methods`

### 7. Config Server (`config-server`)

**Technology**: Spring Cloud Config

**Purpose**: Centralized configuration management for all microservices.

**Features**:

- Centralized configuration files
- Environment-specific configurations
- Dynamic configuration updates
- Git-based configuration storage

### 8. Discovery Service (`discovery-service`)

**Technology**: Eureka Server

**Purpose**: Service registration and discovery for microservices.

**Features**:

- Automatic service registration
- Health monitoring
- Load balancing support
- Service discovery

## 🗄️ Databases

### PostgreSQL (Auth & Order Services)

- **Port**: 5432
- **Purpose**: User authentication and order management
- **Tables**:
  - `users`: User accounts and profiles
  - `roles`: User roles and permissions
  - `orders`: Order information
  - `order_items`: Individual order line items

### MySQL (Product Service)

- **Port**: 3306
- **Purpose**: Product catalog and inventory
- **Tables**:
  - `products`: Product information
  - `categories`: Product categories

### SQL Server (Payment Service)

- **Port**: 1433
- **Purpose**: Payment processing and history
- **Tables**:
  - `payments`: Payment transactions
  - `payment_methods`: Payment method information

### Redis (Cart Service)

- **Port**: 6379
- **Purpose**: Shopping cart caching
- **Data Structure**: Key-value store for cart data

## 🎨 Frontend

### Technology Stack

- **Angular 17** with TypeScript
- **PrimeNG** for UI components
- **RxJS** for reactive programming
- **Angular Router** for navigation
- **Angular Forms** for form handling

### Key Features

#### 1. User Interface

- **Responsive Design**: Mobile-first approach
- **Modern UI**: Clean, professional interface
- **Component-Based**: Reusable Angular components
- **Theme Support**: Consistent styling across the application

#### 2. User Management

- **Authentication**: Login/logout functionality
- **Registration**: User account creation
- **Profile Management**: User profile updates
- **Role-Based Access**: Different views for admin and regular users

#### 3. Product Catalog

- **Product Listing**: Paginated product display
- **Product Details**: Detailed product information
- **Search & Filter**: Product search and filtering
- **Image Handling**: Product images with fallbacks

#### 4. Shopping Cart

- **Add to Cart**: Add products to shopping cart
- **Cart Management**: Update quantities, remove items
- **Cart Persistence**: Cart data persistence
- **Anonymous Cart**: Cart functionality without login

#### 5. Order Management

- **Order Creation**: Create orders from cart
- **Order History**: View past orders
- **Order Tracking**: Track order status
- **Order Details**: Detailed order information

#### 6. Payment Processing

- **Payment Form**: Secure payment form
- **Payment Methods**: Multiple payment options
- **Payment Confirmation**: Payment success/failure handling
- **Order Integration**: Seamless order-payment integration

#### 7. Admin Dashboard

- **User Management**: CRUD operations for users
- **Product Management**: CRUD operations for products
- **Statistics**: Dashboard with key metrics
- **Role Management**: User role assignment

### Component Structure

```
frontend/src/app/
├── components/
│   ├── admin/                 # Admin dashboard
│   │   ├── admin-users/      # User management
│   │   ├── admin-products/   # Product management
│   │   └── admin.component.*
│   ├── cart/                 # Shopping cart
│   ├── layout/               # Header, footer, navigation
│   ├── orders/               # Order management
│   ├── payment/              # Payment processing
│   └── products/             # Product catalog
├── models/                   # TypeScript interfaces
├── services/                 # Angular services
└── guards/                   # Route guards
```

## 🏗️ Infrastructure

### Docker & Containerization

- **Multi-container setup** with Docker Compose
- **Service isolation** with individual containers
- **Environment configuration** via environment variables
- **Volume mounting** for persistent data
- **Network isolation** with custom Docker networks

### Monitoring & Observability

- **Prometheus**: Metrics collection and monitoring
- **Grafana**: Visualization and dashboards
- **Health Checks**: Service health monitoring
- **Logging**: Centralized logging across services

### Message Queuing

- **RabbitMQ**: Asynchronous communication
- **Event-driven architecture**: Loose coupling between services
- **Message persistence**: Reliable message delivery

## 📚 API Documentation

### Authentication Endpoints

```http
POST /api/auth/signup
Content-Type: application/json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "role": ["admin" | "user"]
}

POST /api/auth/signin
Content-Type: application/json
{
  "username": "string",
  "password": "string"
}
```

### Product Endpoints

```http
GET /api/products?page=0&size=10&category=Electronics
GET /api/products/{id}
POST /api/products (Admin only)
PUT /api/products/{id} (Admin only)
DELETE /api/products/{id} (Admin only)
```

### Cart Endpoints

```http
GET /api/cart/{userId}
POST /api/cart/add
PUT /api/cart/update
DELETE /api/cart/remove
DELETE /api/cart/clear
```

### Order Endpoints

```http
GET /api/orders
POST /api/orders
GET /api/orders/{id}
PUT /api/orders/{id}/status
```

### Payment Endpoints

```http
POST /api/payments/process
GET /api/payments/{userId}
GET /api/payments/{id}
```

## 🚀 Getting Started

### Prerequisites

- Docker and Docker Compose
- Node.js 18+ (for local frontend development)
- .NET 8 SDK (for local .NET services)
- Java 17 (for local Java services)

### Quick Start with Docker

1. **Clone the repository**

```bash
git clone <repository-url>
cd marketco
```

2. **Start all services**

```bash
docker-compose up -d
```

3. **Access the application**

- Frontend: http://localhost:4200
- API Gateway: http://localhost:8080
- Admin Dashboard: http://localhost:4200/admin

### Local Development

1. **Start infrastructure services**

```bash
docker-compose up -d postgres-auth-db postgres-order-db mysql-product-db sqlserver-payment-db redis-cart rabbitmq-broker discovery-service config-server
```

2. **Start backend services**

```bash
# Java services
cd auth-service && ./mvnw spring-boot:run
cd product-service && ./mvnw spring-boot:run
cd order-service && ./mvnw spring-boot:run
cd api-gateway && ./mvnw spring-boot:run

# .NET services
cd cart-service && dotnet run
cd payment-service && dotnet run
```

3. **Start frontend**

```bash
cd frontend
npm install
ng serve
```

## 🧪 Testing

### Backend Testing

- **Unit Tests**: JUnit for Java services, xUnit for .NET services
- **Integration Tests**: Spring Boot Test for Java services
- **API Testing**: Postman collections and automated tests

### Frontend Testing

- **Unit Tests**: Jasmine and Karma
- **E2E Tests**: Cypress or Protractor
- **Component Tests**: Angular Testing Utilities

### Test Data

- **Sample Users**: Admin and regular user accounts
- **Sample Products**: Electronics, clothing, books, etc.
- **Test Orders**: Various order states for testing

## 📊 Monitoring

### Prometheus Metrics

- Service health metrics
- Request/response metrics
- Database connection metrics
- Custom business metrics

### Grafana Dashboards

- Service overview dashboard
- Database performance dashboard
- Business metrics dashboard
- Error tracking dashboard

### Health Checks

- Service availability monitoring
- Database connectivity checks
- External service dependency checks

## ✨ Features

### User Features

- **User Registration & Login**: Secure authentication system
- **Product Browsing**: Browse and search products
- **Shopping Cart**: Add, update, and remove items
- **Order Management**: Place and track orders
- **Payment Processing**: Secure payment handling
- **Profile Management**: Update user information

### Admin Features

- **User Management**: Create, update, delete users
- **Product Management**: Manage product catalog
- **Order Management**: View and manage orders
- **Dashboard**: Key metrics and statistics
- **Role Management**: Assign user roles

### Technical Features

- **Microservices Architecture**: Scalable and maintainable
- **JWT Authentication**: Secure token-based authentication
- **CORS Support**: Cross-origin resource sharing
- **API Gateway**: Centralized request routing
- **Service Discovery**: Automatic service registration
- **Message Queuing**: Asynchronous communication
- **Caching**: Redis-based caching for performance
- **Monitoring**: Comprehensive monitoring and logging

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For support and questions:

- Create an issue in the repository
- Contact the development team
- Check the documentation and API references

---

**Built with ❤️ using modern microservices architecture and best practices.**
