import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Auth } from './core/auth';
import { TopBar } from './shared/top-bar';

@Component({
  imports: [RouterOutlet, TopBar],
  selector: 'app-root',
  template: `
    @if (auth.isSignedIn()) {
      <app-top-bar />
    }
    <router-outlet />
  `,
})
export class App {
  protected readonly auth = inject(Auth);
}
