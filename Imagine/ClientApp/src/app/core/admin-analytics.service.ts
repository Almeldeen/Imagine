import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from './IApiResponse';

export interface SalesDataPoint {
  label: string;
  periodStart: string;
  totalRevenue: number;
  totalOrders: number;
  averageOrderValue: number;
}

export interface SalesOverview {
  points: SalesDataPoint[];
  totalRevenue: number;
  totalOrders: number;
}

@Injectable({ providedIn: 'root' })
export class AdminAnalyticsService {
  private readonly baseUrl = environment.apiUrl + '/api/admin/Analytics';

  constructor(private http: HttpClient) {}

  getSalesOverview(period: 'month' | 'year' = 'month'): Observable<ApiResponse<SalesOverview>> {
    let params = new HttpParams().set('period', period);
    return this.http.get<ApiResponse<SalesOverview>>(`${this.baseUrl}/sales-overview`, { params });
  }
}
