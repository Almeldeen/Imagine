import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from './IApiResponse';

export interface AdminDashboardStats {
  totalRevenue: number;
  totalOrders: number;
  pendingOrders: number;
  completedOrders: number;
  totalProducts: number;
  totalCustomers: number;
}

@Injectable({ providedIn: 'root' })
export class AdminDashboardService {
  private readonly baseUrl = environment.apiUrl + '/api/admin/Dashboard';

  constructor(private http: HttpClient) {}

  getStats(): Observable<ApiResponse<AdminDashboardStats>> {
    return this.http.get<ApiResponse<AdminDashboardStats>>(`${this.baseUrl}/stats`);
  }
}
