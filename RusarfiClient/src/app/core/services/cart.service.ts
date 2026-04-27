import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { ApiResponse } from '../models/api-response';
import {
  CartItemDto,
  CartAddRequest,
  CartUpdateRequest,
  CartRemoveRequest,
  CartSummaryDto,
} from '../models/cart-item.dto';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;
  private readonly _count$ = new BehaviorSubject<number>(0);
  public readonly cartCount$ = this._count$.asObservable();

  getCart(userId: number): Observable<ApiResponse<CartSummaryDto>> {
    return this.http.get<ApiResponse<CartSummaryDto>>(
      `${this.baseUrl}/api/cart/${userId}`
    );
  }

  addProduct(request: CartAddRequest): Observable<ApiResponse<CartItemDto>> {
    return this.http.post<ApiResponse<CartItemDto>>(
      `${this.baseUrl}/api/cart/items`,
      request
    ).pipe(
      tap(() => this.refreshCount(request.userId))
    );
  }

  updateQuantity(request: CartUpdateRequest): Observable<ApiResponse<CartItemDto>> {
    return this.http.put<ApiResponse<CartItemDto>>(
      `${this.baseUrl}/api/cart/items`,
      request
    ).pipe(
      tap(() => this.refreshCount(request.userId))
    );
  }

  removeProduct(request: CartRemoveRequest): Observable<ApiResponse<null>> {
    return this.http.request<ApiResponse<null>>('delete', `${this.baseUrl}/api/cart/items`, {
      body: request,
    }).pipe(
      tap(() => this.refreshCount(request.userId))
    );
  }

  refreshCount(userId: number): void {
    this.getCart(userId).subscribe({
      next: (res) => {
        if (res?.success) {
          const data: any = res.data ?? {};
          const items = data?.items ?? data?.Items ?? [];
          if (Array.isArray(items)) {
            this._count$.next(items.length);
            return;
          }
        }
        this._count$.next(0);
      },
      error: () => this._count$.next(0),
    });
  }
}
