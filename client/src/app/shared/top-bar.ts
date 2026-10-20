import { Component, computed, inject, isDevMode, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Api } from '../core/api';
import { Auth } from '../core/auth';
import { DevUser } from '../core/models';

@Component({
  selector: 'app-top-bar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './top-bar.html',
})
export class TopBar {
  protected readonly auth = inject(Auth);
  private readonly api = inject(Api);
  private readonly router = inject(Router);

  protected readonly devMode = isDevMode();
  protected readonly devUsers = signal<DevUser[]>([]);
  protected readonly currentKey = computed(() => {
    const me = this.auth.me();
    return this.devUsers().find((u) => u.displayName === me?.displayName)?.key ?? '';
  });

  constructor() {
    if (this.devMode) {
      this.api.devUsers().subscribe((users) => this.devUsers.set(users));
    }
  }

  protected async switchUser(key: string): Promise<void> {
    await this.auth.signIn(key);
    await this.router.navigateByUrl(this.auth.homeUrl());
  }

  protected rolesOf(user: DevUser): string {
    const top = user.roles.includes('Admin') ? 'Admin' : user.roles.includes('Specialist') ? 'Specialist' : 'Requester';
    return user.origin === 'External' ? `${top} · customer` : top;
  }
}
