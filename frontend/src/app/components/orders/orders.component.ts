import { Component, OnInit } from '@angular/core';
import { Order } from '../../models/order.model';
import { OrderService } from '../../services/order.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-orders',
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.scss']
})
export class OrdersComponent implements OnInit {
  orders: Order[] = [];
  loading = false;
  error: string | null = null;

  constructor(
    private orderService: OrderService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.loadOrders();
  }

  loadOrders() {
    const currentUser = this.authService.getCurrentUser();
    if (currentUser) {
      this.loading = true;
      this.orderService.getUserOrders(currentUser.id).subscribe({
        next: (orders) => {
          this.orders = orders;
          this.loading = false;
        },
        error: (error) => {
          this.error = 'Failed to load orders';
          this.loading = false;
          console.error('Error loading orders:', error);
        }
      });
    }
  }

  getStatusSeverity(status: string): 'success' | 'info' | 'warning' | 'danger' | 'secondary' | 'contrast' {
    switch (status.toLowerCase()) {
      case 'pending': return 'warning';
      case 'confirmed': return 'info';
      case 'shipped': return 'success';
      case 'delivered': return 'success';
      case 'cancelled': return 'danger';
      default: return 'secondary';
    }
  }
}

