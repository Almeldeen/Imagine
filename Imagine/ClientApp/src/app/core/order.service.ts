import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from './IApiResponse';

export interface CreateOrderRequest {
  fullName: string;
  phoneNumber: string;
  address: string;
  city: string;
  notes?: string;
  cartUserOrSessionId: string;
  grandTotal: number;
}

export interface OrderCreatedResponse {
  orderId: number;
  orderNumber: string;
  totalAmount: number;
}

export interface UserOrderSummary {
  id: number;
  orderNumber: string;
  orderDate: string;
  status: string;
  totalAmount: number;
  itemsCount: number;
  thumbnailUrl?: string;
}

export interface AdminOrderItem {
  productName: string;
  colorName?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  productImageUrl?: string;
}

export interface AdminOrder {
  orderId: number;
  orderNumber: string;
  orderDate: string;
  totalAmount: number;
  subTotal: number;
  shippingCost: number;
  tax: number;
  status: string;
  paymentStatus: string;
  paymentMethod?: string;
  userId?: string;
  userName?: string;
  userEmail?: string;
  userPhoneNumber?: string;
  shippingAddress: string;
  shippingCity: string;
  shippingPostalCode: string;
  shippingCountry: string;
  shippingPhone?: string;
  trackingNumber?: string;
  paidAt?: string;
  shippedAt?: string;
  deliveredAt?: string;
  items: AdminOrderItem[];
}

export interface AdminOrdersQuery {
  pageNumber?: number;
  pageSize?: number;
  sortKey?: string;      // 'date-desc', 'date-asc', 'amount-desc', 'amount-asc', 'status', 'customer'
  status?: string;       // 'all', 'pending', 'processing', 'shipped', 'delivered', 'cancelled', 'refunded'
  searchTerm?: string;   // free text
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly baseUrl = environment.apiUrl + '/api/Orders';

  constructor(private http: HttpClient) {}

  createOrder(payload: CreateOrderRequest): Observable<ApiResponse<OrderCreatedResponse>> {
    return this.http.post<ApiResponse<OrderCreatedResponse>>(`${this.baseUrl}/create`, payload);
  }

  getMyOrders(): Observable<ApiResponse<UserOrderSummary[]>> {
    return this.http.get<ApiResponse<UserOrderSummary[]>>(`${this.baseUrl}/mine`);
  }

  getAllOrdersForAdmin(query?: AdminOrdersQuery): Observable<ApiResponse<AdminOrder[]>> {
    let params = new HttpParams();

    if (query) {
      if (query.pageNumber) params = params.set('pageNumber', query.pageNumber);
      if (query.pageSize) params = params.set('pageSize', query.pageSize);
      if (query.sortKey) params = params.set('sortKey', query.sortKey);
      if (query.status) params = params.set('status', query.status);
      if (query.searchTerm) params = params.set('searchTerm', query.searchTerm);
    }

    return this.http.get<ApiResponse<AdminOrder[]>>(`${this.baseUrl}/admin`, { params });
  }

  updateOrderStatus(orderId: number, payload: { status: string; trackingNumber?: string }): Observable<ApiResponse<AdminOrder>> {
    return this.http.put<ApiResponse<AdminOrder>>(
      `${this.baseUrl}/${orderId}/status`,
      {
        status: payload.status,
        trackingNumber: payload.trackingNumber,
      }
    );
  }

  getOrderById(orderId: number): Observable<ApiResponse<AdminOrder>> {
    return this.http.get<ApiResponse<AdminOrder>>(`${this.baseUrl}/${orderId}`);
  }
}
