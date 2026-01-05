import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api';

@Component({
  selector: 'app-leave-entries',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './leave-entries.html',
  styleUrls: ['./leave-entries.css']
})
export class LeaveEntriesComponent {
  @Input() leaveEntries: any[] = [];
  @Input() userId: string | null = null;
  @Output() refresh = new EventEmitter<void>();

  showAddForm = false;
  newLeaveEntry = {
    leaveType: 'PTO',
    leaveDuration: 8,
    status: 'Pending'
  };

  leaveTypes = ['PTO', 'Sick', 'Vacation', 'Personal', 'UPTO'];

  constructor(private api: ApiService) {}

  // ========== FORM TOGGLE ==========
  toggleAddForm() {
    this.showAddForm = !this.showAddForm;
    if (!this.showAddForm) {
      this.resetForm();
    }
  }

  resetForm() {
    this.newLeaveEntry = { leaveType: 'PTO', leaveDuration: 8, status: 'Pending' };
  }

  // ========== ADD LEAVE ENTRY ==========
  addLeaveEntry() {
    const now = new Date();
    const weekStart = this.startOfWeek(now);

    const payload = {
      LeaveEntry: {
        AppUserId: this.resolveUserId(),
        WeekOf: this.formatDate(weekStart),
        Date: this.formatDate(now),
        DayName: now.toLocaleDateString('en-US', { weekday: 'long' }),
        LeaveType: this.newLeaveEntry.leaveType,
        LeaveDuration: this.newLeaveEntry.leaveDuration,
        Status: this.newLeaveEntry.status
      }
    };

    this.api.addLeave(payload).subscribe({
      next: () => {
        console.log('Leave entry added');
        this.showAddForm = false;
        this.resetForm();
        this.refresh.emit();
      },
      error: (err) => console.error('Add leave entry failed', err)
    });
  }

  // ========== DELETE LEAVE ENTRY ==========
  deleteLeaveEntry(id: number) {
    if (!confirm('Are you sure you want to delete this leave entry?')) return;

    this.api.deleteLeave(id).subscribe({
      next: () => {
        console.log('Leave entry deleted');
        this.refresh.emit();
      },
      error: (err) => console.error('Delete leave entry failed', err)
    });
  }

  // ========== HELPER METHODS ==========
  private resolveUserId(): string | null {
    return this.userId
      || localStorage.getItem('userId')
      || this.leaveEntries?.[0]?.appUserId
      || null;
  }

  private startOfWeek(date: Date): Date {
    const start = new Date(date);
    start.setHours(0, 0, 0, 0);
    const day = start.getDay();
    start.setDate(start.getDate() - day);
    return start;
  }

  private formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = date.getDate().toString().padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
