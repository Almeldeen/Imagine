import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { ApiResponse } from '../../../../core/IApiResponse';
import { HomeData } from '../Interface/IHome';

@Injectable({ providedIn: 'root' })
export class HomeService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getHomeData(): Observable<ApiResponse<HomeData>> {
    return this.http.get<ApiResponse<HomeData>>(`${this.baseUrl}/api/home`);
  }
}
