import { Component, computed, input } from '@angular/core';
import { TicketStatus } from '../core/models';

const LABELS: Record<TicketStatus, string> = {
  New: 'New',
  Assigned: 'Assigned',
  InProgress: 'In progress',
  WaitingOnUser: 'Waiting on user',
  Resolved: 'Resolved',
  Closed: 'Closed',
  SelfResolved: 'Self-resolved',
  Reopened: 'Reopened',
};

@Component({
  selector: 'app-status-chip',
  template: `<span class="chip" [class]="'status-' + status()">{{ label() }}</span>`,
})
export class StatusChip {
  readonly status = input.required<TicketStatus>();
  protected readonly label = computed(() => LABELS[this.status()]);
}
