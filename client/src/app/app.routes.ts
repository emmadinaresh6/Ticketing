import { Routes } from '@angular/router';
import { hasAnyRole, redirectHome, signedIn } from './core/auth';

// Each role's area is lazy-loaded and guarded with canMatch, so a customer's browser never
// downloads specialist or admin code. The API enforces access regardless.
export const routes: Routes = [
  { path: 'login', title: 'Sign in · Support Desk', loadComponent: () => import('./features/login/login-page').then((m) => m.LoginPage) },
  {
    path: 'my-tickets',
    title: 'My tickets · Support Desk',
    canMatch: [signedIn],
    loadComponent: () => import('./features/requester/my-tickets-page').then((m) => m.MyTicketsPage),
  },
  {
    path: 'tickets/:id',
    title: 'Ticket · Support Desk',
    canMatch: [signedIn],
    loadComponent: () => import('./features/tickets/ticket-detail-page').then((m) => m.TicketDetailPage),
  },
  {
    path: 'queue',
    title: 'Queue · Support Desk',
    canMatch: [hasAnyRole('Specialist', 'Admin')],
    loadComponent: () => import('./features/specialist/queue-page').then((m) => m.QueuePage),
  },
  {
    path: 'admin',
    title: 'Admin · Support Desk',
    canMatch: [hasAnyRole('Admin')],
    loadComponent: () => import('./features/admin/admin-page').then((m) => m.AdminPage),
  },
  { path: '**', canMatch: [redirectHome], children: [] },
];
