import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from './IApiResponse';

export interface AddToCartRequest {
  userOrSessionId: string;
  productColorId: number;
  quantity: number;
  size?: string;
}

export interface AddCustomProductToCartRequest {
  userOrSessionId: string;
  customProductId: number;
  quantity: number;
  size?: string;
}

export interface CartItemDto {
  id: number;
  name: string;
  image: string;
  color: string;
  size?: string;
  basePrice: number;
  quantity: number;
  isAiPowered: boolean;
}

export interface CartDto {
  id: number;
  userId?: string;
  sessionId?: string;
  expiresAt?: string;
  totalAmount: number;
  totalItems: number;
  items: CartItemDto[];
}

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly baseUrl = environment.apiUrl + '/api/cart';
  private readonly storageKey = 'cartUserOrSessionId';

  private readonly cartSubject = new BehaviorSubject<CartDto | null>(null);
  readonly cart$ = this.cartSubject.asObservable();
  readonly totalItems$ = this.cart$.pipe(
    tap((cart) => {
      // side-effect only used to ensure stream stays hot
    }),
  );

  constructor(private http: HttpClient) {}

  addToCart(productColorId: number, quantity: number = 1, size?: string): Observable<ApiResponse<boolean>> {
    const payload: AddToCartRequest = {
      userOrSessionId: this.getOrCreateUserOrSessionId(),
      productColorId,
      quantity,
      size,
    };
    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/add`, payload).pipe(
      tap(() => {
        this.refreshCart().subscribe();
      })
    );
  }

  addCustomProductToCart(customProductId: number, quantity: number = 1, size?: string): Observable<ApiResponse<boolean>> {
    const payload: AddCustomProductToCartRequest = {
      userOrSessionId: this.getOrCreateUserOrSessionId(),
      customProductId,
      quantity,
      size,
    };

    return this.http.post<ApiResponse<boolean>>(`${this.baseUrl}/add-custom`, payload).pipe(
      tap(() => {
        this.refreshCart().subscribe();
      })
    );
  }

  getCart(): Observable<ApiResponse<CartDto>> {
    const userOrSessionId = this.getOrCreateUserOrSessionId();
    return this.http.get<ApiResponse<CartDto>>(
      `${this.baseUrl}/userOrSessionId`,
      { params: { userOrSessionId } }
    ).pipe(
      tap((res) => {
        this.cartSubject.next(res.data ?? null);
      })
    );
  }

  updateItemQuantity(itemId: number, quantity: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.baseUrl}/update-quantity`, {
      itemId,
      quantity,
    }).pipe(
      tap(() => {
        this.refreshCart().subscribe();
      })
    );
  }

  removeItem(itemId: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/remove/${itemId}`).pipe(
      tap(() => {
        this.refreshCart().subscribe();
      })
    );
  }

  clearCart(): Observable<ApiResponse<boolean>> {
    const userOrSessionId = this.getOrCreateUserOrSessionId();
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/clear/${userOrSessionId}`).pipe(
      tap(() => {
        this.cartSubject.next({
          id: 0,
          userId: undefined,
          sessionId: userOrSessionId,
          expiresAt: undefined,
          totalAmount: 0,
          totalItems: 0,
          items: [],
        });
      })
    );
  }

  getTotalItemsSnapshot(): number {
    const cart = this.cartSubject.value;
    return cart?.totalItems ?? 0;
  }

  refreshCart(): Observable<ApiResponse<CartDto>> {
    return this.getCart();
  }

  getUserOrSessionIdSnapshot(): string {
    return this.getOrCreateUserOrSessionId();
  }

  private getOrCreateUserOrSessionId(): string {
    if (typeof window === 'undefined') {
      // Fallback for SSR or non-browser contexts
      return 'server-session';
    }

    const existing = localStorage.getItem(this.storageKey);
    if (existing) {
      return existing;
    }

    const id = this.generateSessionId();
    localStorage.setItem(this.storageKey, id);
    return id;
  }

  private generateSessionId(): string {
    // Simple guest session id, can be replaced by real auth later
    const random = Math.random().toString(36).substring(2) + Date.now().toString(36);
    return `guest-${random}`;
  }
}
