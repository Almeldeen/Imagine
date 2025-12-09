import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from './IApiResponse';

export interface LoginRequest {
  // Identifier can be email or phone number
  identifier: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  userId: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  roles: string[];
  profileImageUrl?: string | null;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  phoneNumber?: string;
  profileImageFile?: File | null;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = environment.apiUrl + '/api/Auth';
  private readonly tokenKey = 'authToken';
  private readonly fullNameKey = 'userFullName';
  private readonly userIdKey = 'userId';
  private readonly emailKey = 'userEmail';
  private readonly rolesKey = 'userRoles';
  private readonly profileImageKey = 'profileImage';

  constructor(private http: HttpClient) {}

  login(payload: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http
      .post<ApiResponse<LoginResponse>>(`${this.baseUrl}/login`, payload)
      .pipe(
        tap((res) => {
          if (res.success && res.data) {
            this.setSession(res.data);
          }
        }),
      );
  }

  updateProfileImage(file: File): Observable<ApiResponse<string>> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http
      .post<ApiResponse<string>>(`${this.baseUrl}/profile-image`, formData)
      .pipe(
        tap((res) => {
          if (res.success && res.data) {
            this.setProfileImageUrl(res.data);
          }
        }),
      );
  }

  register(payload: RegisterRequest): Observable<ApiResponse<string>> {
    const formData = new FormData();
    formData.append('FullName', payload.fullName);
    formData.append('Email', payload.email);
    formData.append('PhoneNumber', payload.phoneNumber ?? '');
    formData.append('Password', payload.password);
    formData.append('ConfirmPassword', payload.confirmPassword);

    if (payload.profileImageFile) {
      formData.append('ProfileImageFile', payload.profileImageFile);
    }

    return this.http.post<ApiResponse<string>>(`${this.baseUrl}/register`, formData);
  }

  private setSession(user: LoginResponse): void {
    if (typeof window === 'undefined') {
      return;
    }

    localStorage.setItem(this.tokenKey, user.token);
    localStorage.setItem(this.fullNameKey, user.fullName);
    localStorage.setItem(this.userIdKey, user.userId);
    localStorage.setItem(this.emailKey, user.email);
    localStorage.setItem(this.rolesKey, JSON.stringify(user.roles ?? []));

    this.setProfileImageUrl(user.profileImageUrl);
  }

  private setProfileImageUrl(url: string | null | undefined): void {
    if (typeof window === 'undefined') {
      return;
    }

    const value = url && url.trim() ? url : '/assets/images/hero-banner.png';
    localStorage.setItem(this.profileImageKey, value);
  }

  clearSession(): void {
    if (typeof window === 'undefined') {
      return;
    }

    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.fullNameKey);
    localStorage.removeItem(this.userIdKey);
    localStorage.removeItem(this.emailKey);
    localStorage.removeItem(this.rolesKey);
    localStorage.removeItem(this.profileImageKey);
  }

  getToken(): string | null {
    if (typeof window === 'undefined') {
      return null;
    }
    return localStorage.getItem(this.tokenKey);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getFullName(): string | null {
    if (typeof window === 'undefined') {
      return null;
    }
    return localStorage.getItem(this.fullNameKey);
  }

  getProfileImageUrl(): string {
    if (typeof window === 'undefined') {
      return '/assets/images/hero-banner.png';
    }

    const stored = localStorage.getItem(this.profileImageKey);
    if (!stored || !stored.trim()) {
      return '/assets/images/hero-banner.png';
    }

    return stored;
  }

  getUserRoles(): string[] {
    if (typeof window === 'undefined') {
      return [];
    }

    const raw = localStorage.getItem(this.rolesKey);
    if (!raw) {
      return [];
    }

    try {
      const parsed = JSON.parse(raw);
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }

  hasRole(role: string): boolean {
    const roles = this.getUserRoles();
    return roles.some(r => r.toLowerCase() === role.toLowerCase());
  }
}
