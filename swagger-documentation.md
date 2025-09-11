# 📚 **E-commerce Platform API Documentation**

## 🌐 **Swagger/OpenAPI Documentation URLs**

All services are configured with comprehensive Swagger/OpenAPI documentation accessible through the following URLs:

### **🔧 Direct Service Access**

| Service             | Swagger UI                                  | OpenAPI JSON                                  |
| ------------------- | ------------------------------------------- | --------------------------------------------- |
| **Auth Service**    | http://localhost:8081/swagger-ui/index.html | http://localhost:8081/v3/api-docs             |
| **Product Service** | http://localhost:8082/swagger-ui/index.html | http://localhost:8082/v3/api-docs             |
| **Order Service**   | http://localhost:8083/swagger-ui/index.html | http://localhost:8083/v3/api-docs             |
| **Cart Service**    | http://localhost:5001/swagger/index.html    | http://localhost:5001/swagger/v1/swagger.json |
| **Payment Service** | http://localhost:5002/swagger/index.html    | http://localhost:5002/swagger/v1/swagger.json |

### **🚪 Via API Gateway (Recommended)**

| Service             | Gateway Swagger UI                                   | Gateway OpenAPI JSON                                   |
| ------------------- | ---------------------------------------------------- | ------------------------------------------------------ |
| **Auth Service**    | http://localhost:8080/auth/swagger-ui/index.html     | http://localhost:8080/auth/v3/api-docs                 |
| **Product Service** | http://localhost:8080/products/swagger-ui/index.html | http://localhost:8080/products/v3/api-docs             |
| **Order Service**   | http://localhost:8080/orders/swagger-ui/index.html   | http://localhost:8080/orders/v3/api-docs               |
| **Cart Service**    | http://localhost:8080/cart/swagger/index.html        | http://localhost:8080/cart/swagger/v1/swagger.json     |
| **Payment Service** | http://localhost:8080/payments/swagger/index.html    | http://localhost:8080/payments/swagger/v1/swagger.json |

---

## 📋 **API Service Descriptions**

### **🔐 Auth Service API**

- **Purpose**: User authentication, registration, and JWT token management
- **Key Features**:
  - User registration and login
  - JWT token generation and validation
  - Role-based access control (Customer/Admin)
  - Token refresh and validation endpoints

**Main Endpoints:**

- `POST /auth/signup` - Register new user
- `POST /auth/signin` - User login
- `GET /auth/validate` - Validate JWT token

---

### **📦 Product Service API**

- **Purpose**: Product catalog and inventory management
- **Key Features**:
  - Complete CRUD operations for products
  - Category management
  - Product search and filtering
  - Stock reservation and release
  - Inventory tracking

**Main Endpoints:**

- `GET /products` - List all products (paginated)
- `POST /products` - Create new product
- `GET /products/{id}` - Get product by ID
- `GET /products/category/{category}` - Get products by category
- `GET /products/search` - Search products
- `POST /products/{id}/reserve` - Reserve stock
- `POST /products/{id}/release` - Release stock

---

### **📋 Order Service API**

- **Purpose**: Order lifecycle management and processing
- **Key Features**:
  - Order creation and management
  - Order status tracking (Created → Confirmed → Shipped → Delivered)
  - Order history and analytics
  - Integration with stock reservation
  - Event publishing for order state changes

**Main Endpoints:**

- `GET /orders` - List orders (paginated)
- `POST /orders` - Create new order
- `GET /orders/{id}` - Get order by ID
- `GET /orders/user/{userId}` - Get user orders
- `PUT /orders/{id}/status` - Update order status
- `POST /orders/{id}/confirm` - Confirm order
- `POST /orders/{id}/ship` - Ship order
- `DELETE /orders/{id}` - Cancel order

---

### **🛒 Cart Service API**

- **Purpose**: Shopping cart management with TTL logic
- **Key Features**:
  - **TTL-based cart expiration** (24h → 7d → ∞)
  - Real-time cart updates
  - Cart item management
  - Cart statistics and analytics
  - Automatic cleanup of expired items

**TTL Logic:**

- **Default**: 24 hours for new items
- **Order Placed**: 7 days when order is created
- **Payment Completed**: Indefinite storage

**Main Endpoints:**

- `GET /api/cart/{userId}` - Get user cart
- `POST /api/cart/add` - Add item to cart
- `PUT /api/cart/{userId}/items/{itemId}` - Update cart item
- `DELETE /api/cart/{userId}/items/{itemId}` - Remove cart item
- `POST /api/cart/{userId}/ttl` - Update cart TTL
- `GET /api/cart/statistics` - Get cart statistics
- `POST /api/cart/{userId}/checkout` - Prepare cart for checkout

---

### **💳 Payment Service API**

- **Purpose**: Payment processing and transaction management
- **Key Features**:
  - Payment processing with multiple methods
  - Payment status tracking
  - Refund processing
  - Payment history and analytics
  - Fraud detection simulation
  - Integration with order status updates

**Main Endpoints:**

- `POST /api/payment/process` - Process payment
- `GET /api/payment/{id}` - Get payment details
- `GET /api/payment/order/{orderId}` - Get payments for order
- `GET /api/payment/user/{userId}` - Get user payment history
- `POST /api/payment/refund` - Process refund
- `PUT /api/payment/{id}/status` - Update payment status
- `GET /api/payment/statistics` - Get payment statistics

---

## 🔒 **Authentication in Swagger**

All protected endpoints require JWT authentication. To test authenticated endpoints in Swagger:

1. **Get JWT Token**: Use the Auth Service to register/login and get a JWT token
2. **Authorize in Swagger**: Click the "Authorize" button in Swagger UI
3. **Enter Token**: Enter `Bearer <your-jwt-token>` in the authorization field
4. **Test Endpoints**: Now you can test protected endpoints

### **Example Authentication Flow:**

1. **Register User** (Auth Service):

   ```json
   POST /auth/signup
   {
     "username": "testuser",
     "email": "test@example.com",
     "password": "password123"
   }
   ```

2. **Login** (Auth Service):

   ```json
   POST /auth/signin
   {
     "username": "testuser",
     "password": "password123"
   }
   ```

3. **Copy JWT Token** from response and use in other services

---

## 🔧 **API Documentation Features**

### **✅ Comprehensive Documentation Includes:**

- **Detailed endpoint descriptions**
- **Request/response schemas**
- **Authentication requirements**
- **Error response codes**
- **Example requests and responses**
- **Model definitions and validation rules**
- **Security scheme documentation**

### **✅ Interactive Features:**

- **Try it out** functionality for all endpoints
- **Model schema visualization**
- **Response code examples**
- **Authentication token management**
- **Request parameter validation**

---

## 📊 **Testing Workflow Example**

Here's a complete workflow to test the entire platform using Swagger:

### **1. Authentication** (Auth Service)

- Register a new user
- Login to get JWT token
- Validate token

### **2. Product Management** (Product Service)

- Create sample products
- Search and filter products
- Check product availability

### **3. Cart Operations** (Cart Service)

- Add products to cart
- Update quantities
- Check cart TTL status

### **4. Order Processing** (Order Service)

- Create order from cart
- Track order status
- View order history

### **5. Payment Processing** (Payment Service)

- Process payment for order
- Check payment status
- View payment history

---

## 🌟 **Best Practices for API Usage**

### **🔐 Security**

- Always use HTTPS in production
- Store JWT tokens securely
- Implement proper token refresh logic
- Use appropriate authentication headers

### **📝 Error Handling**

- Check HTTP status codes
- Handle validation errors appropriately
- Implement retry logic for transient failures
- Log errors for debugging

### **⚡ Performance**

- Use pagination for large datasets
- Implement caching where appropriate
- Monitor API response times
- Use circuit breakers for resilience

---

**🎯 All services are fully documented and ready for development, testing, and integration!**


