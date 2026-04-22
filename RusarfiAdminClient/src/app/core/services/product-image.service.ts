import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response';
import { ProductImageResponse } from '../models/admin-product.models';

@Injectable({ providedIn: 'root' })
export class ProductImageService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  uploadImage(file: File) {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<ApiResponse<ProductImageResponse>>(
      `${this.baseUrl}/api/products/image`,
      formData
    );
  }
}
