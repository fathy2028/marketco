export interface CartItem {
  cartItemId: string;
  userId: number;
  productId: number;
  quantity: number;
  price: number;
  productName?: string;
  productDescription?: string;
  imageUrl?: string;
  createdAt?: Date;
  updatedAt?: Date;
  expiryTime: Date;
  ttlType: CartTtlType;
}

export interface Cart {
  userId: number;
  items: CartItem[];
  totalAmount: number;
  totalItems: number;
  lastUpdated: Date;
  ttlType: CartTtlType;
}

export enum CartTtlType {
  Default = 'Default',
  OrderPlaced = 'OrderPlaced',
  PaymentCompleted = 'PaymentCompleted'
}

export interface AddToCartRequest {
  userId: number;
  productId: number;
  quantity: number;
  price: number;
  productName?: string;
  productDescription?: string;
  imageUrl?: string;
}

export interface UpdateCartItemRequest {
  quantity: number;
}

export interface CartTtlUpdateRequest {
  userId: number;
  ttlType: CartTtlType;
}
