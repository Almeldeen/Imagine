import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { ApiResponse } from '../../../../../core/IApiResponse';
import { IProduct, CreateProductRequestModel } from '../Interface/IProduct';

export interface ProductsListQuery {
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'Asc' | 'Desc';
}

@Injectable({ providedIn: 'root' })
export class ProductService {
  private baseUrl = environment.apiUrl + '/api/products';

  constructor(private http: HttpClient) {}

  getAll(query?: ProductsListQuery): Observable<ApiResponse<IProduct[]>> {
    let params = new HttpParams();

    if (query) {
      if (query.searchTerm) params = params.set('searchTerm', query.searchTerm);
      if (query.pageNumber) params = params.set('pageNumber', query.pageNumber);
      if (query.pageSize) params = params.set('pageSize', query.pageSize);
      if (query.sortBy) params = params.set('sortBy', query.sortBy);
      if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);
    }

    return this.http.get<ApiResponse<IProduct[]>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<ApiResponse<IProduct>> {
    return this.http.get<ApiResponse<IProduct>>(`${this.baseUrl}/${id}`);
  }

  create(form: FormData): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(this.baseUrl, form);
  }

  // Create full product (basic info + colors + images) in a single request
  createFullProduct(model: CreateProductRequestModel, mainImageFile?: File): Observable<ApiResponse<number>> {
    const form = new FormData();

    // Build payload matching CreateProductRequestDto on the backend (without File objects)
    const payload = {
      categoryId: Number(model.categoryId),
      name: model.name,
      description: model.description,
      price: Number(model.price),
      isActive: model.isActive,
      isFeatured: model.isFeatured,
      colors: (model.colors || []).map(color => ({
        colorName: color.colorName,
        colorHex: color.colorHex,
        stock: Number(color.stock),
        additionalPrice: Number(color.additionalPrice),
        isAvailable: color.isAvailable,
        images: (color.images || []).map(img => ({
          fileKey: img.fileKey,
          altText: img.altText,
          isMain: img.isMain,
          displayOrder: img.displayOrder,
        })),
      })),
    };

    form.append('Payload', JSON.stringify(payload));

    if (mainImageFile) {
      form.append('MainImageFile', mainImageFile);
    }

    // Attach all color images using their fileKey as the form field name
    (model.colors || []).forEach(color => {
      (color.images || []).forEach(img => {
        if (img.file && img.fileKey) {
          form.append(img.fileKey, img.file);
        }
      });
    });

    return this.http.post<ApiResponse<number>>(this.baseUrl, form);
  }

  update(id: number, form: FormData): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.baseUrl}/${id}`, form);
  }

  delete(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${id}`);
  }

  // Colors
  addColor(productId: number, payload: any): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.baseUrl}/${productId}/colors`, payload);
  }

  updateColor(colorId: number, payload: any): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.baseUrl}/colors/${colorId}`, payload);
  }

  deleteColor(colorId: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/colors/${colorId}`);
  }

  // Images
  addImage(colorId: number, form: FormData): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.baseUrl}/colors/${colorId}/images`, form);
  }

  deleteImage(imageId: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/images/${imageId}`);
  }

  reorderImages(colorId: number, imageIds: number[]): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/colors/${colorId}/images/reorder`, {
      productColorId: colorId,
      imageIdsInOrder: imageIds,
    });
  }
}
