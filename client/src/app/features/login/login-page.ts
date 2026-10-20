import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Api } from '../../core/api';
import { Auth } from '../../core/auth';
import { DevUser } from '../../core/models';

// Development sign-in with the seeded users. Replaced by Entra ID sign-in later.
@Component({
  selector: 'app-login-page',
  template: `
    <main class="page login">
      <h1>Support Desk</h1>
      <p class="muted">Development sign-in. Pick a seeded user to continue.</p>

      @if (error()) {
        <p class="notice notice-error" role="alert">{{ error() }}</p>
      }

      <ul class="users">
        @for (u of users(); track u.key) {
          <li>
            <button type="button" class="card user" [disabled]="busy()" (click)="signIn(u.key)">
              <span class="name">{{ u.displayName }}</span>
              <span class="muted">{{ u.roles.join(', ') }} · {{ u.origin === 'External' ? 'External customer' : 'Internal staff' }}</span>
            </button>
          </li>
        } @empty {
          @if (!error()) {
            <li class="muted">Loading users…</li>
          }
        }
      </ul>
    </main>
  `,
  styles: `
    .login { max-width: 560px; }
    h1 { margin-bottom: 4px; }
    .users { list-style: none; margin: 24px 0 0; padding: 0; display: flex; flex-direction: column; gap: 12px; }
    .user {
      width: 100%; min-height: 64px; padding: 14px 18px; font: inherit; text-align: left; cursor: pointer;
      display: flex; flex-direction: column; gap: 2px; color: var(--ink);
    }
    .user:hover { border-color: var(--accent); }
    .name { font-weight: 600; }
  `,
})
export class LoginPage {
  private readonly api = inject(Api);
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  protected readonly users = signal<DevUser[]>([]);
  protected readonly busy = signal(false);
  protected readonly error = signal('');

  constructor() {
    this.api.devUsers().subscribe({
      next: (users) => this.users.set(users),
      error: () => this.error.set('Cannot reach the API. Start it with: dotnet run --project src/SupportDesk.Api'),
    });
  }

  protected async signIn(key: string): Promise<void> {
    this.busy.set(true);
    this.error.set('');
    try {
      await this.auth.signIn(key);
      await this.router.navigateByUrl(this.auth.homeUrl());
    } catch {
      this.error.set('Sign-in failed. Check that the API is running.');
    } finally {
      this.busy.set(false);
    }
  }
}
