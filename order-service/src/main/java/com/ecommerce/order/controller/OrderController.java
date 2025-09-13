package com.ecommerce.order.controller;

import com.ecommerce.order.dto.CreateOrderRequest;
import com.ecommerce.order.dto.OrderDto;
import com.ecommerce.order.entity.Order;
import com.ecommerce.order.service.OrderService;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import jakarta.validation.Valid;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.format.annotation.DateTimeFormat;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDateTime;
import java.util.List;

@RestController
@RequestMapping("/orders")
@Tag(name = "Orders", description = "Order Management API")
public class OrderController {

    @Autowired
    private OrderService orderService;

    @GetMapping
    @Operation(summary = "Get all orders", description = "Retrieve all orders with optional pagination")
    public ResponseEntity<Page<OrderDto>> getAllOrders(
            @RequestParam(defaultValue = "0") int page,
            @RequestParam(defaultValue = "10") int size) {
        Pageable pageable = PageRequest.of(page, size);
        Page<OrderDto> orders = orderService.getAllOrders(pageable);
        return ResponseEntity.ok(orders);
    }

    @GetMapping("/all")
    @Operation(summary = "Get all orders without pagination", description = "Retrieve all orders as a list")
    public ResponseEntity<List<OrderDto>> getAllOrdersList() {
        List<OrderDto> orders = orderService.getAllOrders();
        return ResponseEntity.ok(orders);
    }

    @GetMapping("/{id}")
    @Operation(summary = "Get order by ID", description = "Retrieve a specific order by its ID")
    public ResponseEntity<OrderDto> getOrderById(@PathVariable Long id) {
        return orderService.getOrderById(id)
                .map(order -> ResponseEntity.ok(order))
                .orElse(ResponseEntity.notFound().build());
    }

    @GetMapping("/user/{userId}")
    @Operation(summary = "Get orders by user ID", description = "Retrieve all orders for a specific user")
    public ResponseEntity<List<OrderDto>> getOrdersByUserId(@PathVariable Long userId) {
        List<OrderDto> orders = orderService.getOrdersByUserId(userId);
        return ResponseEntity.ok(orders);
    }

    @GetMapping("/status/{status}")
    @Operation(summary = "Get orders by status", description = "Retrieve orders by their status")
    public ResponseEntity<List<OrderDto>> getOrdersByStatus(@PathVariable Order.OrderStatus status) {
        List<OrderDto> orders = orderService.getOrdersByStatus(status);
        return ResponseEntity.ok(orders);
    }

    @GetMapping("/date-range")
    @Operation(summary = "Get orders by date range", description = "Retrieve orders within a specific date range")
    public ResponseEntity<List<OrderDto>> getOrdersByDateRange(
            @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime startDate,
            @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime endDate) {
        List<OrderDto> orders = orderService.getOrdersBetweenDates(startDate, endDate);
        return ResponseEntity.ok(orders);
    }

    @GetMapping("/count/{status}")
    @Operation(summary = "Get order count by status", description = "Get the count of orders with a specific status")
    public ResponseEntity<Long> getOrderCountByStatus(@PathVariable Order.OrderStatus status) {
        Long count = orderService.getOrderCountByStatus(status);
        return ResponseEntity.ok(count);
    }

    @PostMapping
    @Operation(summary = "Create order", description = "Create a new order")
    public ResponseEntity<OrderDto> createOrder(@Valid @RequestBody CreateOrderRequest request) {
        OrderDto createdOrder = orderService.createOrder(request);
        return ResponseEntity.status(HttpStatus.CREATED).body(createdOrder);
    }

    @PutMapping("/{id}/status")
    @Operation(summary = "Update order status", description = "Update the status of an existing order")
    public ResponseEntity<OrderDto> updateOrderStatus(@PathVariable Long id,
            @RequestParam Order.OrderStatus status) {
        OrderDto updatedOrder = orderService.updateOrderStatus(id, status);
        if (updatedOrder != null) {
            return ResponseEntity.ok(updatedOrder);
        }
        return ResponseEntity.notFound().build();
    }

    @DeleteMapping("/{id}")
    @Operation(summary = "Cancel order", description = "Cancel an order (only if in CREATED or CONFIRMED status)")
    public ResponseEntity<String> cancelOrder(@PathVariable Long id) {
        if (orderService.cancelOrder(id)) {
            return ResponseEntity.ok("Order cancelled successfully");
        }
        return ResponseEntity.badRequest().body("Order cannot be cancelled or not found");
    }

    @PostMapping("/{id}/confirm")
    @Operation(summary = "Confirm order", description = "Confirm an order (change status to CONFIRMED)")
    public ResponseEntity<OrderDto> confirmOrder(@PathVariable Long id) {
        OrderDto updatedOrder = orderService.updateOrderStatus(id, Order.OrderStatus.CONFIRMED);
        if (updatedOrder != null) {
            return ResponseEntity.ok(updatedOrder);
        }
        return ResponseEntity.notFound().build();
    }

    @PostMapping("/{id}/ship")
    @Operation(summary = "Ship order", description = "Mark order as shipped")
    public ResponseEntity<OrderDto> shipOrder(@PathVariable Long id) {
        OrderDto updatedOrder = orderService.updateOrderStatus(id, Order.OrderStatus.SHIPPED);
        if (updatedOrder != null) {
            return ResponseEntity.ok(updatedOrder);
        }
        return ResponseEntity.notFound().build();
    }

    @PostMapping("/{id}/deliver")
    @Operation(summary = "Deliver order", description = "Mark order as delivered")
    public ResponseEntity<OrderDto> deliverOrder(@PathVariable Long id) {
        OrderDto updatedOrder = orderService.updateOrderStatus(id, Order.OrderStatus.DELIVERED);
        if (updatedOrder != null) {
            return ResponseEntity.ok(updatedOrder);
        }
        return ResponseEntity.notFound().build();
    }
}
