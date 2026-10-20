import { Component, ElementRef, computed, inject, output, signal, viewChild } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { Api } from '../../core/api';
import { findDuplicate, findSimilar } from '../../core/category-name';
import { Category, PRIORITIES, Priority, SEVERITIES, Severity, TicketSummary } from '../../core/models';

@Component({
  selector: 'app-raise-ticket-dialog',
  imports: [ReactiveFormsModule],
  templateUrl: './raise-ticket-dialog.html',
  styleUrl: './raise-ticket-dialog.scss',
})
export class RaiseTicketDialog {
  private readonly api = inject(Api);
  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  readonly created = output<TicketSummary>();

  protected readonly priorities = PRIORITIES;
  protected readonly severities = SEVERITIES;
  protected readonly categories = signal<Category[]>([]);
  protected readonly submitting = signal(false);
  protected readonly error = signal('');

  protected readonly form = new FormGroup({
    title: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }),
    categoryId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    priority: new FormControl<Priority>('Medium', { nonNullable: true }),
    severity: new FormControl<Severity>('Moderate', { nonNullable: true }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(8000)] }),
  });

  // Add category: anyone can add one; exact duplicates are blocked and similar names suggested.
  protected readonly adding = signal(false);
  protected readonly draft = signal('');
  protected readonly duplicate = computed(() => findDuplicate(this.draft(), this.categories()));
  protected readonly similar = computed(() => findSimilar(this.draft(), this.categories()));
  protected readonly canAdd = computed(() => this.draft().trim().length > 0 && !this.duplicate());
  protected readonly addingBusy = signal(false);

  open(): void {
    this.error.set('');
    this.api.categories().subscribe({
      next: (list) => {
        this.categories.set(list);
        if (!this.form.controls.categoryId.value && list.length) {
          this.form.controls.categoryId.setValue(list[0].id);
        }
      },
      error: () => this.error.set('Could not load categories.'),
    });
    this.dialog().nativeElement.showModal();
  }

  protected close(): void {
    this.dialog().nativeElement.close();
  }

  // Native <dialog> close (Esc, close()) resets the form for next time.
  protected onClosed(): void {
    this.form.reset();
    this.cancelAdd();
    this.error.set('');
  }

  protected startAdd(): void {
    this.draft.set('');
    this.adding.set(true);
  }

  protected cancelAdd(): void {
    this.adding.set(false);
    this.draft.set('');
  }

  protected useCategory(category: Category): void {
    this.form.controls.categoryId.setValue(category.id);
    this.cancelAdd();
  }

  protected async addCategory(): Promise<void> {
    if (!this.canAdd()) return;
    this.addingBusy.set(true);
    try {
      // The server checks duplicates too; if someone else just added it, we get the existing one back.
      const result = await firstValueFrom(this.api.addCategory(this.draft()));
      if (!this.categories().some((c) => c.id === result.category.id)) {
        this.categories.update((list) => [...list, result.category].sort((a, b) => a.name.localeCompare(b.name)));
      }
      this.useCategory(result.category);
    } catch {
      this.error.set('Could not add the category. Try again.');
    } finally {
      this.addingBusy.set(false);
    }
  }

  protected async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.error.set('');
    try {
      const ticket = await firstValueFrom(this.api.createTicket(this.form.getRawValue()));
      this.close();
      this.created.emit(ticket);
    } catch {
      this.error.set('Could not create the ticket. Check your details and try again.');
    } finally {
      this.submitting.set(false);
    }
  }

  protected showError(name: 'title' | 'description'): boolean {
    const control = this.form.controls[name];
    return control.invalid && control.touched;
  }
}
