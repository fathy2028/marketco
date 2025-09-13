import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

// PrimeNG Modules
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { BadgeModule } from 'primeng/badge';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';

import { OrdersComponent } from './orders.component';

const routes = [
  { path: '', component: OrdersComponent }
];

@NgModule({
  declarations: [
    OrdersComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    RouterModule.forChild(routes),
    CardModule,
    ButtonModule,
    BadgeModule,
    DividerModule,
    MessageModule,
    ProgressSpinnerModule,
    TableModule,
    TagModule
  ]
})
export class OrdersModule { }


