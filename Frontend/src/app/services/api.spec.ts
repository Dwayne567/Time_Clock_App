import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { ApiService } from './api';

describe('ApiService', () => {
  let service: ApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ApiService]
    });
    service = TestBed.inject(ApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ========== Authentication Tests ==========
  describe('login', () => {
    it('sends POST request to correct URL', () => {
      const credentials = { EmailAddress: 'test@example.com', Password: 'pass123' };

      service.login(credentials).subscribe();

      const req = httpMock.expectOne('api/Account/Login');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(credentials);
      req.flush({ userId: 'user-123' });
    });

    it('returns response on success', () => {
      const mockResponse = { userId: 'user-123', token: 'abc' };

      service.login({}).subscribe(res => {
        expect(res).toEqual(mockResponse);
      });

      const req = httpMock.expectOne('api/Account/Login');
      req.flush(mockResponse);
    });
  });

  describe('register', () => {
    it('sends POST request with text response type', () => {
      const data = { EmailAddress: 'new@example.com', Password: 'pass123' };

      service.register(data).subscribe();

      const req = httpMock.expectOne('api/Account/Register');
      expect(req.request.method).toBe('POST');
      expect(req.request.responseType).toBe('text');
      expect(req.request.body).toEqual(data);
      req.flush('Success');
    });
  });

  describe('logout', () => {
    it('sends POST request to logout endpoint', () => {
      service.logout().subscribe();

      const req = httpMock.expectOne('api/Account/Logout');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({});
      req.flush({});
    });
  });

  // ========== Dashboard Tests ==========
  describe('getDashboard', () => {
    it('sends GET request without parameters', () => {
      service.getDashboard().subscribe();

      const req = httpMock.expectOne('api/Dashboard/Index');
      expect(req.request.method).toBe('GET');
      req.flush({});
    });

    it('includes WeekSelect parameter when provided', () => {
      service.getDashboard('2026-01-04').subscribe();

      const req = httpMock.expectOne(r => r.url === 'api/Dashboard/Index');
      expect(req.request.params.get('WeekSelect')).toBe('2026-01-04');
      req.flush({});
    });

    it('includes userId parameter when provided', () => {
      service.getDashboard(undefined, 'user-123').subscribe();

      const req = httpMock.expectOne(r => r.url === 'api/Dashboard/Index');
      expect(req.request.params.get('userId')).toBe('user-123');
      req.flush({});
    });

    it('includes both parameters when provided', () => {
      service.getDashboard('2026-01-04', 'user-123').subscribe();

      const req = httpMock.expectOne(r => r.url === 'api/Dashboard/Index');
      expect(req.request.params.get('WeekSelect')).toBe('2026-01-04');
      expect(req.request.params.get('userId')).toBe('user-123');
      req.flush({});
    });
  });

  describe('clockInOut', () => {
    it('sends POST request with day entry data', () => {
      const data = { DayEntry: { Id: 1, DayStartTime: '08:00:00' } };

      service.clockInOut(data).subscribe();

      const req = httpMock.expectOne('api/Dashboard/ClockInOut');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(data);
      req.flush({});
    });
  });

  // ========== Task Entry Tests ==========
  describe('addTaskEntry', () => {
    it('sends POST request with text response type', () => {
      const data = { TaskEntry: { JobId: 1, TaskName: 'Dev' } };

      service.addTaskEntry(data).subscribe();

      const req = httpMock.expectOne('api/Dashboard/AddTaskEntry');
      expect(req.request.method).toBe('POST');
      expect(req.request.responseType).toBe('text');
      req.flush('Success');
    });
  });

  describe('deleteTaskEntry', () => {
    it('sends DELETE request with entry id', () => {
      service.deleteTaskEntry(123).subscribe();

      const req = httpMock.expectOne('api/Dashboard/DeleteTaskEntry/123');
      expect(req.request.method).toBe('DELETE');
      expect(req.request.responseType).toBe('text');
      req.flush('Deleted');
    });
  });

  // ========== Leave Entry Tests ==========
  describe('addLeave', () => {
    it('sends POST request with leave data', () => {
      const data = { LeaveEntry: { LeaveType: 'PTO', LeaveDuration: 8 } };

      service.addLeave(data).subscribe();

      const req = httpMock.expectOne('api/Dashboard/AddLeave');
      expect(req.request.method).toBe('POST');
      expect(req.request.responseType).toBe('text');
      req.flush('Success');
    });
  });

  describe('deleteLeave', () => {
    it('sends DELETE request with leave id', () => {
      service.deleteLeave(456).subscribe();

      const req = httpMock.expectOne('api/Dashboard/DeleteLeave/456');
      expect(req.request.method).toBe('DELETE');
      req.flush('Deleted');
    });
  });

  // ========== Day Entry Tests ==========
  describe('deleteDay', () => {
    it('sends DELETE request with day id', () => {
      service.deleteDay(789).subscribe();

      const req = httpMock.expectOne('api/Dashboard/DeleteDay/789');
      expect(req.request.method).toBe('DELETE');
      req.flush('Deleted');
    });
  });

  // ========== Job Tests ==========
  describe('addJob', () => {
    it('sends POST request with job data', () => {
      const data = { JobModel: { JobNumber: 'J001', JobName: 'Test Job' } };

      service.addJob(data).subscribe();

      const req = httpMock.expectOne('api/Dashboard/AddJob');
      expect(req.request.method).toBe('POST');
      expect(req.request.responseType).toBe('text');
      req.flush('Success');
    });
  });

  describe('getJobs', () => {
    it('sends GET request without search term', () => {
      service.getJobs().subscribe();

      const req = httpMock.expectOne('api/Jobs');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('includes search term when provided', () => {
      service.getJobs('test').subscribe();

      const req = httpMock.expectOne(r => r.url === 'api/Jobs');
      expect(req.request.params.get('searchTerm')).toBe('test');
      req.flush([]);
    });
  });

  describe('deleteJob', () => {
    it('sends DELETE request with job id', () => {
      service.deleteJob(100).subscribe();

      const req = httpMock.expectOne('api/Jobs/100');
      expect(req.request.method).toBe('DELETE');
      req.flush('Deleted');
    });
  });

  // ========== Task Tests ==========
  describe('getTasks', () => {
    it('sends GET request to tasks endpoint', () => {
      service.getTasks().subscribe();

      const req = httpMock.expectOne('api/Tasks');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });
  });

  describe('createTask', () => {
    it('sends POST request with task description', () => {
      service.createTask('New Task').subscribe();

      const req = httpMock.expectOne('api/Tasks');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ taskDescription: 'New Task' });
      expect(req.request.responseType).toBe('text');
      req.flush('Created');
    });
  });

  describe('deleteTask', () => {
    it('sends DELETE request with task id', () => {
      service.deleteTask(200).subscribe();

      const req = httpMock.expectOne('api/Tasks/200');
      expect(req.request.method).toBe('DELETE');
      req.flush('Deleted');
    });
  });

  // ========== Export Tests ==========
  describe('exportTimeSheet', () => {
    it('sends GET request with group parameter', () => {
      service.exportTimeSheet('Engineering').subscribe();

      const req = httpMock.expectOne(r => r.url === 'api/Dashboard/ExportTimeSheet');
      expect(req.request.method).toBe('GET');
      expect(req.request.params.get('group')).toBe('Engineering');
      expect(req.request.responseType).toBe('blob');
      req.flush(new Blob(['csv data'], { type: 'text/csv' }));
    });

    it('includes date range when provided', () => {
      service.exportTimeSheet('Engineering', '2026-01-01', '2026-01-31').subscribe();

      const req = httpMock.expectOne(r => r.url === 'api/Dashboard/ExportTimeSheet');
      expect(req.request.params.get('group')).toBe('Engineering');
      expect(req.request.params.get('fromDate')).toBe('2026-01-01');
      expect(req.request.params.get('toDate')).toBe('2026-01-31');
      req.flush(new Blob());
    });

    it('returns blob response', () => {
      const mockBlob = new Blob(['test,data'], { type: 'text/csv' });

      service.exportTimeSheet('Engineering').subscribe(blob => {
        expect(blob instanceof Blob).toBeTrue();
      });

      const req = httpMock.expectOne(r => r.url === 'api/Dashboard/ExportTimeSheet');
      req.flush(mockBlob);
    });
  });
});
