package com.ecommerce.order.service;

import com.ecommerce.order.dto.CreateOrderRequest;
import com.ecommerce.order.dto.OrderDto;
import com.ecommerce.order.dto.OrderItemDto;
import com.ecommerce.order.entity.Order;
import com.ecommerce.order.entity.OrderItem;
import com.ecommerce.order.repository.OrderRepository;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;

@Service
@Transactional
public class OrderService {

    private static final Logger logger = LoggerFactory.getLogger(OrderService.class);

    @Autowired
    private OrderRepository orderRepository;

    @Autowired
    private RabbitTemplate rabbitTemplate;

    @Autowired
    private ObjectMapper objectMapper;

    public List<OrderDto> getAllOrders() {
        return orderRepository.findAll().stream()
                .map(this::convertToDto)
                .collect(Collectors.toList());
    }

    public Page<OrderDto> getAllOrders(Pageable pageable) {
        return orderRepository.findAll(pageable)
                .map(this::convertToDto);
    }

    public Optional<OrderDto> getOrderById(Long id) {
        return orderRepository.findById(id)
                .map(this::convertToDto);
    }

    public List<OrderDto> getOrdersByUserId(Long userId) {
        return orderRepository.findByUserId(userId).stream()
                .map(this::convertToDto)
                .collect(Collectors.toList());
    }

    public List<OrderDto> getOrdersByStatus(Order.OrderStatus status) {
        return orderRepository.findByStatus(status).stream()
                .map(this::convertToDto)
                .collect(Collectors.toList());
    }

    public OrderDto createOrder(CreateOrderRequest request) {
        // Calculate total amount
        BigDecimal totalAmount = request.getOrderItems().stream()
                .map(item -> item.getPrice().multiply(BigDecimal.valueOf(item.getQuantity())))
                .reduce(BigDecimal.ZERO, BigDecimal::add);

        // Create order
        Order order = new Order(request.getUserId(), totalAmount);
        order.setShippingAddress(request.getShippingAddress());
        order.setBillingAddress(request.getBillingAddress());
        order.setNotes(request.getNotes());

        // Create order items
        for (OrderItemDto itemDto : request.getOrderItems()) {
            OrderItem orderItem = new OrderItem(order, itemDto.getProductId(),
                    itemDto.getQuantity(), itemDto.getPrice());
            orderItem.setProductName(itemDto.getProductName());
            orderItem.setProductDescription(itemDto.getProductDescription());
            order.getOrderItems().add(orderItem);
        }

        Order savedOrder = orderRepository.save(order);

        // Publish order created event
        publishOrderEvent("order.created", savedOrder);

        // Reserve stock for products
        reserveStockForOrder(savedOrder);

        logger.info("Order created with ID: {}", savedOrder.getOrderId());
        return convertToDto(savedOrder);
    }

    public OrderDto updateOrderStatus(Long orderId, Order.OrderStatus newStatus) {
        return orderRepository.findById(orderId)
                .map(order -> {
                    Order.OrderStatus oldStatus = order.getStatus();
                    order.setStatus(newStatus);
                    Order savedOrder = orderRepository.save(order);

                    // Publish status change event
                    publishOrderStatusChangeEvent(savedOrder, oldStatus, newStatus);

                    logger.info("Order {} status changed from {} to {}", orderId, oldStatus, newStatus);
                    return convertToDto(savedOrder);
                })
                .orElse(null);
    }

    public boolean cancelOrder(Long orderId) {
        return orderRepository.findById(orderId)
                .map(order -> {
                    if (order.getStatus() == Order.OrderStatus.CREATED ||
                            order.getStatus() == Order.OrderStatus.CONFIRMED) {
                        order.setStatus(Order.OrderStatus.CANCELLED);
                        orderRepository.save(order);

                        // Publish cancellation event
                        publishOrderEvent("order.cancelled", order);

                        // Release reserved stock
                        releaseStockForOrder(order);

                        logger.info("Order {} cancelled", orderId);
                        return true;
                    }
                    return false;
                })
                .orElse(false);
    }

    public List<OrderDto> getOrdersBetweenDates(LocalDateTime startDate, LocalDateTime endDate) {
        return orderRepository.findOrdersBetweenDates(startDate, endDate).stream()
                .map(this::convertToDto)
                .collect(Collectors.toList());
    }

    public Long getOrderCountByStatus(Order.OrderStatus status) {
        return orderRepository.countByStatus(status);
    }

    private void reserveStockForOrder(Order order) {
        for (OrderItem item : order.getOrderItems()) {
            try {
                String stockReservationMessage = objectMapper.writeValueAsString(
                        new StockReservationMessage(item.getProductId(), item.getQuantity(),
                                order.getOrderId().toString()));
                rabbitTemplate.convertAndSend("product.exchange", "product.stock.reserve", stockReservationMessage);
                logger.info("Stock reservation requested for product {} quantity {}",
                        item.getProductId(), item.getQuantity());
            } catch (Exception e) {
                logger.error("Failed to send stock reservation message", e);
            }
        }
    }

    private void releaseStockForOrder(Order order) {
        for (OrderItem item : order.getOrderItems()) {
            try {
                String stockReleaseMessage = objectMapper.writeValueAsString(
                        new StockReservationMessage(item.getProductId(), item.getQuantity(),
                                order.getOrderId().toString()));
                rabbitTemplate.convertAndSend("product.exchange", "product.stock.release", stockReleaseMessage);
                logger.info("Stock release requested for product {} quantity {}",
                        item.getProductId(), item.getQuantity());
            } catch (Exception e) {
                logger.error("Failed to send stock release message", e);
            }
        }
    }

    private void publishOrderEvent(String eventType, Order order) {
        try {
            String orderMessage = objectMapper.writeValueAsString(
                    new OrderEventMessage(eventType, order.getOrderId(), order.getUserId(),
                            order.getStatus().toString(), order.getTotalAmount()));
            rabbitTemplate.convertAndSend("order.exchange", "order.status", orderMessage);
            logger.info("Published order event: {}", eventType);
        } catch (Exception e) {
            logger.error("Failed to publish order event", e);
        }
    }

    private void publishOrderStatusChangeEvent(Order order, Order.OrderStatus oldStatus, Order.OrderStatus newStatus) {
        try {
            String statusChangeMessage = objectMapper.writeValueAsString(
                    new OrderStatusChangeMessage(order.getOrderId(), order.getUserId(),
                            oldStatus.toString(), newStatus.toString()));
            rabbitTemplate.convertAndSend("order.exchange", "order.status.changed", statusChangeMessage);
            logger.info("Published order status change event for order {}", order.getOrderId());
        } catch (Exception e) {
            logger.error("Failed to publish status change event", e);
        }
    }

    private OrderDto convertToDto(Order order) {
        OrderDto dto = new OrderDto(
                order.getOrderId(),
                order.getUserId(),
                order.getOrderDate(),
                order.getStatus(),
                order.getTotalAmount(),
                order.getShippingAddress(),
                order.getBillingAddress(),
                order.getNotes());

        List<OrderItemDto> itemDtos = order.getOrderItems().stream()
                .map(item -> new OrderItemDto(
                        item.getOrderItemId(),
                        item.getProductId(),
                        item.getQuantity(),
                        item.getPrice(),
                        item.getProductName(),
                        item.getProductDescription()))
                .collect(Collectors.toList());

        dto.setOrderItems(itemDtos);
        return dto;
    }

    // Helper classes for messaging
    public static class StockReservationMessage {
        private Long productId;
        private Integer quantity;
        private String orderId;

        public StockReservationMessage(Long productId, Integer quantity, String orderId) {
            this.productId = productId;
            this.quantity = quantity;
            this.orderId = orderId;
        }

        // Getters and setters
        public Long getProductId() {
            return productId;
        }

        public void setProductId(Long productId) {
            this.productId = productId;
        }

        public Integer getQuantity() {
            return quantity;
        }

        public void setQuantity(Integer quantity) {
            this.quantity = quantity;
        }

        public String getOrderId() {
            return orderId;
        }

        public void setOrderId(String orderId) {
            this.orderId = orderId;
        }
    }

    public static class OrderEventMessage {
        private String eventType;
        private Long orderId;
        private Long userId;
        private String status;
        private BigDecimal totalAmount;

        public OrderEventMessage(String eventType, Long orderId, Long userId, String status, BigDecimal totalAmount) {
            this.eventType = eventType;
            this.orderId = orderId;
            this.userId = userId;
            this.status = status;
            this.totalAmount = totalAmount;
        }

        // Getters and setters
        public String getEventType() {
            return eventType;
        }

        public void setEventType(String eventType) {
            this.eventType = eventType;
        }

        public Long getOrderId() {
            return orderId;
        }

        public void setOrderId(Long orderId) {
            this.orderId = orderId;
        }

        public Long getUserId() {
            return userId;
        }

        public void setUserId(Long userId) {
            this.userId = userId;
        }

        public String getStatus() {
            return status;
        }

        public void setStatus(String status) {
            this.status = status;
        }

        public BigDecimal getTotalAmount() {
            return totalAmount;
        }

        public void setTotalAmount(BigDecimal totalAmount) {
            this.totalAmount = totalAmount;
        }
    }

    public static class OrderStatusChangeMessage {
        private Long orderId;
        private Long userId;
        private String oldStatus;
        private String newStatus;

        public OrderStatusChangeMessage(Long orderId, Long userId, String oldStatus, String newStatus) {
            this.orderId = orderId;
            this.userId = userId;
            this.oldStatus = oldStatus;
            this.newStatus = newStatus;
        }

        // Getters and setters
        public Long getOrderId() {
            return orderId;
        }

        public void setOrderId(Long orderId) {
            this.orderId = orderId;
        }

        public Long getUserId() {
            return userId;
        }

        public void setUserId(Long userId) {
            this.userId = userId;
        }

        public String getOldStatus() {
            return oldStatus;
        }

        public void setOldStatus(String oldStatus) {
            this.oldStatus = oldStatus;
        }

        public String getNewStatus() {
            return newStatus;
        }

        public void setNewStatus(String newStatus) {
            this.newStatus = newStatus;
        }
    }
}


