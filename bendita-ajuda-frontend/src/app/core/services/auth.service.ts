import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  AuthCredentials,
  LoginResponse,
  MessageResponse,
  RegisterRequest,
  ResetPasswordRequest,
  AccessTokenPayload,
  GoogleLoginRequest,
  UserRole,
} from '../../pages/auth/models/auth.models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly tokenKey = 'accessToken';
  private readonly authUrl = environment.authApiBaseUrl;
  private readonly roleClaim =
    'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

  constructor(private readonly http: HttpClient) {}

  register(payload: RegisterRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.authUrl}register`, payload);
  }

  registerPrestador(payload: RegisterRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.authUrl}register-prestador`, payload);
  }

  login(payload: AuthCredentials): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.authUrl}login`, payload).pipe(
      tap((response) => {
        localStorage.setItem(this.tokenKey, response.accessToken);
      }),
    );
  }

  loginWithGoogle(payload: GoogleLoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.authUrl}google-login`, payload).pipe(
      tap((response) => {
        localStorage.setItem(this.tokenKey, response.accessToken);
      }),
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
  }

  forgotPassword(email: string): Observable<string> {
    return this.http.post(`${this.authUrl}forgot-password`, { email }, { responseType: 'text' });
  }

  resetPassword(payload: ResetPasswordRequest): Observable<string> {
    return this.http.post(`${this.authUrl}reset-password`, payload, { responseType: 'text' });
  }

  confirmEmail(email: string, token: string): Observable<string> {
    return this.http.get(`${this.authUrl}confirm-email`, {
      params: { email, token },
      responseType: 'text',
    });
  }

  me(): Observable<unknown> {
    return this.http.get<unknown>(`${this.authUrl}me`);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isAuthenticated(): boolean {
    const payload = this.getAccessTokenPayload();

    if (!payload) {
      return false;
    }

    if (payload.exp && payload.exp * 1000 <= Date.now()) {
      this.logout();
      return false;
    }

    return true;
  }

  getCurrentRole(): UserRole | null {
    const payload = this.getAccessTokenPayload();
    const roleValue = payload?.[this.roleClaim] || payload?.role || payload?.roles;
    const role = Array.isArray(roleValue) ? roleValue[0] : roleValue;

    return typeof role === 'string' ? this.normalizeRole(role) : null;
  }

  hasAnyRole(allowedRoles: UserRole[]): boolean {
    const currentRole = this.getCurrentRole();
    return !!currentRole && allowedRoles.includes(currentRole);
  }

  getDefaultRouteForCurrentUser(): string {
    const role = this.getCurrentRole();

    switch (role) {
      case 'Admin':
        return '/admin';
      case 'Operador':
        return '/operador';
      case 'Prestador':
        return '/prestador/home';
      case 'Cliente':
        return '/cliente';
      default:
        return '/login';
    }
  }

  private getAccessTokenPayload(): AccessTokenPayload | null {
    const token = this.getToken();

    if (!token) {
      return null;
    }

    const [, payload] = token.split('.');

    if (!payload) {
      return null;
    }

    try {
      const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
      const json = decodeURIComponent(
        atob(base64)
          .split('')
          .map((char) => `%${char.charCodeAt(0).toString(16).padStart(2, '0')}`)
          .join(''),
      );

      return JSON.parse(json) as AccessTokenPayload;
    } catch {
      return null;
    }
  }

  private normalizeRole(role: string | undefined): UserRole | null {
    switch (role?.toLowerCase()) {
      case 'admin':
        return 'Admin';
      case 'operador':
        return 'Operador';
      case 'prestador':
        return 'Prestador';
      case 'cliente':
        return 'Cliente';
      default:
        return null;
    }
  }
}
