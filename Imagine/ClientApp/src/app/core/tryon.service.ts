import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from './IApiResponse';

export interface PreprocessResult {
  preprocessedImageUrl: string;
  customizationJobId?: number;
}

export interface GenerateGarmentResult {
  customizationJobId: number;
  deApiRequestId: string;
  generatedGarmentUrl?: string;
}

export interface TryOnJobCreated {
  jobId: string;
  statusUrl?: string;
}

export interface TryOnJobStatus {
  status: string;
  imageUrl?: string;
  imageBase64?: string;
  message?: string;
  error?: string;
  errorCode?: string;
  provider?: string;
}

@Injectable({ providedIn: 'root' })
export class TryOnService {
  private readonly baseUrl = environment.apiUrl + '/api/customization';

  constructor(private http: HttpClient) {}

  preprocessGarment(
    prompt: string,
    garmentType: string,
    file?: File | null,
    baseImageUrl?: string
  ): Observable<ApiResponse<PreprocessResult>> {
    const form = new FormData();
    form.append('prompt', prompt);
    form.append('garmentType', garmentType);

    if (baseImageUrl) {
      form.append('baseImageUrl', baseImageUrl);
    }

    if (file) {
      form.append('file', file);
    }

    return this.http.post<ApiResponse<PreprocessResult>>(`${this.baseUrl}/preprocess`, form);
  }

  startTryOn(personImage: File, customizationJobId: number): Observable<ApiResponse<TryOnJobCreated>> {
    const form = new FormData();
    form.append('personImage', personImage);
    form.append('customizationJobId', customizationJobId.toString());
    return this.http.post<ApiResponse<TryOnJobCreated>>(`${this.baseUrl}/tryon`, form);
  }

  getTryOnStatus(jobId: string): Observable<ApiResponse<TryOnJobStatus>> {
    return this.http.get<ApiResponse<TryOnJobStatus>>(`${this.baseUrl}/tryon/${jobId}`);
  }
}
