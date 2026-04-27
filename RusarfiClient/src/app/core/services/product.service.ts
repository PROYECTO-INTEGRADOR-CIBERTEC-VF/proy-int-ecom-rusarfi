import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ProductsResponse, ProductResponse, ProductDto } from '../../features/products/products.models';
import { ApiResponse } from '../models/api-response';


@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  getAvailableProducts(params?: { search?: string; category?: string }) {
    let httpParams = new HttpParams();

    if (params?.search) {
      httpParams = httpParams.set('search', params.search);
    }

    if (params?.category) {
      httpParams = httpParams.set('category', params.category);
    }

    return this.http.get<ProductsResponse>(`${this.baseUrl}/api/products`, {
      params: httpParams,
    });
  }

  getProductById(id: number) {
    return this.http.get<ProductResponse>(`${this.baseUrl}/api/products/${id}`);
  }

}
