package com.ecommerce.product.messaging;

import com.ecommerce.product.dto.StockReservationRequest;
import com.ecommerce.product.service.ProductService;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.amqp.rabbit.annotation.RabbitListener;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

@Component
public class StockEventListener {

    private static final Logger logger = LoggerFactory.getLogger(StockEventListener.class);

    @Autowired
    private ProductService productService;

    @Autowired
    private ObjectMapper objectMapper;

    @RabbitListener(queues = "product.stock.reserve")
    public void handleStockReservation(String message) {
        try {
            logger.info("Received stock reservation request: {}", message);
            StockReservationRequest request = objectMapper.readValue(message, StockReservationRequest.class);

            boolean success = productService.reserveStock(request.getProductId(), request.getQuantity());

            if (success) {
                logger.info("Stock reserved successfully for product {} quantity {}",
                        request.getProductId(), request.getQuantity());
            } else {
                logger.warn("Failed to reserve stock for product {} quantity {}",
                        request.getProductId(), request.getQuantity());
            }
        } catch (Exception e) {
            logger.error("Error processing stock reservation: {}", e.getMessage(), e);
        }
    }

    @RabbitListener(queues = "product.stock.release")
    public void handleStockRelease(String message) {
        try {
            logger.info("Received stock release request: {}", message);
            StockReservationRequest request = objectMapper.readValue(message, StockReservationRequest.class);

            boolean success = productService.releaseStock(request.getProductId(), request.getQuantity());

            if (success) {
                logger.info("Stock released successfully for product {} quantity {}",
                        request.getProductId(), request.getQuantity());
            } else {
                logger.warn("Failed to release stock for product {} quantity {}",
                        request.getProductId(), request.getQuantity());
            }
        } catch (Exception e) {
            logger.error("Error processing stock release: {}", e.getMessage(), e);
        }
    }
}
