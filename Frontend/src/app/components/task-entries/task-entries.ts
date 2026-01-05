import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api';

@Component({
  selector: 'app-task-entries',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './task-entries.html',
  styleUrls: ['./task-entries.css']
})
export class TaskEntriesComponent {
  @Input() taskEntries: any[] = [];
  @Input() jobs: any[] = [];
  @Input() tasks: any[] = [];
  @Input() userId: string | null = null;
  @Output() refresh = new EventEmitter<void>();

  showAddForm = false;
  newTaskEntry = {
    jobId: null as number | null,
    taskName: '',
    duration: 0,
    comment: ''
  };

  constructor(private api: ApiService) {}

  // ========== FORM TOGGLE ==========
  toggleAddForm() {
    this.showAddForm = !this.showAddForm;
    if (!this.showAddForm) {
      this.resetForm();
    }
  }

  resetForm() {
    this.newTaskEntry = { jobId: null, taskName: '', duration: 0, comment: '' };
  }

  // ========== ADD TASK ENTRY ==========
  addTaskEntry() {
    if (!this.newTaskEntry.jobId) {
      alert('Please select a job before saving.');
      return;
    }
    if (!this.newTaskEntry.taskName) {
      alert('Please select a task before saving.');
      return;
    }
    if (!this.newTaskEntry.duration || this.newTaskEntry.duration <= 0) {
      alert('Please enter a duration greater than zero.');
      return;
    }

    const now = new Date();
    const weekStart = this.startOfWeek(now);

    const payload = {
      TaskEntry: {
        AppUserId: this.resolveUserId(),
        WeekOf: this.formatDate(weekStart),
        Date: this.formatDate(now),
        DayName: now.toLocaleDateString('en-US', { weekday: 'long' }),
        JobId: this.newTaskEntry.jobId,
        TaskName: this.newTaskEntry.taskName,
        Duration: this.newTaskEntry.duration,
        Comment: this.newTaskEntry.comment,
        Status: 'Open'
      }
    };

    this.api.addTaskEntry(payload).subscribe({
      next: () => {
        console.log('Task entry added');
        this.showAddForm = false;
        this.resetForm();
        this.refresh.emit();
      },
      error: (err) => console.error('Add task entry failed', err)
    });
  }

  // ========== DELETE TASK ENTRY ==========
  deleteTaskEntry(id: number) {
    if (!confirm('Are you sure you want to delete this task entry?')) return;

    this.api.deleteTaskEntry(id).subscribe({
      next: () => {
        console.log('Task entry deleted');
        this.refresh.emit();
      },
      error: (err) => console.error('Delete task entry failed', err)
    });
  }

  // ========== HELPER METHODS ==========
  private resolveUserId(): string | null {
    return this.userId
      || localStorage.getItem('userId')
      || this.taskEntries?.[0]?.appUserId
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
