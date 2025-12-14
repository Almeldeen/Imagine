import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ICustomer, ICustomerDetails, ICustomerListResult, IImportCustomersResult } from '../Interface/ICustomer';
import { environment } from '../../../../../../environments/environment';
import { ApiResponse } from '../../../../../core/IApiResponse';

export interface PasswordResetResult {
  emailSent: boolean;
}

export interface CustomerFilterParams {
  search?: string;
  role?: string;
  status?: string;
  pageNumber?: number;
  pageSize?: number;
  sort?: string;
}

@Injectable({ providedIn: 'root' })
export class AdminCustomersService {
  private readonly baseUrl = environment.apiUrl + '/api/admin/customers';

  constructor(private http: HttpClient) {}

  getAll(params?: CustomerFilterParams): Observable<ApiResponse<ICustomerListResult>> {
    let httpParams = new HttpParams();

    if (params) {
      if (params.search) httpParams = httpParams.set('search', params.search);
      if (params.role) httpParams = httpParams.set('role', params.role);
      if (params.status) httpParams = httpParams.set('status', params.status);
      if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber);
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize);
      if (params.sort) httpParams = httpParams.set('sort', params.sort);
    }

    return this.http.get<ApiResponse<ICustomerListResult>>(this.baseUrl, { params: httpParams });
  }

  getById(id: string): Observable<ApiResponse<ICustomerDetails>> {
    return this.http.get<ApiResponse<ICustomerDetails>>(`${this.baseUrl}/${id}`);
  }

  update(id: string, form: FormData): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.baseUrl}/${id}`, form);
  }

  resetPassword(id: string): Observable<ApiResponse<PasswordResetResult>> {
    return this.http.post<ApiResponse<PasswordResetResult>>(`${this.baseUrl}/${id}/reset-password`, {});
  }

  export(params?: CustomerFilterParams): Observable<Blob> {
    let httpParams = new HttpParams();

    if (params) {
      if (params.search) httpParams = httpParams.set('search', params.search);
      if (params.role) httpParams = httpParams.set('role', params.role);
      if (params.status) httpParams = httpParams.set('status', params.status);
      if (params.sort) httpParams = httpParams.set('sort', params.sort);
    }

    return this.http.get(`${this.baseUrl}/export`, {
      params: httpParams,
      responseType: 'blob',
    });
  }

  import(file: File): Observable<ApiResponse<IImportCustomersResult>> {
    const form = new FormData();
    form.append('file', file);

    return this.http.post<ApiResponse<IImportCustomersResult>>(
      `${this.baseUrl}/import`,
      form
    );
  }
}
