import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, effect, inject, input, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Api } from '../../core/api';
import { Auth } from '../../core/auth';
import { TicketDetail } from '../../core/models';
import { StatusChip } from '../../shared/status-chip';
import { TicketNumber } from '../../shared/ticket-number';

@Component({
  selector: 'app-ticket-detail-page',
  imports: [DatePipe, ReactiveFormsModule, RouterLink, StatusChip, TicketNumber],
  templateUrl: './ticket-detail-page.html',
  styleUrl: './ticket-detail-page.scss',
})
export class TicketDetailPage {
  private readonly api = inject(Api);
  protected readonly auth = inject(Auth);

  // Bound from the route (withComponentInputBinding).
  readonly id = input.required<string>();

  protected readonly ticket = signal<TicketDetail | null>(null);
  protected readonly state = signal<'loading' | 'ready' | 'not-found' | 'error'>('loading');

  protected readonly comment = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.maxLength(8000)],
  });
  protected readonly internal = signal(false);
  protected readonly posting = signal(false);
  protected readonly postError = signal('');

  constructor() {
    effect(() => this.load(this.id()));
  }

  protected backLink(): string {
    return this.auth.isStaff() && this.ticket()?.requesterName !== this.auth.me()?.displayName ? '/queue' : '/my-tickets';
  }

  private load(id: string): void {
    this.state.set('loading');
    this.api.ticket(id).subscribe({
      next: (t) => {
        this.ticket.set(t);
        this.state.set('ready');
      },
      // 404 also covers tickets this user is not allowed to see.
      error: (e: HttpErrorResponse) => this.state.set(e.status === 404 ? 'not-found' : 'error'),
    });
  }

  protected async post(): Promise<void> {
    const ticket = this.ticket();
    if (!ticket || this.comment.invalid) {
      this.comment.markAsTouched();
      return;
    }
    this.posting.set(true);
    this.postError.set('');
    try {
      const added = await firstValueFrom(
        this.api.addComment(ticket.id, this.comment.value, this.auth.isStaff() && this.internal()),
      );
      this.ticket.set({ ...ticket, comments: [...ticket.comments, added] });
      this.comment.reset();
    } catch {
      this.postError.set('Could not post your comment. Try again.');
    } finally {
      this.posting.set(false);
    }
  }

  protected initials(name: string): string {
    return name
      .split(' ')
      .map((p) => p[0])
      .join('')
      .slice(0, 2)
      .toUpperCase();
  }
}
