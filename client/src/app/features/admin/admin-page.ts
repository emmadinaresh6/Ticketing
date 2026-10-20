import { Component, inject, signal } from '@angular/core';
import { Api } from '../../core/api';
import { TicketSummary } from '../../core/models';
import { TicketTable } from '../../shared/ticket-table';

// First cut: all tickets. Dashboard, users, groups, categories/routing and audit search come with epic 7.
@Component({
  selector: 'app-admin-page',
  imports: [TicketTable],
  template: `
    <main class="page">
      <div class="page-head">
        <div>
          <p class="eyebrow">Admin</p>
          <h1>All tickets</h1>
        </div>
      </div>
      <section class="card" aria-label="All tickets">
        @if (error()) {
          <p class="notice notice-error pad" role="alert">{{ error() }}</p>
        } @else if (loading()) {
          <p class="muted pad">Loading tickets…</p>
        } @else {
          <app-ticket-table [tickets]="tickets()" emptyText="No tickets yet." />
        }
      </section>
    </main>
  `,
  styles: `.pad { margin: 0; padding: 16px; }`,
})
export class AdminPage {
  private readonly api = inject(Api);

  protected readonly tickets = signal<TicketSummary[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');

  constructor() {
    this.api.tickets('All').subscribe({
      next: (list) => {
        this.tickets.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load tickets.');
        this.loading.set(false);
      },
    });
  }
}
