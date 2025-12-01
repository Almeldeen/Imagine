import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderHeader } from './Components/order-header/order-header';
import { OrderList } from './Components/order-list/order-list';
import { OrderEmptyState } from './Components/order-empty-state/order-empty-state';

// Order Status Enum
export enum OrderStatus {
  Pending = 0,
  Processing = 1,
  Shipped = 2,
  Delivered = 3,
  Cancelled = 4,
  Refunded = 5
}

// Custom Product Status Enum
export enum CustomProductStatus {
  Draft = 0,
  Submitted = 1,
  InReview = 2,
  Approved = 3,
  InProduction = 4,
  Completed = 5,
  Rejected = 6
}

// Order Interfaces based on C# entities
export interface Order {
  id: number;
  userId?: string;
  orderNumber: string;
  orderDate: Date;
  subTotal: number;
  shippingCost: number;
  tax: number;
  totalAmount: number;
  status: OrderStatus;
  
  // Shipping Information
  shippingAddress: string;
  shippingCity: string;
  shippingPostalCode: string;
  shippingCountry: string;
  shippingPhone?: string;
  
  // Payment Information
  paymentMethod?: string;
  paymentTransactionId?: string;
  paidAt?: Date;
  
  // Tracking
  trackingNumber?: string;
  shippedAt?: Date;
  deliveredAt?: Date;
  
  // Relations
  user?: any; // ApplicationUser
  orderItems: OrderItem[];
}

export interface OrderItem {
  id: number;
  orderId: number;
  productColorId?: number;
  customProductId?: number;
  productName: string;
  colorName?: string;
  productImageUrl?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  
  // Relations
  order: Order;
  productColor?: any;
  customProduct?: CustomProduct;
}

export interface CustomProduct {
  id: number;
  userId?: string;
  productId?: number;
  customDesignImageUrl?: string;
  aiRenderedPreviewUrl?: string;
  notes?: string;
  estimatedPrice: number;
  status: CustomProductStatus;
  adminNotes?: string;
  
  // Relations
  user?: any;
  product?: any;
  customColors: CustomProductColor[];
}

export interface CustomProductColor {
  id: number;
  customProductId: number;
  colorName: string;
  colorHex?: string;
  imageUrl?: string;
  
  // Relations
  customProduct: CustomProduct;
}

@Component({
  selector: 'app-orders',
  imports: [CommonModule, OrderHeader, OrderList, OrderEmptyState],
  templateUrl: './orders.html',
  styleUrl: './orders.css',
})
export class Orders {
  hasOrders = true; // Toggle this to show empty state

  onFilterChange(filter: string) {
    console.log('Filter changed:', filter);
    // Handle filter change
  }

  onViewChange(view: string) {
    console.log('View changed:', view);
    // Handle view change
  }

  onSortChange(sort: string) {
    console.log('Sort changed:', sort);
    // Handle sort change
  }

  onSearchChange(search: string) {
    console.log('Search changed:', search);
    // Handle search change
  }

  onDateRangeChange(dateRange: any) {
    console.log('Date range changed:', dateRange);
    // Handle date range change
  }
}
