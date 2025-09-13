import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { Cart, CartItem } from '../../models/cart.model';
import { CartService } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.scss']
})
export class CartComponent implements OnInit, OnDestroy {
  cart: Cart | null = null;
  loading = false;
  error: string | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private cartService: CartService,
    private authService: AuthService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {}

  ngOnInit() {
    console.log('Cart component initialized');
    
    this.cartService.cart$
      .pipe(takeUntil(this.destroy$))
      .subscribe(cart => {
        console.log('Cart data received:', cart);
        this.cart = cart;
      });

    this.loadCart();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCart() {
    // Use authenticated user ID or default to 1 for anonymous users
    const userId = this.authService.isAuthenticated() ? 
      (this.authService.getCurrentUser()?.id || 1) : 1;
    
    this.loading = true;
    this.cartService.getCart(userId).subscribe({
      next: (cart) => {
        this.cart = cart;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load cart';
        this.loading = false;
        console.error('Error loading cart:', error);
      }
    });
  }

  updateQuantity(item: CartItem, newQuantity: number) {
    if (newQuantity < 1) {
      this.removeItem(item);
      return;
    }

    const userId = this.authService.isAuthenticated() ? 
      (this.authService.getCurrentUser()?.id || 1) : 1;

    this.cartService.updateCartItem(userId, item.cartItemId, {
      quantity: newQuantity
    }).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Updated',
          detail: 'Cart item updated successfully'
        });
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to update cart item'
        });
        console.error('Error updating cart item:', error);
      }
    });
  }

  removeItem(item: CartItem) {
    this.confirmationService.confirm({
      message: 'Are you sure you want to remove this item from your cart?',
      accept: () => {
        const userId = this.authService.isAuthenticated() ? 
          (this.authService.getCurrentUser()?.id || 1) : 1;

        this.cartService.removeCartItem(userId, item.cartItemId).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Removed',
              detail: 'Item removed from cart'
            });
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to remove item from cart'
            });
            console.error('Error removing cart item:', error);
          }
        });
      }
    });
  }

  clearCart() {
    const userId = this.authService.isAuthenticated() ? 
      (this.authService.getCurrentUser()?.id || 1) : 1;

    this.confirmationService.confirm({
      message: 'Are you sure you want to clear your entire cart?',
      accept: () => {
        this.cartService.clearCart(userId).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Cleared',
              detail: 'Cart cleared successfully'
            });
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to clear cart'
            });
            console.error('Error clearing cart:', error);
          }
        });
      }
    });
  }

  getTotalPrice(): number {
    if (!this.cart || !this.cart.items) return 0;
    return this.cart.items.reduce((total, item) => total + (item.price * item.quantity), 0);
  }

  getTotalItems(): number {
    if (!this.cart || !this.cart.items) return 0;
    return this.cart.items.reduce((total, item) => total + item.quantity, 0);
  }

  getPlaceholderImage(): string {
    // Return a simple SVG placeholder as data URL
    return 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjAwIiBoZWlnaHQ9IjIwMCIgdmlld0JveD0iMCAwIDIwMCAyMDAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIyMDAiIGhlaWdodD0iMjAwIiBmaWxsPSIjRjNGNEY2Ii8+CjxwYXRoIGQ9Ik0xMDAgNzVMMTI1IDEwMEgxMDBWNzVaIiBmaWxsPSIjOUNBM0FGIi8+CjxwYXRoIGQ9Ik0xMDAgMTI1TDEyNSAxMDBIMTAwVjEyNVoiIGZpbGw9IiM5Q0EzQUYiLz4KPHBhdGggZD0iTTEwMCA3NUw3NSAxMDBIMTAwVjc1WiIgZmlsbD0iIzlDQTNBRiIvPgo8cGF0aCBkPSJNMTAwIDEyNUw3NSAxMDBIMTAwVjEyNVoiIGZpbGw9IiM5Q0EzQUYiLz4KPHN2ZyB4PSI4NSIgeT0iODUiIHdpZHRoPSIzMCIgaGVpZ2h0PSIzMCIgdmlld0JveD0iMCAwIDMwIDMwIiBmaWxsPSJub25lIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPgo8Y2lyY2xlIGN4PSIxNSIgY3k9IjE1IiByPSIxMiIgZmlsbD0iI0Q5RURGQyIvPgo8cGF0aCBkPSJNMTUgMTBMMTggMTNIMTVWMTBaIiBmaWxsPSIjOUNBM0FGIi8+CjxwYXRoIGQ9Ik0xNSAyMEwxOCAxN0gxNVYyMFoiIGZpbGw9IiM5Q0EzQUYiLz4KPHBhdGggZD0iTTE1IDEwTDEyIDEzSDE1VjEwWiIgZmlsbD0iIzlDQTNBRiIvPgo8cGF0aCBkPSJNMTUgMjBMMTIgMTdIMTVWMjBaIiBmaWxsPSIjOUNBM0FGIi8+Cjwvc3ZnPgo8L3N2Zz4K';
  }
}


