export interface Order {
  id: number;
  orderId: number;
  userId: number;
  orderDate: Date;
  createdAt: Date;
  status: OrderStatus;
  totalAmount: number;
  items: OrderItem[];
  orderItems: OrderItem[];
  shippingAddress?: string;
  billingAddress?: string;
  notes?: string;
}

export interface OrderItem {
  orderItemId: number;
  productId: number;
  quantity: number;
  price: number;
  productName?: string;
  productDescription?: string;
}

export enum OrderStatus {
  CREATED = 'CREATED',
  CONFIRMED = 'CONFIRMED',
  SHIPPED = 'SHIPPED',
  DELIVERED = 'DELIVERED',
  CANCELLED = 'CANCELLED',
  PAID = 'PAID'
}

export interface CreateOrderRequest {
  userId: number;
  orderItems: OrderItem[];
  shippingAddress?: string;
  billingAddress?: string;
  notes?: string;
}

export interface OrderPage {
  content: Order[];
  totalElements: number;
  totalPages: number;
  size: number;
  number: number;
  numberOfElements: number;
  first: boolean;
  last: boolean;
  empty: boolean;
}


