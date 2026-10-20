// Mirrors the API DTOs. To be replaced by a client generated from the API's OpenAPI document.

export type Origin = 'Internal' | 'External';
export type RoleName = 'Requester' | 'Specialist' | 'Admin';
export type TicketStatus =
  | 'New'
  | 'Assigned'
  | 'InProgress'
  | 'WaitingOnUser'
  | 'Resolved'
  | 'Closed'
  | 'SelfResolved'
  | 'Reopened';
export type Priority = 'Low' | 'Medium' | 'High' | 'Critical';
export type Severity = 'Minor' | 'Moderate' | 'Major' | 'Critical';

export interface Me {
  id: string;
  displayName: string;
  email: string;
  origin: Origin;
  roles: RoleName[];
  groups: { id: string; name: string }[];
}

export interface DevUser {
  key: string;
  displayName: string;
  origin: Origin;
  roles: RoleName[];
}

export interface Category {
  id: string;
  name: string;
  hasRouting: boolean;
}

export interface AddCategoryResult {
  category: Category;
  created: boolean;
}

export interface TicketSummary {
  id: string;
  number: string;
  origin: Origin;
  title: string;
  status: TicketStatus;
  priority: Priority;
  severity: Severity;
  categoryName: string;
  createdAt: string;
}

export interface TicketComment {
  id: string;
  authorName: string;
  body: string;
  isInternal: boolean;
  createdAt: string;
}

export interface TicketDetail extends Omit<TicketSummary, 'categoryName'> {
  description: string;
  categoryId: string;
  categoryName: string;
  requesterName: string;
  assignedGroupName: string | null;
  assignedUserName: string | null;
  comments: TicketComment[];
}

export interface CreateTicketRequest {
  title: string;
  description: string;
  categoryId: string;
  priority: Priority;
  severity: Severity;
}

export const PRIORITIES: Priority[] = ['Low', 'Medium', 'High', 'Critical'];

// Placeholder values; final list is an open item. Severity does not affect SLA.
export const SEVERITIES: { value: Severity; label: string }[] = [
  { value: 'Minor', label: 'Minor · workaround exists' },
  { value: 'Moderate', label: 'Moderate · slows me down' },
  { value: 'Major', label: "Major · I can't work" },
  { value: 'Critical', label: 'Critical · many people affected' },
];

export const OPEN_STATUSES: TicketStatus[] = ['New', 'Assigned', 'InProgress', 'WaitingOnUser', 'Reopened'];
