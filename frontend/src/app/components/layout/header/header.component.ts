import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { MenuItem } from 'primeng/api';
import { AuthService } from '../../../services/auth.service';
import { CartService } from '../../../services/cart.service';
import { User } from '../../../models/user.model';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit, OnDestroy {
  items: MenuItem[] = [];
  currentUser: User | null = null;
  isAuthenticated = false;
  cartItemCount = 0;
  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private cartService: CartService,
    private router: Router
  ) {}

  ngOnInit() {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
        this.updateMenuItems();
      });

    this.authService.isAuthenticated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(isAuth => {
        this.isAuthenticated = isAuth;
        this.updateMenuItems();
      });

    this.cartService.cartItemCount$
      .pipe(takeUntil(this.destroy$))
      .subscribe(count => {
        this.cartItemCount = count;
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateMenuItems() {
    this.items = [
      {
        label: 'Products',
        icon: 'pi pi-shopping-bag',
        routerLink: '/products'
      }
    ];

    if (this.isAuthenticated) {
      this.items.push(
        {
          label: 'Orders',
          icon: 'pi pi-list',
          routerLink: '/orders'
        },
        {
          label: 'Profile',
          icon: 'pi pi-user',
          routerLink: '/profile'
        }
      );

      if (this.currentUser?.role === 'ADMIN' || this.currentUser?.role === 'ROLE_ADMIN') {
        this.items.push({
          label: 'Admin',
          icon: 'pi pi-cog',
          routerLink: '/admin'
        });
      }
    }
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }

  goToCart() {
    if (this.isAuthenticated) {
      this.router.navigate(['/cart']);
    } else {
      this.router.navigate(['/auth/login']);
    }
  }

  goToLogin() {
    this.router.navigate(['/auth/login']);
  }
}
