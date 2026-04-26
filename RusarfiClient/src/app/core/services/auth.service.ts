import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { RegisterRequest, RegisterResponse } from '../../features/auth/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  register(payload: RegisterRequest) {
    return this.http.post<RegisterResponse>(`${this.baseUrl}/api/auth/register`, payload);
  }

  login(payload: { email: string; password: string }) {
  return this.http.post<any>(`${this.baseUrl}/api/auth/login`, payload);
}
}
