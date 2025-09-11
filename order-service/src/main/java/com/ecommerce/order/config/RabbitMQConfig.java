package com.ecommerce.order.config;

import org.springframework.amqp.core.*;
import org.springframework.amqp.rabbit.connection.ConnectionFactory;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.amqp.support.converter.Jackson2JsonMessageConverter;
import org.springframework.amqp.support.converter.MessageConverter;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class RabbitMQConfig {

    // Exchanges
    public static final String ORDER_EXCHANGE = "order.exchange";
    public static final String PRODUCT_EXCHANGE = "product.exchange";

    // Queues
    public static final String ORDER_STATUS_QUEUE = "order.status";
    public static final String ORDER_STATUS_CHANGED_QUEUE = "order.status.changed";
    public static final String PAYMENT_STATUS_QUEUE = "order.payment.status";

    // Routing Keys
    public static final String ORDER_STATUS_ROUTING_KEY = "order.status";
    public static final String ORDER_STATUS_CHANGED_ROUTING_KEY = "order.status.changed";
    public static final String PAYMENT_STATUS_ROUTING_KEY = "order.payment.status";

    @Bean
    public DirectExchange orderExchange() {
        return new DirectExchange(ORDER_EXCHANGE);
    }

    @Bean
    public Queue orderStatusQueue() {
        return QueueBuilder.durable(ORDER_STATUS_QUEUE).build();
    }

    @Bean
    public Queue orderStatusChangedQueue() {
        return QueueBuilder.durable(ORDER_STATUS_CHANGED_QUEUE).build();
    }

    @Bean
    public Queue paymentStatusQueue() {
        return QueueBuilder.durable(PAYMENT_STATUS_QUEUE).build();
    }

    @Bean
    public Binding orderStatusBinding() {
        return BindingBuilder
                .bind(orderStatusQueue())
                .to(orderExchange())
                .with(ORDER_STATUS_ROUTING_KEY);
    }

    @Bean
    public Binding orderStatusChangedBinding() {
        return BindingBuilder
                .bind(orderStatusChangedQueue())
                .to(orderExchange())
                .with(ORDER_STATUS_CHANGED_ROUTING_KEY);
    }

    @Bean
    public Binding paymentStatusBinding() {
        return BindingBuilder
                .bind(paymentStatusQueue())
                .to(orderExchange())
                .with(PAYMENT_STATUS_ROUTING_KEY);
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
