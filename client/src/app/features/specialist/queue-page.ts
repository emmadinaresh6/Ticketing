import { Component, inject, signal } from '@angular/core';
import { Api } from '../../core/api';
import { Auth } from '../../core/auth';
import { TicketSummary } from '../../core/models';
import { TicketTable } from '../../shared/ticket-table';

// First cut: every ticket assigned to my groups or me. Unassigned/claim tabs and SLA come with epic 5.
@Component({
  selector: 'app-queue-page',
  imports: [TicketTable],
  template: `
    <main class="page">
      <div class="page-head">
        <div>
          <p class="eyebrow">{{ auth.me()?.displayName }} · {{ groupNames() }}</p>
          <h1>Queue</h1>
        </div>
      </div>
      <section class="card" aria-label="Tickets">
        @if (error()) {
          <p class="notice notice-error pad" role="alert">{{ error() }}</p>
        } @else if (loading()) {
          <p class="muted pad">Loading queue…</p>
        } @else {
          <app-ticket-table [tickets]="tickets()" emptyText="Nothing assigned to your groups right now." />
        }
        <p class="muted pad footnote">Only tickets assigned to your groups or to you are shown. Every ticket you open is recorded in the audit log.</p>
      </section>
    </main>
  `,
  styles: `
    .pad { margin: 0; padding: 16px; }
    .footnote { font-size: 13px; }
  `,
})
export class QueuePage {
  private readonly api = inject(Api);
  protected readonly auth = inject(Auth);

  protected readonly tickets = signal<TicketSummary[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');

  constructor() {
    this.api.tickets('Queue').subscribe({
      next: (list) => {
        this.tickets.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load the queue.');
        this.loading.set(false);
      },
    });
  }

  protected groupNames(): string {
    const groups = this.auth.me()?.groups ?? [];
    return groups.length ? groups.map((g) => g.name).join(', ') : 'No groups';
  }
}
