import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';

import { TaskEntriesComponent } from './task-entries';
import { ApiService } from '../../services/api';

describe('TaskEntriesComponent', () => {
  let component: TaskEntriesComponent;
  let fixture: ComponentFixture<TaskEntriesComponent>;
  let apiService: jasmine.SpyObj<ApiService>;

  beforeEach(async () => {
    const apiSpy = jasmine.createSpyObj('ApiService', ['addTaskEntry', 'deleteTaskEntry']);

    await TestBed.configureTestingModule({
      imports: [TaskEntriesComponent, HttpClientTestingModule],
      providers: [{ provide: ApiService, useValue: apiSpy }]
    }).compileComponents();

    fixture = TestBed.createComponent(TaskEntriesComponent);
    component = fixture.componentInstance;
    apiService = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;

    // Default setup
    component.taskEntries = [];
    component.jobs = [
      { id: 1, jobNumber: 'JOB001', jobName: 'Test Job 1' },
      { id: 2, jobNumber: 'JOB002', jobName: 'Test Job 2' }
    ];
    component.tasks = [
      { id: 1, taskDescription: 'Development' },
      { id: 2, taskDescription: 'Testing' }
    ];
    component.userId = 'test-user-123';
  });

  afterEach(() => {
    try {
      localStorage.clear();
    } catch {
      // Ignore storage cleanup errors
    }
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ========== Form Toggle Tests ==========
  describe('toggleAddForm', () => {
    it('toggles showAddForm from false to true', () => {
      component.showAddForm = false;
      component.toggleAddForm();
      expect(component.showAddForm).toBeTrue();
    });

    it('toggles showAddForm from true to false and resets form', () => {
      component.showAddForm = true;
      component.newTaskEntry = { jobId: 1, taskName: 'Test', duration: 5, comment: 'Comment' };

      component.toggleAddForm();

      expect(component.showAddForm).toBeFalse();
      expect(component.newTaskEntry.jobId).toBeNull();
      expect(component.newTaskEntry.taskName).toBe('');
      expect(component.newTaskEntry.duration).toBe(0);
      expect(component.newTaskEntry.comment).toBe('');
    });
  });

  describe('resetForm', () => {
    it('resets the form to initial values', () => {
      component.newTaskEntry = { jobId: 5, taskName: 'Some Task', duration: 10, comment: 'Test' };

      component.resetForm();

      expect(component.newTaskEntry).toEqual({
        jobId: null,
        taskName: '',
        duration: 0,
        comment: ''
      });
    });
  });

  // ========== Validation Tests ==========
  describe('addTaskEntry validation', () => {
    beforeEach(() => {
      spyOn(window, 'alert');
    });

    it('shows alert when no job is selected', () => {
      component.newTaskEntry = { jobId: null, taskName: 'Test', duration: 1, comment: '' };

      component.addTaskEntry();

      expect(window.alert).toHaveBeenCalledWith('Please select a job before saving.');
      expect(apiService.addTaskEntry).not.toHaveBeenCalled();
    });

    it('shows alert when no task is selected', () => {
      component.newTaskEntry = { jobId: 1, taskName: '', duration: 1, comment: '' };

      component.addTaskEntry();

      expect(window.alert).toHaveBeenCalledWith('Please select a task before saving.');
      expect(apiService.addTaskEntry).not.toHaveBeenCalled();
    });

    it('shows alert when duration is zero', () => {
      component.newTaskEntry = { jobId: 1, taskName: 'Test', duration: 0, comment: '' };

      component.addTaskEntry();

      expect(window.alert).toHaveBeenCalledWith('Please enter a duration greater than zero.');
      expect(apiService.addTaskEntry).not.toHaveBeenCalled();
    });

    it('shows alert when duration is negative', () => {
      component.newTaskEntry = { jobId: 1, taskName: 'Test', duration: -5, comment: '' };

      component.addTaskEntry();

      expect(window.alert).toHaveBeenCalledWith('Please enter a duration greater than zero.');
      expect(apiService.addTaskEntry).not.toHaveBeenCalled();
    });
  });

  // ========== addTaskEntry API Tests ==========
  describe('addTaskEntry', () => {
    beforeEach(() => {
      component.newTaskEntry = { jobId: 1, taskName: 'Development', duration: 2.5, comment: 'Test comment' };
    });

    it('calls api.addTaskEntry with correct payload', () => {
      apiService.addTaskEntry.and.returnValue(of({}));
      spyOn(component.refresh, 'emit');

      component.addTaskEntry();

      expect(apiService.addTaskEntry).toHaveBeenCalled();
      const payload = apiService.addTaskEntry.calls.mostRecent().args[0];
      expect(payload.TaskEntry).toBeDefined();
      expect(payload.TaskEntry.AppUserId).toBe('test-user-123');
      expect(payload.TaskEntry.JobId).toBe(1);
      expect(payload.TaskEntry.TaskName).toBe('Development');
      expect(payload.TaskEntry.Duration).toBe(2.5);
      expect(payload.TaskEntry.Comment).toBe('Test comment');
      expect(payload.TaskEntry.Status).toBe('Open');
    });

    it('emits refresh and resets form on success', () => {
      apiService.addTaskEntry.and.returnValue(of({}));
      spyOn(component.refresh, 'emit');
      component.showAddForm = true;

      component.addTaskEntry();

      expect(component.refresh.emit).toHaveBeenCalled();
      expect(component.showAddForm).toBeFalse();
      expect(component.newTaskEntry.jobId).toBeNull();
    });

    it('does not emit refresh on error', () => {
      apiService.addTaskEntry.and.returnValue(throwError(() => new Error('API Error')));
      spyOn(component.refresh, 'emit');
      spyOn(console, 'error');

      component.addTaskEntry();

      expect(component.refresh.emit).not.toHaveBeenCalled();
      expect(console.error).toHaveBeenCalled();
    });
  });

  // ========== deleteTaskEntry Tests ==========
  describe('deleteTaskEntry', () => {
    it('calls api.deleteTaskEntry on confirmation', () => {
      spyOn(window, 'confirm').and.returnValue(true);
      apiService.deleteTaskEntry.and.returnValue(of({}));
      spyOn(component.refresh, 'emit');

      component.deleteTaskEntry(456);

      expect(apiService.deleteTaskEntry).toHaveBeenCalledWith(456);
      expect(component.refresh.emit).toHaveBeenCalled();
    });

    it('does not call API when user cancels', () => {
      spyOn(window, 'confirm').and.returnValue(false);

      component.deleteTaskEntry(456);

      expect(apiService.deleteTaskEntry).not.toHaveBeenCalled();
    });

    it('does not emit refresh on error', () => {
      spyOn(window, 'confirm').and.returnValue(true);
      apiService.deleteTaskEntry.and.returnValue(throwError(() => new Error('API Error')));
      spyOn(component.refresh, 'emit');
      spyOn(console, 'error');

      component.deleteTaskEntry(456);

      expect(component.refresh.emit).not.toHaveBeenCalled();
      expect(console.error).toHaveBeenCalled();
    });
  });

  // ========== Input/Output Tests ==========
  describe('inputs', () => {
    it('accepts taskEntries input', () => {
      const entries = [
        { id: 1, taskName: 'Dev', duration: 2 },
        { id: 2, taskName: 'Test', duration: 1 }
      ];
      component.taskEntries = entries;

      expect(component.taskEntries).toEqual(entries);
    });

    it('accepts jobs input', () => {
      const jobs = [{ id: 10, jobNumber: 'J10', jobName: 'Job Ten' }];
      component.jobs = jobs;

      expect(component.jobs).toEqual(jobs);
    });

    it('accepts tasks input', () => {
      const tasks = [{ id: 20, taskDescription: 'Custom Task' }];
      component.tasks = tasks;

      expect(component.tasks).toEqual(tasks);
    });
  });
});
