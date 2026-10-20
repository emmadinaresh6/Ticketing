import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Origin } from '../core/models';

// INT and EXT numbers get distinct badges so origin is visible at a glance.
@Component({
  selector: 'app-ticket-number',
  imports: [RouterLink],
  template: `
    @if (ticketId(); as id) {
      <a class="ticket-number mono" [class.ext]="origin() === 'External'" [routerLink]="['/tickets', id]">{{ number() }}</a>
    } @else {
      <span class="ticket-number mono" [class.ext]="origin() === 'External'">{{ number() }}</span>
    }
  `,
})
export class TicketNumber {
  readonly number = input.required<string>();
  readonly origin = input.required<Origin>();
  readonly ticketId = input<string>();
}
