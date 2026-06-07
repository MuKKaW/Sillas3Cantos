import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { LoginRequest, LoginResponse } from '../models/api.models';

export interface SessionState {
  token: string;
  role: string;
  expiresAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenStorageKey = 's3c_front_token';
  private readonly roleStorageKey = 's3c_front_role';
  private readonly expiresStorageKey = 's3c_front_expires';

  private readonly sessionSignal = signal<SessionState | null>(this.readSessionFromStorage());

  readonly session = this.sessionSignal.asReadonly();
  readonly isAuthenticated = computed(() => {
    const session = this.sessionSignal();
    if (!session) {
      return false;
    }

    const expirationMs = Date.parse(session.expiresAtUtc);
    if (Number.isNaN(expirationMs)) {
      return true;
    }

    return expirationMs > Date.now();
  });
  readonly role = computed(() => this.sessionSignal()?.role ?? '');
  readonly isSuperAdmin = computed(() => this.role().toLowerCase() === 'superadmin');

  login(payload: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${API_BASE_URL}/auth/login`, payload).pipe(
      tap((response) => {
        this.persistSession({
          token: response.accessToken,
          role: response.role,
          expiresAtUtc: response.expiresAtUtc
        });
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenStorageKey);
    localStorage.removeItem(this.roleStorageKey);
    localStorage.removeItem(this.expiresStorageKey);
    this.sessionSignal.set(null);
  }

  getToken(): string {
    return this.sessionSignal()?.token ?? '';
  }

  private persistSession(session: SessionState): void {
    localStorage.setItem(this.tokenStorageKey, session.token);
    localStorage.setItem(this.roleStorageKey, session.role);
    localStorage.setItem(this.expiresStorageKey, session.expiresAtUtc);
    this.sessionSignal.set(session);
  }

  private readSessionFromStorage(): SessionState | null {
    const token = localStorage.getItem(this.tokenStorageKey);
    const role = localStorage.getItem(this.roleStorageKey);
    const expiresAtUtc = localStorage.getItem(this.expiresStorageKey);

    if (!token || !role || !expiresAtUtc) {
      return null;
    }

    return {
      token,
      role,
      expiresAtUtc
    };
  }
}
