import { Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Api } from '../../core/api';
import { Auth } from '../../core/auth';
import { OPEN_STATUSES, TicketSummary } from '../../core/models';
import { TicketTable } from '../../shared/ticket-table';
import { RaiseTicketDialog } from './raise-ticket-dialog';

type View = 'open' | 'all' | 'done';

@Component({
  selector: 'app-my-tickets-page',
  imports: [TicketTable, RaiseTicketDialog],
  template: `
    <main class="page">
      <div class="page-head">
        <div>
          <p class="eyebrow">
            Signed in as {{ auth.me()?.displayName }} ·
            {{ auth.me()?.origin === 'External' ? 'External customer' : 'Internal staff' }}
          </p>
          <h1>My tickets</h1>
        </div>
        <button type="button" class="btn btn-primary" aria-haspopup="dialog" (click)="raise.open()">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2"
            stroke-linecap="round" aria-hidden="true"><path d="M12 5v14M5 12h14"></path></svg>
          Raise Ticket
        </button>
      </div>

      <section class="card" aria-labelledby="list-title">
        <div class="card-head">
          <h2 id="list-title">Your tickets</h2>
          <div class="filters">
            <label class="sr-only" for="search">Search your tickets</label>
            <input id="search" class="input" type="search" placeholder="Search by number or title"
              [value]="search()" (input)="search.set($any($event.target).value)" />
            <label class="sr-only" for="view">Show</label>
            <select id="view" class="input" [value]="view()" (change)="view.set($any($event.target).value)">
              <option value="open">Open tickets</option>
              <option value="all">All tickets</option>
              <option value="done">Resolved and closed</option>
            </select>
          </div>
        </div>

        @if (error()) {
          <p class="notice notice-error" role="alert">{{ error() }}</p>
        } @else if (loading()) {
          <p class="muted loading">Loading tickets…</p>
        } @else {
          <app-ticket-table [tickets]="visible()" [emptyText]="emptyText()" />
        }
      </section>

      <app-raise-ticket-dialog #raise (created)="onCreated($event)" />
    </main>
  `,
  styles: `
    .filters { display: flex; flex-wrap: wrap; gap: 8px; }
    .filters input { width: 260px; max-width: 100%; }
    .filters select { width: auto; }
    .loading, .notice { margin: 0 16px 16px; }
  `,
})
export class MyTicketsPage {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  protected readonly auth = inject(Auth);

  private readonly tickets = signal<TicketSummary[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly search = signal('');
  protected readonly view = signal<View>('open');

  protected readonly visible = computed(() => {
    const term = this.search().trim().toLowerCase();
    const view = this.view();
    return this.tickets().filter((t) => {
      const inView =
        view === 'all' || (view === 'open' ? OPEN_STATUSES.includes(t.status) : !OPEN_STATUSES.includes(t.status));
      const matches = !term || t.number.toLowerCase().includes(term) || t.title.toLowerCase().includes(term);
      return inView && matches;
    });
  });

  protected readonly emptyText = computed(() =>
    this.tickets().length === 0 ? 'You have no tickets yet. Use Raise Ticket to get help.' : 'No tickets match.',
  );

  constructor() {
    this.load();
  }

  private load(): void {
    // Staff can also see tickets assigned to their groups; those belong in the queue, not here.
    this.api.tickets('Mine').subscribe({
      next: (list) => {
        this.tickets.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load your tickets.');
        this.loading.set(false);
      },
    });
  }

  protected onCreated(ticket: TicketSummary): void {
    void this.router.navigate(['/tickets', ticket.id]);
  }
}
