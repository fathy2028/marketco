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
    
    if (!this.authService.isAuthenticated()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Login Required',
        detail: 'Please login to add items to cart'
      });
      this.router.navigate(['/auth/login']);
      return;
    }

    const user = this.authService.getUser();
    if (!user) return;

    this.addingToCart = true;
    
    const request: AddToCartRequest = {
      userId: user.id,
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

  getStockSeverity(): string {
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
}
