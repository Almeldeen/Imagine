import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { ICategory } from '../Interface/ICategory';
import { ApiResponse } from '../../../../../core/IApiResponse';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private baseUrl = environment.apiUrl + '/api/categories';

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<ICategory[]>> {
    return this.http.get<ApiResponse<ICategory[]>>(this.baseUrl);
  }

  getById(id: number): Observable<ApiResponse<ICategory>> {
    return this.http.get<ApiResponse<ICategory>>(`${this.baseUrl}/${id}`);
  }

  create(form: FormData): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(this.baseUrl, form);
  }

  update(id: number, form: FormData): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.baseUrl}/${id}`, form);
  }

  delete(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${id}`);
  }
}
