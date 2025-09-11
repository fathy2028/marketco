export interface Payment {
  paymentId: number;
  orderId: number;
  userId: number;
  amount: number;
  status: PaymentStatus;
  paymentDate: Date;
  paymentMethod?: string;
  transactionId?: string;
  paymentGateway?: string;
  failureReason?: string;
  processedAt?: Date;
  currency?: string;
  notes?: string;
}

export enum PaymentStatus {
  PENDING = 'Pending',
  PROCESSING = 'Processing',
  COMPLETED = 'Completed',
  FAILED = 'Failed',
  CANCELLED = 'Cancelled',
  REFUNDED = 'Refunded'
}

export interface PaymentRequest {
  orderId: number;
  userId: number;
  amount: number;
  paymentMethod: string;
  paymentGateway?: string;
  currency?: string;
  notes?: string;
}

export interface RefundRequest {
  paymentId: number;
  amount?: number;
  reason?: string;
}
