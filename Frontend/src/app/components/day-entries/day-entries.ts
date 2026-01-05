import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api';

@Component({
  selector: 'app-day-entries',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './day-entries.html',
  styleUrls: ['./day-entries.css']
})
export class DayEntriesComponent {
  @Input() dayEntries: any[] = [];
  @Input() userId: string | null = null;
  @Input() weekSelect: string | null = null;
  @Output() refresh = new EventEmitter<void>();

  constructor(private api: ApiService) {}

  // ========== CLOCK IN/OUT ==========
  clockIn() {
    const resolvedUserId = this.resolveUserId();
    if (!resolvedUserId) {
      console.error('Unable to determine the user ID for clock in.');
      return;
    }

    const now = new Date();
    const weekStart = this.startOfWeek(now);

    const newEntry = {
      Id: 0,
      AppUserId: resolvedUserId,
      WeekOf: this.formatDate(weekStart),
      Date: this.formatDate(now),
      DayName: now.toLocaleDateString('en-US', { weekday: 'long' }),
      DayStartTime: this.formatTime(now),
      DayEndTime: null,
      LunchStartTime: null,
      LunchEndTime: null,
      DayDuration: null,
      LunchDuration: null,
      WorkDuration: null,
      Status: 'Open'
    };

    const payload = { DayEntry: newEntry };
    console.log('Clocking In with:', payload);

    this.api.clockInOut(payload).subscribe({
      next: () => {
        console.log('Clock In Success');
        this.refresh.emit();
      },
      error: (err) => console.error('Clock In Failed', err)
    });
  }

  clockOut() {
    const openEntry = this.findOpenEntry();

    if (!openEntry || !openEntry.id) {
      console.error('No open entry found to clock out');
      return;
    }

    const baseEntry = this.buildDayEntryPayload(openEntry);
    baseEntry.DayStartTime = openEntry.dayStartTime;
    baseEntry.DayEndTime = this.formatTime(new Date());

    const payload = { DayEntry: baseEntry };
    console.log('Clocking Out with:', payload);

    this.api.clockInOut(payload).subscribe({
      next: () => {
        console.log('Clock Out Success');
        this.refresh.emit();
      },
      error: (err) => console.error('Clock Out Failed', err)
    });
  }

  // ========== DELETE ==========
  deleteDayEntry(id: number) {
    if (!confirm('Are you sure you want to delete this day entry?')) return;

    this.api.deleteDay(id).subscribe({
      next: () => {
        console.log('Day entry deleted');
        this.refresh.emit();
      },
      error: (err) => console.error('Delete day entry failed', err)
    });
  }

  // ========== GETTERS ==========
  get isClockedIn(): boolean {
    const entry = this.findOpenEntry();
    return !!(entry && entry.dayStartTime && !entry.dayEndTime);
  }

  get currentDayEntry(): any {
    return this.findOpenEntry();
  }

  // ========== HELPER METHODS ==========
  findOpenEntry(): any {
    if (!this.dayEntries) {
      return null;
    }
    const today = this.formatDate(new Date());
    return this.dayEntries.find((entry: any) => {
      if (!entry.date) return false;
      const entryDate = this.formatDate(new Date(entry.date));
      return entryDate === today && entry.dayStartTime && !entry.dayEndTime;
    });
  }

  findTodayEntry(): any {
    if (!this.dayEntries) {
      return null;
    }
    const today = this.formatDate(new Date());
    const todayEntries = this.dayEntries.filter((entry: any) => {
      if (!entry.date) return false;
      const entryDate = this.formatDate(new Date(entry.date));
      return entryDate === today;
    });
    return todayEntries.length > 0 ? todayEntries[todayEntries.length - 1] : null;
  }

  private resolveUserId(): string | null {
    return this.userId
      || localStorage.getItem('userId')
      || this.dayEntries?.[0]?.appUserId
      || null;
  }

  private buildDayEntryPayload(entry?: any) {
    const now = new Date();
    const entryDate = entry?.date ? new Date(entry.date) : now;
    const normalizedEntryDate = new Date(entryDate);
    normalizedEntryDate.setHours(0, 0, 0, 0);
    const weekStart = entry?.weekOf ? this.startOfWeek(new Date(entry.weekOf)) : this.startOfWeek(normalizedEntryDate);

    return {
      Id: entry?.id ?? 0,
      AppUserId: entry?.appUserId ?? this.resolveUserId(),
      WeekOf: this.formatDate(weekStart),
      Date: this.formatDate(normalizedEntryDate),
      DayName: entry?.dayName ?? normalizedEntryDate.toLocaleDateString('en-US', { weekday: 'long' }),
      DayStartTime: entry?.dayStartTime ?? null,
      DayEndTime: entry?.dayEndTime ?? null,
      LunchStartTime: entry?.lunchStartTime ?? null,
      LunchEndTime: entry?.lunchEndTime ?? null,
      DayDuration: entry?.dayDuration ?? null,
      LunchDuration: entry?.lunchDuration ?? null,
      WorkDuration: entry?.workDuration ?? null,
      Status: entry?.status ?? null
    };
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

  private formatTime(date: Date): string {
    return date.toTimeString().split(' ')[0];
  }

  formatTime12Hour(timeStr: string | null): string {
    if (!timeStr) return '';

    const [hoursStr, minutesStr] = timeStr.split(':');
    let hours = parseInt(hoursStr, 10);
    const minutes = minutesStr;

    const ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12;
    hours = hours ? hours : 12;

    return `${hours}:${minutes} ${ampm}`;
  }
}
