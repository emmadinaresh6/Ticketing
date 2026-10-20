import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { catchError, firstValueFrom, throwError } from 'rxjs';
import { Api } from './api';
import { Me, RoleName } from './models';

const TOKEN_KEY = 'supportdesk.token';

// Dev login for now. Entra ID / Entra External ID (MSAL) replaces signIn later;
// the rest of the app only uses me(), hasRole() and the bearer token.
@Injectable({ providedIn: 'root' })
export class Auth {
  private readonly api = inject(Api);
  private readonly router = inject(Router);

  readonly token = signal<string | null>(readToken());
  readonly me = signal<Me | null>(null);
  readonly isSignedIn = computed(() => this.me() !== null);
  readonly isStaff = computed(() => this.hasRole('Specialist') || this.hasRole('Admin'));

  hasRole(role: RoleName): boolean {
    return this.me()?.roles.includes(role) ?? false;
  }

  homeUrl(): string {
    if (this.hasRole('Admin')) return '/admin';
    if (this.hasRole('Specialist')) return '/queue';
    return '/my-tickets';
  }

  // Called once at startup: keeps the session across page reloads while the token is still valid.
  async restore(): Promise<void> {
    if (!this.token()) return;
    try {
      this.me.set(await firstValueFrom(this.api.me()));
    } catch {
      this.clear();
    }
  }

  async signIn(devUser: string): Promise<void> {
    const login = await firstValueFrom(this.api.devLogin(devUser));
    this.setToken(login.accessToken);
    this.me.set(await firstValueFrom(this.api.me()));
  }

  signOut(): void {
    this.clear();
    void this.router.navigateByUrl('/login');
  }

  private clear(): void {
    this.setToken(null);
    this.me.set(null);
  }

  private setToken(token: string | null): void {
    this.token.set(token);
    try {
      if (token) sessionStorage.setItem(TOKEN_KEY, token);
      else sessionStorage.removeItem(TOKEN_KEY);
    } catch {
      // Storage unavailable (private mode): the session just won't survive a reload.
    }
  }
}

function readToken(): string | null {
  try {
    return sessionStorage.getItem(TOKEN_KEY);
  } catch {
    return null;
  }
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(Auth);
  const token = auth.token();
  const request = token && req.url.startsWith('/api/')
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(request).pipe(
    catchError((error: unknown) => {
      // Expired token (dev tokens also expire when the API restarts): back to sign-in.
      if (error instanceof HttpErrorResponse && error.status === 401 && !req.url.startsWith('/api/dev/')) {
        auth.signOut();
      }
      return throwError(() => error);
    }),
  );
};

// canMatch keeps the code for screens a user may not use from even loading.
// The API still enforces every rule; these guards only shape navigation.
export const signedIn: CanMatchFn = () => {
  const auth = inject(Auth);
  return auth.isSignedIn() || inject(Router).createUrlTree(['/login']);
};

export const hasAnyRole = (...roles: RoleName[]): CanMatchFn => () => {
  const auth = inject(Auth);
  const router = inject(Router);
  if (!auth.isSignedIn()) return router.createUrlTree(['/login']);
  return roles.some((r) => auth.hasRole(r)) || router.createUrlTree([auth.homeUrl()]);
};

export const redirectHome: CanMatchFn = () => {
  const auth = inject(Auth);
  return inject(Router).createUrlTree([auth.isSignedIn() ? auth.homeUrl() : '/login']);
};
