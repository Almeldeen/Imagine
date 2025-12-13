import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from './IApiResponse';

export interface SaveCustomizationRequest {
  customizationJobId: number;
  productId?: number;
  notes?: string;
  colorName?: string;
  colorHex?: string;
  size?: string;
}

export interface SavedCustomProduct {
  id: number;
  productId?: number | null;
  customDesignImageUrl?: string | null;
  aiRenderedPreviewUrl?: string | null;
  notes?: string | null;
  estimatedPrice: number;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class CustomProductService {
  private readonly baseUrl = environment.apiUrl + '/api/custom-products';

  constructor(private http: HttpClient) {}

  saveFromCustomization(payload: SaveCustomizationRequest): Observable<ApiResponse<SavedCustomProduct>> {
    return this.http.post<ApiResponse<SavedCustomProduct>>(
      `${this.baseUrl}/save-from-customization`,
      payload
    );
  }
}
