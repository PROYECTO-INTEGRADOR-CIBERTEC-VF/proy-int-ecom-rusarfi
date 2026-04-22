import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response';
import { CategoryOption } from '../models/category.models';

@Injectable({ providedIn: 'root' })
export class AdminCategoryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  getCategories() {
    return this.http.get<ApiResponse<CategoryOption[]>>(
      `${this.baseUrl}/api/admin/categories`
    );
  }
}
