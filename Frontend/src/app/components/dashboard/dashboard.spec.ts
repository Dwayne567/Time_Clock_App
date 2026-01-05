import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { of, throwError } from 'rxjs';

import { ApiService } from '../../services/api';
import { DashboardComponent } from './dashboard';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let apiService: jasmine.SpyObj<ApiService>;
  let router: Router;
  let dashboardResponse: any;

  beforeEach(async () => {
    const apiSpy = jasmine.createSpyObj('ApiService', [
      'getDashboard',
      'logout',
      'addJob',
      'deleteJob',
      'createTask',
      'deleteTask',
      'exportTimeSheet'
    ]);

    dashboardResponse = {
      dayEntries: [],
      taskEntries: [],
      leaveEntries: [],
      jobs: [],
      tasks: [],
      weekSelect: '2026-01-04'
    };

    apiSpy.getDashboard.and.returnValue(of(dashboardResponse));

    await TestBed.configureTestingModule({
      imports: [DashboardComponent, RouterTestingModule],
      providers: [
        { provide: ApiService, useValue: apiSpy },
        { provide: ActivatedRoute, useValue: { queryParams: of({}) } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    apiService = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;
    router = TestBed.inject(Router);

    fixture.detectChanges();
    await fixture.whenStable();
  });

  afterEach(() => {
    try {
      localStorage.clear();
    } catch {
      // Ignore storage cleanup errors in test envs
    }
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ========== Initialization Tests ==========
  describe('ngOnInit', () => {
    it('calls loadDashboard on init', () => {
      expect(apiService.getDashboard).toHaveBeenCalled();
    });

    it('uses userId from query params if available', fakeAsync(() => {
      const activatedRoute = TestBed.inject(ActivatedRoute);
      (activatedRoute as any).queryParams = of({ userId: 'query-user-id' });

      component.ngOnInit();
      tick();

      expect(component.userId).toBe('query-user-id');
    }));
  });

  // ========== loadDashboard Tests ==========
  describe('loadDashboard', () => {
    it('sets loading to false after data loads', () => {
      expect(component.loading).toBeFalse();
    });

    it('stores data from API response', () => {
      dashboardResponse.dayEntries = [{ id: 1 }];
      apiService.getDashboard.and.returnValue(of(dashboardResponse));

      component.loadDashboard();

      expect(component.data.dayEntries.length).toBe(1);
    });

    it('sets userId from response when not already set', () => {
      dashboardResponse.currentUserId = 'response-user-id';
      apiService.getDashboard.and.returnValue(of(dashboardResponse));
      component.userId = null;

      component.loadDashboard();

      expect(component.userId).toBe('response-user-id' as any);
    });

    it('stores userId in localStorage', () => {
      dashboardResponse.currentUserId = 'stored-user-id';
      apiService.getDashboard.and.returnValue(of(dashboardResponse));
      component.userId = null;

      component.loadDashboard();

      expect(localStorage.getItem('userId')).toBe('stored-user-id');
    });

    it('handles API errors gracefully', () => {
      apiService.getDashboard.and.returnValue(throwError(() => new Error('API Error')));
      spyOn(console, 'error');

      component.loadDashboard();

      expect(component.loading).toBeFalse();
      expect(console.error).toHaveBeenCalled();
    });
  });

  // ========== changeWeek Tests ==========
  describe('changeWeek', () => {
    it('navigates with updated week parameter', () => {
      spyOn(router, 'navigate');
      component.data = { weekSelect: '2026-01-05' };
      component.userId = 'test-user';

      component.changeWeek(1);

      expect(router.navigate).toHaveBeenCalled();
    });

    it('increments week by 7 days for positive offset', () => {
      spyOn(router, 'navigate');
      // Set a known week, the component adds 7 days
      component.data = { weekSelect: '2026-01-04' };

      component.changeWeek(1);

      const navCall = (router.navigate as jasmine.Spy).calls.mostRecent();
      const queryParams = navCall.args[1].queryParams;
      // Verify week was incremented (exact date depends on timezone handling)
      const newDate = new Date(queryParams.WeekSelect);
      const oldDate = new Date('2026-01-04');
      const daysDiff = Math.round((newDate.getTime() - oldDate.getTime()) / (1000 * 60 * 60 * 24));
      expect(daysDiff).toBeGreaterThanOrEqual(6);
      expect(daysDiff).toBeLessThanOrEqual(8);
    });

    it('decrements week by 7 days for negative offset', () => {
      spyOn(router, 'navigate');
      component.data = { weekSelect: '2026-01-11' };

      component.changeWeek(-1);

      const navCall = (router.navigate as jasmine.Spy).calls.mostRecent();
      const queryParams = navCall.args[1].queryParams;
      // Verify week was decremented (exact date depends on timezone handling)
      const newDate = new Date(queryParams.WeekSelect);
      const oldDate = new Date('2026-01-11');
      const daysDiff = Math.round((oldDate.getTime() - newDate.getTime()) / (1000 * 60 * 60 * 24));
      expect(daysDiff).toBeGreaterThanOrEqual(6);
      expect(daysDiff).toBeLessThanOrEqual(8);
    });
  });

  // ========== logout Tests ==========
  describe('logout', () => {
    it('calls api.logout and navigates to login', () => {
      apiService.logout.and.returnValue(of({}));
      spyOn(router, 'navigate');

      component.logout();

      expect(apiService.logout).toHaveBeenCalled();
      expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });

    it('removes userId from localStorage', () => {
      localStorage.setItem('userId', 'test-user');
      apiService.logout.and.returnValue(of({}));
      spyOn(router, 'navigate');

      component.logout();

      expect(localStorage.getItem('userId')).toBeNull();
    });
  });

  // ========== Admin Job Management Tests ==========
  describe('toggleAddJobForm', () => {
    it('toggles showAddJobForm', () => {
      component.showAddJobForm = false;
      component.toggleAddJobForm();
      expect(component.showAddJobForm).toBeTrue();

      component.toggleAddJobForm();
      expect(component.showAddJobForm).toBeFalse();
    });

    it('resets newJob when closing form', () => {
      component.showAddJobForm = true;
      component.newJob = { jobNumber: 'JOB001', jobName: 'Test' };

      component.toggleAddJobForm();

      expect(component.newJob.jobNumber).toBe('');
      expect(component.newJob.jobName).toBe('');
    });
  });

  describe('createJob', () => {
    beforeEach(() => {
      spyOn(window, 'alert');
    });

    it('shows alert when job number is empty', () => {
      component.newJob = { jobNumber: '', jobName: 'Test Job' };

      component.createJob();

      expect(window.alert).toHaveBeenCalledWith('Job number and job name are required.');
      expect(apiService.addJob).not.toHaveBeenCalled();
    });

    it('shows alert when job name is empty', () => {
      component.newJob = { jobNumber: 'JOB001', jobName: '' };

      component.createJob();

      expect(window.alert).toHaveBeenCalledWith('Job number and job name are required.');
      expect(apiService.addJob).not.toHaveBeenCalled();
    });

    it('calls api.addJob with correct payload', () => {
      apiService.addJob.and.returnValue(of({}));
      component.newJob = { jobNumber: 'JOB001', jobName: 'Test Job' };

      component.createJob();

      expect(apiService.addJob).toHaveBeenCalled();
      const payload = apiService.addJob.calls.mostRecent().args[0];
      expect(payload.JobModel.JobNumber).toBe('JOB001');
      expect(payload.JobModel.JobName).toBe('Test Job');
      expect(payload.JobModel.JobNumberAndJobName).toBe('JOB001 - Test Job');
    });

    it('closes form and reloads dashboard on success', () => {
      apiService.addJob.and.returnValue(of({}));
      component.showAddJobForm = true;
      component.newJob = { jobNumber: 'JOB001', jobName: 'Test Job' };

      component.createJob();

      expect(component.showAddJobForm).toBeFalse();
      expect(apiService.getDashboard).toHaveBeenCalled();
    });
  });

  describe('deleteJob', () => {
    it('calls api.deleteJob on confirmation', () => {
      spyOn(window, 'confirm').and.returnValue(true);
      apiService.deleteJob.and.returnValue(of({}));

      component.deleteJob(123);

      expect(apiService.deleteJob).toHaveBeenCalledWith(123);
    });

    it('does not call API when user cancels', () => {
      spyOn(window, 'confirm').and.returnValue(false);

      component.deleteJob(123);

      expect(apiService.deleteJob).not.toHaveBeenCalled();
    });

    it('shows alert on error', () => {
      spyOn(window, 'confirm').and.returnValue(true);
      spyOn(window, 'alert');
      apiService.deleteJob.and.returnValue(throwError(() => ({ error: 'Job in use' })));

      component.deleteJob(123);

      expect(window.alert).toHaveBeenCalledWith('Job in use');
    });
  });

  // ========== Admin Task Management Tests ==========
  describe('toggleAddTaskForm', () => {
    it('toggles showAddTaskForm', () => {
      component.showAddTaskForm = false;
      component.toggleAddTaskForm();
      expect(component.showAddTaskForm).toBeTrue();
    });

    it('resets newTask when closing form', () => {
      component.showAddTaskForm = true;
      component.newTask = { taskDescription: 'Test Task' };

      component.toggleAddTaskForm();

      expect(component.newTask.taskDescription).toBe('');
    });
  });

  describe('createTask', () => {
    beforeEach(() => {
      spyOn(window, 'alert');
    });

    it('shows alert when description is empty', () => {
      component.newTask = { taskDescription: '' };

      component.createTask();

      expect(window.alert).toHaveBeenCalledWith('Task description is required.');
      expect(apiService.createTask).not.toHaveBeenCalled();
    });

    it('calls api.createTask with description', () => {
      apiService.createTask.and.returnValue(of({}));
      component.newTask = { taskDescription: 'Development' };

      component.createTask();

      expect(apiService.createTask).toHaveBeenCalledWith('Development');
    });

    it('closes form and reloads dashboard on success', () => {
      apiService.createTask.and.returnValue(of({}));
      component.showAddTaskForm = true;
      component.newTask = { taskDescription: 'Development' };

      component.createTask();

      expect(component.showAddTaskForm).toBeFalse();
      expect(apiService.getDashboard).toHaveBeenCalled();
    });
  });

  describe('deleteTask', () => {
    it('calls api.deleteTask on confirmation', () => {
      spyOn(window, 'confirm').and.returnValue(true);
      apiService.deleteTask.and.returnValue(of({}));

      component.deleteTask(456);

      expect(apiService.deleteTask).toHaveBeenCalledWith(456);
    });

    it('does not call API when user cancels', () => {
      spyOn(window, 'confirm').and.returnValue(false);

      component.deleteTask(456);

      expect(apiService.deleteTask).not.toHaveBeenCalled();
    });
  });

  // ========== Export TimeSheet Tests ==========
  describe('exportTimeSheet', () => {
    beforeEach(() => {
      spyOn(window, 'alert');
    });

    it('shows alert when no group is selected', () => {
      component.exportForm = { group: '', fromDate: '', toDate: '' };

      component.exportTimeSheet();

      expect(window.alert).toHaveBeenCalledWith('Please select a group to export.');
      expect(apiService.exportTimeSheet).not.toHaveBeenCalled();
    });

    it('calls api.exportTimeSheet with correct parameters', () => {
      const blob = new Blob(['test'], { type: 'text/csv' });
      apiService.exportTimeSheet.and.returnValue(of(blob));
      component.exportForm = { group: 'Engineering', fromDate: '2026-01-01', toDate: '2026-01-31' };

      // Mock URL and link creation
      spyOn(window.URL, 'createObjectURL').and.returnValue('blob:test');
      spyOn(window.URL, 'revokeObjectURL');
      spyOn(document.body, 'appendChild');
      spyOn(document.body, 'removeChild');

      component.exportTimeSheet();

      expect(apiService.exportTimeSheet).toHaveBeenCalledWith('Engineering', '2026-01-01', '2026-01-31');
    });

    it('sets exporting flag during export', () => {
      const blob = new Blob(['test'], { type: 'text/csv' });
      apiService.exportTimeSheet.and.returnValue(of(blob));
      component.exportForm = { group: 'Engineering', fromDate: '', toDate: '' };

      spyOn(window.URL, 'createObjectURL').and.returnValue('blob:test');
      spyOn(window.URL, 'revokeObjectURL');
      spyOn(document.body, 'appendChild');
      spyOn(document.body, 'removeChild');

      component.exportTimeSheet();

      expect(component.exporting).toBeFalse(); // Should be false after completion
    });

    it('handles export error', () => {
      apiService.exportTimeSheet.and.returnValue(throwError(() => new Error('Export failed')));
      component.exportForm = { group: 'Engineering', fromDate: '', toDate: '' };
      spyOn(console, 'error');

      component.exportTimeSheet();

      expect(window.alert).toHaveBeenCalledWith('Unable to export time sheet. Please try again.');
      expect(component.exporting).toBeFalse();
    });
  });
});

