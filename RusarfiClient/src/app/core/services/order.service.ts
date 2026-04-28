import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api-response';

@Injectable({ providedIn: 'root' })
export class OrderService {

  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl; // 👈 ESTA ES LA LÍNEA CLAVE

  confirmOrder(request: { userId: number }): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(
      `${this.baseUrl}/api/orders/confirm`,
      request
    );
  }
}