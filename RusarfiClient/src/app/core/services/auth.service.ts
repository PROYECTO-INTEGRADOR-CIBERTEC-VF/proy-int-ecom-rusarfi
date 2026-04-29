import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response';
import { UserDto } from '../../features/auth/auth.models';

interface AuthResponse {
  token: string;
  expiresAtUtc: string | Date;
  user: UserDto;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  private readonly _currentUser = new BehaviorSubject<UserDto | null>(this.loadUserFromStorage());
  public readonly currentUser$ = this._currentUser.asObservable();
  public readonly isAuthenticated$ = this.currentUser$.pipe(map((u) => !!u));

  register(payload: { name: string; email: string; password: string; ConfirmPassword: string }) {
    return this.http.post<ApiResponse<UserDto>>(`${this.baseUrl}/api/auth/register`, payload);
  }

  private loadUserFromStorage(): UserDto | null {
    try {
      const raw = localStorage.getItem('current_user');
      if (!raw) return null;
      return JSON.parse(raw) as UserDto;
    } catch {
      return null;
    }
  }

  private saveUserToStorage(user: UserDto | null) {
    try {
      if (user) localStorage.setItem('current_user', JSON.stringify(user));
      else localStorage.removeItem('current_user');
    } catch {}
  }

  private saveToken(token: string | null) {
    try {
      if (token) localStorage.setItem('auth_token', token);
      else localStorage.removeItem('auth_token');
    } catch {}
  }

  getToken(): string | null {
    try {
      return localStorage.getItem('auth_token');
    } catch {
      return null;
    }
  }

  getUserId(): number | null {
    return this._currentUser.value?.id ?? null;
  }

  login(payload: { email: string; password: string }): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.baseUrl}/api/auth/login`, payload).pipe(
      tap((res) => {
        if (res?.success && res.data) {
          const auth = res.data;
          this.saveToken(auth.token);
          this.saveUserToStorage(auth.user);
          this._currentUser.next(auth.user);
        }
      })
    );
  }

  logout(): void {
    this.saveToken(null);
    this.saveUserToStorage(null);
    try { this._currentUser.next(null); } catch {}
  }
}
