import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../services/api';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule], // Import FormsModule for ngModel if needed
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit {
  data: any;
  loading = true;
  userId: string | null = null;
  weekSelect: string | null = null;
  currentDate: Date = new Date();

  constructor(private api: ApiService, private route: ActivatedRoute, private router: Router) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.userId = params['userId'] || localStorage.getItem('userId');
      this.weekSelect = params['WeekSelect'];
      this.loadDashboard();
    });
  }

  loadDashboard() {
    this.loading = true;
    this.api.getDashboard(this.weekSelect || undefined, this.userId || undefined).subscribe({
      next: (res) => {
        this.data = res;
        if (this.data?.isAdmin && this.data?.groups?.length && !this.exportForm.group) {
          this.exportForm.group = this.data.groups[0];
        }
        if (!this.userId) {
          this.userId =
            this.data?.currentUserId ||
            this.data?.dayEntries?.[0]?.appUserId ||
            this.data?.taskEntries?.[0]?.appUserId ||
            null;
        }
        if (this.userId) {
          localStorage.setItem('userId', this.userId);
        }
        console.log('Dashboard Data Loaded:', this.data);
        console.log('Current User ID:', this.userId);
        console.log('Today Entry Found:', this.currentDayEntry);
        console.log('Is Clocked In:', this.isClockedIn);
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading dashboard', err);
        this.loading = false;
      }
    });
  }

  changeWeek(offset: number) {
    // Calculate new date based on current selection or today
    // This logic should ideally be robust, but for now we might need to rely on backend returning next/prev week links or calculating it here.
    // The backend returns 'weekSelect' in the model.

    let currentWeekStart = this.data?.weekSelect ? new Date(this.data.weekSelect) : new Date();
    currentWeekStart.setDate(currentWeekStart.getDate() + (offset * 7));

    const newWeekStr = currentWeekStart.toISOString(); // Or format as needed by backend

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { WeekSelect: newWeekStr, userId: this.userId },
      queryParamsHandling: 'merge'
    });
  }

  clockIn() {
    const resolvedUserId = this.resolveUserId();
    if (!resolvedUserId) {
      console.error('Unable to determine the user ID for clock in.');
      return;
    }

    // Always create a NEW entry on clock in (Id = 0)
    const now = new Date();
    const weekStart = this.startOfWeek(now);

    const newEntry = {
      Id: 0, // Force new entry
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
        this.loadDashboard();
      },
      error: (err) => console.error('Clock In Failed', err)
    });
  }

  clockOut() {
    // Find the open entry (has start time but no end time)
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
        this.loadDashboard();
      },
      error: (err) => console.error('Clock Out Failed', err)
    });
  }

  logout() {
    this.api.logout().subscribe(() => {
      localStorage.removeItem('userId');
      this.router.navigate(['/login']);
    });
  }

  private resolveUserId(): string | null {
    return this.userId
      || localStorage.getItem('userId')
      || this.data?.userId
      || this.data?.currentUserId
      || this.data?.dayEntries?.[0]?.appUserId
      || this.data?.taskEntries?.[0]?.appUserId
      || null;
  }

  get currentDayEntry(): any {
    // Return the most recent open entry for today (has start but no end)
    return this.findOpenEntry();
  }

  get isClockedIn(): boolean {
    const entry = this.findOpenEntry();
    return !!(entry && entry.dayStartTime && !entry.dayEndTime);
  }

  // Find an open entry (has start time but no end time) for today
  public findOpenEntry(): any {
    if (!this.data?.dayEntries) {
      return null;
    }
    const today = this.formatDate(new Date());
    // Find entry for today that has start time but no end time
    return this.data.dayEntries.find((entry: any) => {
      if (!entry.date) return false;
      const entryDate = this.formatDate(new Date(entry.date));
      return entryDate === today && entry.dayStartTime && !entry.dayEndTime;
    });
  }

  // Find the most recent entry for today (for status display after clocking out)
  public findTodayEntry(): any {
    if (!this.data?.dayEntries) {
      console.log('findTodayEntry: No dayEntries in data');
      return null;
    }
    const today = this.formatDate(new Date());
    console.log(`findTodayEntry: Looking for date matching ${today}`);
    // Return the last entry for today (most recent)
    const todayEntries = this.data.dayEntries.filter((entry: any) => {
      if (!entry.date) return false;
      const entryDate = this.formatDate(new Date(entry.date));
      return entryDate === today;
    });
    return todayEntries.length > 0 ? todayEntries[todayEntries.length - 1] : null;
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
    const day = start.getDay(); // Sunday = 0
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

  public formatTime12Hour(timeStr: string | null): string {
    if (!timeStr) return '';

    // Check if timeStr is already in HH:mm:ss format, else try to parse it
    // Assuming backend sends "14:30:00"
    const [hoursStr, minutesStr] = timeStr.split(':');
    let hours = parseInt(hoursStr, 10);
    const minutes = minutesStr;

    const ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12;
    hours = hours ? hours : 12; // the hour '0' should be '12'

    return `${hours}:${minutes} ${ampm}`;
  }

  // ========== DELETE METHODS ==========
  deleteTaskEntry(id: number) {
    if (!confirm('Are you sure you want to delete this task entry?')) return;

    this.api.deleteTaskEntry(id).subscribe({
      next: () => {
        console.log('Task entry deleted');
        this.loadDashboard();
      },
      error: (err) => console.error('Delete task entry failed', err)
    });
  }

  deleteLeaveEntry(id: number) {
    if (!confirm('Are you sure you want to delete this leave entry?')) return;

    this.api.deleteLeave(id).subscribe({
      next: () => {
        console.log('Leave entry deleted');
        this.loadDashboard();
      },
      error: (err) => console.error('Delete leave entry failed', err)
    });
  }

  deleteDayEntry(id: number) {
    if (!confirm('Are you sure you want to delete this day entry?')) return;

    this.api.deleteDay(id).subscribe({
      next: () => {
        console.log('Day entry deleted');
        this.loadDashboard();
      },
      error: (err) => console.error('Delete day entry failed', err)
    });
  }

  // ========== ADD TIME ENTRY ==========
  showAddTaskEntryForm = false;
  newTaskEntry = {
    jobId: null as number | null,
    taskName: '',
    duration: 0,
    comment: ''
  };
  showAddJobForm = false;
  newJob = {
    jobNumber: '',
    jobName: ''
  };
  showAddTaskForm = false;
  newTask = {
    taskDescription: ''
  };
  exportForm = {
    group: '',
    fromDate: '',
    toDate: ''
  };
  exporting = false;

  toggleAddTaskEntryForm() {
    this.showAddTaskEntryForm = !this.showAddTaskEntryForm;
    if (!this.showAddTaskEntryForm) {
      this.resetTaskEntryForm();
    }
  }

  resetTaskEntryForm() {
    this.newTaskEntry = { jobId: null, taskName: '', duration: 0, comment: '' };
  }

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
        this.showAddTaskEntryForm = false;
        this.resetTaskEntryForm();
        this.loadDashboard();
      },
      error: (err) => console.error('Add task entry failed', err)
    });
  }

  toggleAddJobForm() {
    this.showAddJobForm = !this.showAddJobForm;
    if (!this.showAddJobForm) {
      this.resetJobForm();
    }
  }

  resetJobForm() {
    this.newJob = { jobNumber: '', jobName: '' };
  }

  createJob() {
    const jobNumber = this.newJob.jobNumber.trim();
    const jobName = this.newJob.jobName.trim();

    if (!jobNumber || !jobName) {
      alert('Job number and job name are required.');
      return;
    }

    const payload = {
      JobModel: {
        JobNumber: jobNumber,
        JobName: jobName,
        JobNumberAndJobName: `${jobNumber} - ${jobName}`
      }
    };

    this.api.addJob(payload).subscribe({
      next: () => {
        console.log('Job created');
        this.showAddJobForm = false;
        this.resetJobForm();
        this.loadDashboard();
      },
      error: (err) => console.error('Add job failed', err)
    });
  }

  toggleAddTaskForm() {
    this.showAddTaskForm = !this.showAddTaskForm;
    if (!this.showAddTaskForm) {
      this.resetTaskForm();
    }
  }

  resetTaskForm() {
    this.newTask = { taskDescription: '' };
  }

  createTask() {
    const description = this.newTask.taskDescription.trim();
    if (!description) {
      alert('Task description is required.');
      return;
    }

    this.api.createTask(description).subscribe({
      next: () => {
        console.log('Task created');
        this.showAddTaskForm = false;
        this.resetTaskForm();
        this.loadDashboard();
      },
      error: (err) => console.error('Add task failed', err)
    });
  }

  deleteJob(id: number) {
    if (!confirm('Are you sure you want to delete this job?')) return;

    this.api.deleteJob(id).subscribe({
      next: () => {
        console.log('Job deleted');
        this.loadDashboard();
      },
      error: (err) => {
        console.error('Delete job failed', err);
        const message = err.error || 'Failed to delete job.';
        alert(message);
      }
    });
  }

  deleteTask(id: number) {
    if (!confirm('Are you sure you want to delete this task?')) return;

    this.api.deleteTask(id).subscribe({
      next: () => {
        console.log('Task deleted');
        this.loadDashboard();
      },
      error: (err) => {
        console.error('Delete task failed', err);
        const message = err.error || 'Failed to delete task.';
        alert(message);
      }
    });
  }

  // ========== ADD LEAVE ENTRY ==========
  showAddLeaveForm = false;
  newLeaveEntry = {
    leaveType: 'PTO',
    leaveDuration: 8,
    status: 'Pending'
  };

  toggleAddLeaveForm() {
    this.showAddLeaveForm = !this.showAddLeaveForm;
    if (!this.showAddLeaveForm) {
      this.resetLeaveEntryForm();
    }
  }

  resetLeaveEntryForm() {
    this.newLeaveEntry = { leaveType: 'PTO', leaveDuration: 8, status: 'Pending' };
  }

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
        this.showAddLeaveForm = false;
        this.resetLeaveEntryForm();
        this.loadDashboard();
      },
      error: (err) => console.error('Add leave entry failed', err)
    });
  }

  exportTimeSheet() {
    if (!this.exportForm.group) {
      alert('Please select a group to export.');
      return;
    }

    this.exporting = true;
    const fromDate = this.exportForm.fromDate || undefined;
    const toDate = this.exportForm.toDate || undefined;

    this.api.exportTimeSheet(this.exportForm.group, fromDate, toDate).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        const fromLabel = this.exportForm.fromDate || 'start';
        const toLabel = this.exportForm.toDate || 'end';
        link.download = `timesheet_${this.exportForm.group}_${fromLabel}_${toLabel}.csv`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
        this.exporting = false;
      },
      error: (err) => {
        console.error('Export time sheet failed', err);
        alert('Unable to export time sheet. Please try again.');
        this.exporting = false;
      }
    });
  }
}
