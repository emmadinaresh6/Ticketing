import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TicketSummary } from '../core/models';
import { StatusChip } from './status-chip';
import { TicketNumber } from './ticket-number';

@Component({
  selector: 'app-ticket-table',
  imports: [DatePipe, RouterLink, StatusChip, TicketNumber],
  template: `
    <div class="table-scroll">
      <table>
        <thead>
          <tr>
            <th scope="col">Ticket</th>
            <th scope="col">Title</th>
            <th scope="col">Category</th>
            <th scope="col">Status</th>
            <th scope="col">Priority</th>
            <th scope="col">Raised</th>
          </tr>
        </thead>
        <tbody>
          @for (t of tickets(); track t.id) {
            <tr>
              <td><app-ticket-number [number]="t.number" [origin]="t.origin" [ticketId]="t.id" /></td>
              <td><a class="row-title" [routerLink]="['/tickets', t.id]">{{ t.title }}</a></td>
              <td class="muted">{{ t.categoryName }}</td>
              <td><app-status-chip [status]="t.status" /></td>
              <td>{{ t.priority }}</td>
              <td class="muted nowrap">{{ t.createdAt | date: 'd MMM, HH:mm' }}</td>
            </tr>
          } @empty {
            <tr>
              <td colspan="6" class="muted empty">{{ emptyText() }}</td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `,
})
export class TicketTable {
  readonly tickets = input.required<TicketSummary[]>();
  readonly emptyText = input('No tickets.');
}
