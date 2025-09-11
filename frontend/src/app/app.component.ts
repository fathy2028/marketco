import { Component, OnInit } from '@angular/core';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  template: `
    <div class="app-container">
      <app-header></app-header>
      <main class="main-content">
        <router-outlet></router-outlet>
      </main>
      <app-footer></app-footer>
      <p-toast position="top-right"></p-toast>
    </div>
  `,
  styles: [`
    .app-container {
      min-height: 100vh;
      display: flex;
      flex-direction: column;
    }
    
    .main-content {
      flex: 1;
      padding-top: 1rem;
    }
  `]
})
export class AppComponent implements OnInit {
  title = 'E-commerce Platform';

  constructor(private authService: AuthService) {}

  ngOnInit() {
    // Initialize authentication state
  }
}