import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../services/api';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DayEntriesComponent } from '../day-entries/day-entries';
import { TaskEntriesComponent } from '../task-entries/task-entries';
import { LeaveEntriesComponent } from '../leave-entries/leave-entries';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, DayEntriesComponent, TaskEntriesComponent, LeaveEntriesComponent],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit {
  data: any;
  loading = true;
  userId: string | null = null;
  weekSelect: string | null = null;

  // Admin features
  showAddJobForm = false;
  newJob = { jobNumber: '', jobName: '' };
  showAddTaskForm = false;
  newTask = { taskDescription: '' };
  exportForm = { group: '', fromDate: '', toDate: '' };
  exporting = false;

  constructor(private api: ApiService, private route: ActivatedRoute, private router: Router) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.userId = params['userId'] || localStorage.getItem('userId');
      this.weekSelect = params['WeekSelect'] || this.formatDate(this.startOfWeek(new Date()));
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
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading dashboard', err);
        this.loading = false;
      }
    });
  }

  changeWeek(offset: number) {
    let currentWeekStart = this.data?.weekSelect ? new Date(this.data.weekSelect) : new Date();
    currentWeekStart.setDate(currentWeekStart.getDate() + (offset * 7));

    const newWeekStr = this.formatDate(currentWeekStart);

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { WeekSelect: newWeekStr, userId: this.userId },
      queryParamsHandling: 'merge'
    });
  }

  logout() {
    this.api.logout().subscribe(() => {
      localStorage.removeItem('userId');
      this.router.navigate(['/login']);
    });
  }

  // ========== ADMIN: JOB MANAGEMENT ==========
  toggleAddJobForm() {
    this.showAddJobForm = !this.showAddJobForm;
    if (!this.showAddJobForm) {
      this.newJob = { jobNumber: '', jobName: '' };
    }
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
        this.newJob = { jobNumber: '', jobName: '' };
        this.loadDashboard();
      },
      error: (err) => console.error('Add job failed', err)
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
        alert(err.error || 'Failed to delete job.');
      }
    });
  }

  // ========== ADMIN: TASK MANAGEMENT ==========
  toggleAddTaskForm() {
    this.showAddTaskForm = !this.showAddTaskForm;
    if (!this.showAddTaskForm) {
      this.newTask = { taskDescription: '' };
    }
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
        this.newTask = { taskDescription: '' };
        this.loadDashboard();
      },
      error: (err) => console.error('Add task failed', err)
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
        alert(err.error || 'Failed to delete task.');
      }
    });
  }

  // ========== ADMIN: EXPORT ==========
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

  // ========== HELPER METHODS ==========
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
