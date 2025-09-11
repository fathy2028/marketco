import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Payment, PaymentRequest, PaymentStatus, RefundRequest } from '../models/payment.model';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private readonly API_URL = 'http://localhost:8080/api/payment';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  processPayment(request: PaymentRequest): Observable<Payment> {
    return this.http.post<Payment>(`${this.API_URL}/process`, request, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getPayment(id: number): Observable<Payment> {
    return this.http.get<Payment>(`${this.API_URL}/${id}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getPaymentsByOrder(orderId: number): Observable<Payment[]> {
    return this.http.get<Payment[]>(`${this.API_URL}/order/${orderId}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getPaymentsByUser(userId: number): Observable<Payment[]> {
    return this.http.get<Payment[]>(`${this.API_URL}/user/${userId}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getPaymentsByStatus(status: PaymentStatus): Observable<Payment[]> {
    return this.http.get<Payment[]>(`${this.API_URL}/status/${status}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  updatePaymentStatus(id: number, status: PaymentStatus, reason?: string): Observable<Payment> {
    let params = new HttpParams().set('status', status);
    if (reason) {
      params = params.set('reason', reason);
    }
    
    return this.http.put<Payment>(`${this.API_URL}/${id}/status`, null, { 
      params,
      headers: this.authService.getAuthHeaders()
    });
  }

  refundPayment(request: RefundRequest): Observable<Payment> {
    return this.http.post<Payment>(`${this.API_URL}/refund`, request, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getPaymentStatistics(): Observable<any> {
    return this.http.get(`${this.API_URL}/statistics`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getPaymentHistory(userId: number, page: number = 0, size: number = 10): Observable<Payment[]> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('size', size.toString());
    
    return this.http.get<Payment[]>(`${this.API_URL}/history/${userId}`, { 
      params,
      headers: this.authService.getAuthHeaders()
    });
  }
}
