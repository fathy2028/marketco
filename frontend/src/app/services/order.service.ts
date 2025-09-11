import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order, OrderPage, CreateOrderRequest, OrderStatus } from '../models/order.model';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private readonly API_URL = 'http://localhost:8080/api/orders';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getAllOrders(page: number = 0, size: number = 10): Observable<OrderPage> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('size', size.toString());
    
    return this.http.get<OrderPage>(this.API_URL, { 
      params,
      headers: this.authService.getAuthHeaders()
    });
  }

  getAllOrdersList(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.API_URL}/all`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getOrderById(id: number): Observable<Order> {
    return this.http.get<Order>(`${this.API_URL}/${id}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getOrdersByUser(userId: number): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.API_URL}/user/${userId}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getOrdersByStatus(status: OrderStatus): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.API_URL}/status/${status}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getOrdersByDateRange(startDate: Date, endDate: Date): Observable<Order[]> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());
    
    return this.http.get<Order[]>(`${this.API_URL}/date-range`, { 
      params,
      headers: this.authService.getAuthHeaders()
    });
  }

  getOrderCountByStatus(status: OrderStatus): Observable<number> {
    return this.http.get<number>(`${this.API_URL}/count/${status}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  createOrder(request: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.API_URL, request, {
      headers: this.authService.getAuthHeaders()
    });
  }

  updateOrderStatus(id: number, status: OrderStatus): Observable<Order> {
    const params = new HttpParams().set('status', status);
    return this.http.put<Order>(`${this.API_URL}/${id}/status`, null, { 
      params,
      headers: this.authService.getAuthHeaders()
    });
  }

  cancelOrder(id: number): Observable<string> {
    return this.http.delete<string>(`${this.API_URL}/${id}`, {
      headers: this.authService.getAuthHeaders(),
      responseType: 'text' as 'json'
    });
  }

  confirmOrder(id: number): Observable<Order> {
    return this.http.post<Order>(`${this.API_URL}/${id}/confirm`, null, {
      headers: this.authService.getAuthHeaders()
    });
  }

  shipOrder(id: number): Observable<Order> {
    return this.http.post<Order>(`${this.API_URL}/${id}/ship`, null, {
      headers: this.authService.getAuthHeaders()
    });
  }

  deliverOrder(id: number): Observable<Order> {
    return this.http.post<Order>(`${this.API_URL}/${id}/deliver`, null, {
      headers: this.authService.getAuthHeaders()
    });
  }
}


