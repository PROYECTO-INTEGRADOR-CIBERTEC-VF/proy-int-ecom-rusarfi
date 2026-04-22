import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
  AdminProduct,
  CreateProductRequest,
  UpdateProductRequest
} from '../models/admin-product.models';
import { ApiResponse } from '../models/api-response';

@Injectable({ providedIn: 'root' })
export class AdminProductService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  getProducts(params?: { search?: string; category?: string }) {
    let httpParams = new HttpParams();

    if (params?.search) {
      httpParams = httpParams.set('search', params.search);
    }

    if (params?.category) {
      httpParams = httpParams.set('category', params.category);
    }

    return this.http.get<ApiResponse<AdminProduct[]>>(
      `${this.baseUrl}/api/admin/products`,
      { params: httpParams }
    );
  }

  getProduct(id: number) {
    return this.http.get<ApiResponse<AdminProduct>>(
      `${this.baseUrl}/api/admin/products/${id}`
    );
  }

  createProduct(payload: CreateProductRequest) {
    return this.http.post<ApiResponse<AdminProduct>>(
      `${this.baseUrl}/api/admin/products`,
      payload
    );
  }

  updateProduct(id: number, payload: UpdateProductRequest) {
    return this.http.put<ApiResponse<AdminProduct>>(
      `${this.baseUrl}/api/admin/products/${id}`,
      payload
    );
  }

  deleteProduct(id: number) {
    return this.http.delete<ApiResponse<unknown>>(
      `${this.baseUrl}/api/admin/products/${id}`
    );
  }
}
