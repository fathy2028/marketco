import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Cart, CartItem, AddToCartRequest, UpdateCartItemRequest, CartTtlUpdateRequest } from '../models/cart.model';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private readonly API_URL = 'http://localhost:5001/api/cart';
  
  private cartSubject = new BehaviorSubject<Cart | null>(null);
  public cart$ = this.cartSubject.asObservable();

  private cartItemCountSubject = new BehaviorSubject<number>(0);
  public cartItemCount$ = this.cartItemCountSubject.asObservable();

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {
    // Load cart for both authenticated and anonymous users
    this.authService.isAuthenticated$.subscribe(isAuth => {
      this.loadUserCart();
    });
    
    // Also load cart on service initialization
    this.loadUserCart();
  }

  private loadUserCart(): void {
    // Use authenticated user ID or default to 1 for anonymous users
    const userId = this.authService.isAuthenticated() ? 
      (this.authService.getUser()?.id || 1) : 1;
    
    this.getCart(userId).subscribe({
      next: (cart) => {
        this.cartSubject.next(cart);
        this.cartItemCountSubject.next(cart.totalItems);
      },
      error: (error) => {
        console.error('Error loading cart:', error);
        this.cartSubject.next(null);
        this.cartItemCountSubject.next(0);
      }
    });
  }

  getCart(userId: number): Observable<Cart> {
    return this.http.get<Cart>(`${this.API_URL}/${userId}`);
  }

  addToCart(request: AddToCartRequest): Observable<CartItem> {
    return this.http.post<CartItem>(`${this.API_URL}/add`, request).pipe(
      tap(() => this.loadUserCart())
    );
  }

  updateCartItem(userId: number, cartItemId: string, request: UpdateCartItemRequest): Observable<CartItem> {
    return this.http.put<CartItem>(`${this.API_URL}/${userId}/items/${cartItemId}`, request).pipe(
      tap(() => this.loadUserCart())
    );
  }

  removeCartItem(userId: number, cartItemId: string): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${userId}/items/${cartItemId}`).pipe(
      tap(() => this.loadUserCart())
    );
  }

  clearCart(userId: number): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${userId}`).pipe(
      tap(() => this.clearLocalCart())
    );
  }

  updateCartTtl(request: CartTtlUpdateRequest): Observable<any> {
    return this.http.post(`${this.API_URL}/${request.userId}/ttl`, request);
  }

  getCartStatistics(): Observable<any> {
    return this.http.get(`${this.API_URL}/statistics`);
  }

  checkoutCart(userId: number): Observable<any> {
    return this.http.post(`${this.API_URL}/${userId}/checkout`, null);
  }

  private clearLocalCart(): void {
    this.cartSubject.next(null);
    this.cartItemCountSubject.next(0);
  }

  getCurrentCart(): Cart | null {
    return this.cartSubject.value;
  }

  getCartItemCount(): number {
    return this.cartItemCountSubject.value;
  }
}


