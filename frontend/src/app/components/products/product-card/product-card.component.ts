import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { Product } from '../../../models/product.model';
import { AuthService } from '../../../services/auth.service';
import { CartService } from '../../../services/cart.service';
import { AddToCartRequest } from '../../../models/cart.model';

@Component({
  selector: 'app-product-card',
  templateUrl: './product-card.component.html',
  styleUrls: ['./product-card.component.scss']
})
export class ProductCardComponent {
  @Input() product!: Product;
  @Input() loading = false;
  @Output() productClick = new EventEmitter<Product>();

  addingToCart = false;

  constructor(
    private router: Router,
    private authService: AuthService,
    private cartService: CartService,
    private messageService: MessageService
  ) {}

  onProductClick() {
    this.productClick.emit(this.product);
    this.router.navigate(['/products', this.product.productId]);
  }

  addToCart(event: Event) {
    event.stopPropagation();
    
    // Use a default user ID for anonymous users (you can implement session-based cart later)
    const userId = this.authService.isAuthenticated() ? 
      (this.authService.getUser()?.id || 1) : 1;

    this.addingToCart = true;
    
    const request: AddToCartRequest = {
      userId: userId,
      productId: this.product.productId,
      quantity: 1,
      price: this.product.price,
      productName: this.product.name,
      productDescription: this.product.description,
      imageUrl: this.product.imageUrl
    };

    this.cartService.addToCart(request).subscribe({
      next: () => {
        this.addingToCart = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Added to Cart',
          detail: `${this.product.name} added to cart successfully`
        });
      },
      error: (error) => {
        this.addingToCart = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.message || 'Failed to add item to cart'
        });
      }
    });
  }

  getStockSeverity(): 'success' | 'info' | 'warning' | 'danger' | 'secondary' | 'contrast' {
    if (this.product.stock === 0) return 'danger';
    if (this.product.stock < 10) return 'warning';
    return 'success';
  }

  getStockText(): string {
    if (this.product.stock === 0) return 'Out of Stock';
    if (this.product.stock < 10) return `Only ${this.product.stock} left`;
    return 'In Stock';
  }

  isOutOfStock(): boolean {
    return this.product.stock === 0;
  }

  getPlaceholderImage(): string {
    // Return a simple SVG placeholder as data URL
    return 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjAwIiBoZWlnaHQ9IjIwMCIgdmlld0JveD0iMCAwIDIwMCAyMDAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIyMDAiIGhlaWdodD0iMjAwIiBmaWxsPSIjRjNGNEY2Ii8+CjxwYXRoIGQ9Ik0xMDAgNzVMMTI1IDEwMEgxMDBWNzVaIiBmaWxsPSIjOUNBM0FGIi8+CjxwYXRoIGQ9Ik0xMDAgMTI1TDEyNSAxMDBIMTAwVjEyNVoiIGZpbGw9IiM5Q0EzQUYiLz4KPHBhdGggZD0iTTEwMCA3NUw3NSAxMDBIMTAwVjc1WiIgZmlsbD0iIzlDQTNBRiIvPgo8cGF0aCBkPSJNMTAwIDEyNUw3NSAxMDBIMTAwVjEyNVoiIGZpbGw9IiM5Q0EzQUYiLz4KPHN2ZyB4PSI4NSIgeT0iODUiIHdpZHRoPSIzMCIgaGVpZ2h0PSIzMCIgdmlld0JveD0iMCAwIDMwIDMwIiBmaWxsPSJub25lIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPgo8Y2lyY2xlIGN4PSIxNSIgY3k9IjE1IiByPSIxMiIgZmlsbD0iI0Q5RURGQyIvPgo8cGF0aCBkPSJNMTUgMTBMMTggMTNIMTVWMTBaIiBmaWxsPSIjOUNBM0FGIi8+CjxwYXRoIGQ9Ik0xNSAyMEwxOCAxN0gxNVYyMFoiIGZpbGw9IiM5Q0EzQUYiLz4KPHBhdGggZD0iTTE1IDEwTDEyIDEzSDE1VjEwWiIgZmlsbD0iIzlDQTNBRiIvPgo8cGF0aCBkPSJNMTUgMjBMMTIgMTdIMTVWMjBaIiBmaWxsPSIjOUNBM0FGIi8+Cjwvc3ZnPgo8L3N2Zz4K';
  }
}
