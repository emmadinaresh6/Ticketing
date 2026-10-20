import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import {
  AddCategoryResult,
  Category,
  CreateTicketRequest,
  DevUser,
  Me,
  TicketComment,
  TicketDetail,
  TicketSummary,
} from './models';

@Injectable({ providedIn: 'root' })
export class Api {
  private readonly http = inject(HttpClient);

  devUsers() {
    return this.http.get<DevUser[]>('/api/dev/users');
  }

  devLogin(user: string) {
    return this.http.post<{ accessToken: string; expiresAt: string }>('/api/dev/login', { user });
  }

  me() {
    return this.http.get<Me>('/api/me');
  }

  categories() {
    return this.http.get<Category[]>('/api/categories');
  }

  addCategory(name: string) {
    return this.http.post<AddCategoryResult>('/api/categories', { name });
  }

  // Mine: tickets I raised. Queue: others' tickets I can see (my groups, or all for admins).
  tickets(scope: 'All' | 'Mine' | 'Queue' = 'All') {
    return this.http.get<TicketSummary[]>('/api/tickets', { params: { scope } });
  }

  ticket(id: string) {
    return this.http.get<TicketDetail>(`/api/tickets/${encodeURIComponent(id)}`);
  }

  createTicket(request: CreateTicketRequest) {
    return this.http.post<TicketSummary>('/api/tickets', request);
  }

  addComment(ticketId: string, body: string, isInternal: boolean) {
    return this.http.post<TicketComment>(`/api/tickets/${encodeURIComponent(ticketId)}/comments`, {
      body,
      isInternal,
    });
  }
}
