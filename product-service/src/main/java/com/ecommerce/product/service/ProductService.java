package com.ecommerce.product.service;

import com.ecommerce.product.dto.ProductDto;
import com.ecommerce.product.entity.Product;
import com.ecommerce.product.repository.ProductRepository;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;

@Service
@Transactional
public class ProductService {

    @Autowired
    private ProductRepository productRepository;

    public List<ProductDto> getAllProducts() {
        return productRepository.findAll().stream()
                .map(this::convertToDto)
                .collect(Collectors.toList());
    }

    public Page<ProductDto> getAllProducts(Pageable pageable) {
        return productRepository.findAll(pageable)
                .map(this::convertToDto);
    }

    public Optional<ProductDto> getProductById(Long id) {
        return productRepository.findById(id)
                .map(this::convertToDto);
    }

    public List<ProductDto> getProductsByCategory(String category) {
        return productRepository.findByCategory(category).stream()
                .map(this::convertToDto)
                .collect(Collectors.toList());
    }

    public List<ProductDto> searchProducts(String name) {
        return productRepository.findByNameContaining(name).stream()
                .map(this::convertToDto)
                .collect(Collectors.toList());
    }

    public List<ProductDto> getAvailableProducts() {
        return productRepository.findAvailableProducts().stream()
                .map(this::convertToDto)
                .collect(Collectors.toList());
    }

    public List<String> getCategories() {
        return productRepository.findDistinctCategories();
    }

    public ProductDto createProduct(ProductDto productDto) {
        Product product = convertToEntity(productDto);
        Product savedProduct = productRepository.save(product);
        return convertToDto(savedProduct);
    }

    public ProductDto updateProduct(Long id, ProductDto productDto) {
        return productRepository.findById(id)
                .map(product -> {
                    product.setName(productDto.getName());
                    product.setDescription(productDto.getDescription());
                    product.setPrice(productDto.getPrice());
                    product.setStock(productDto.getStock());
                    product.setImageUrl(productDto.getImageUrl());
                    product.setCategory(productDto.getCategory());
                    return convertToDto(productRepository.save(product));
                })
                .orElse(null);
    }

    public boolean deleteProduct(Long id) {
        if (productRepository.existsById(id)) {
            productRepository.deleteById(id);
            return true;
        }
        return false;
    }

    public boolean reserveStock(Long productId, Integer quantity) {
        return productRepository.findById(productId)
                .map(product -> {
                    if (product.getStock() >= quantity) {
                        product.setStock(product.getStock() - quantity);
                        productRepository.save(product);
                        return true;
                    }
                    return false;
                })
                .orElse(false);
    }

    public boolean releaseStock(Long productId, Integer quantity) {
        return productRepository.findById(productId)
                .map(product -> {
                    product.setStock(product.getStock() + quantity);
                    productRepository.save(product);
                    return true;
                })
                .orElse(false);
    }

    private ProductDto convertToDto(Product product) {
        return new ProductDto(
                product.getProductId(),
                product.getName(),
                product.getDescription(),
                product.getPrice(),
                product.getStock(),
                product.getImageUrl(),
                product.getCategory());
    }

    private Product convertToEntity(ProductDto productDto) {
        Product product = new Product();
        product.setProductId(productDto.getProductId());
        product.setName(productDto.getName());
        product.setDescription(productDto.getDescription());
        product.setPrice(productDto.getPrice());
        product.setStock(productDto.getStock());
        product.setImageUrl(productDto.getImageUrl());
        product.setCategory(productDto.getCategory());
        return product;
    }
}
