import { environment } from './../../../../../../environments/environment';
import { Observable } from 'rxjs';
import { ICategory } from '../Interface/ICategory';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { IApiResponse } from '../Interface/IApiResponse';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private baseUrl = environment.apiUrl + '/api/categories';

  constructor(private http: HttpClient) {}

  getAll(): Observable<IApiResponse<ICategory[]>> {
    return this.http.get<IApiResponse<ICategory[]>>(this.baseUrl);
  }

  // 🔹 GET: Category by ID
  getById(id: number): Observable<ICategory> {
    return this.http.get<ICategory>(`${this.baseUrl}/${id}`);
  }

  // 🔹 POST: Create Category
  create(data: any): Observable<ICategory> {
    return this.http.post<ICategory>(`${this.baseUrl}/create`, data);
  }

  // 🔹 PUT: Update Category
  update(id: number, category: any): Observable<ICategory> {
    return this.http.put<ICategory>(`${this.baseUrl}/${id}`, category);
  }

  // 🔹 DELETE: Remove Category
   delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
   }
}
