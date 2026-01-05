import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';

import { LeaveEntriesComponent } from './leave-entries';
import { ApiService } from '../../services/api';

describe('LeaveEntriesComponent', () => {
  let component: LeaveEntriesComponent;
  let fixture: ComponentFixture<LeaveEntriesComponent>;
  let apiService: jasmine.SpyObj<ApiService>;

  beforeEach(async () => {
    const apiSpy = jasmine.createSpyObj('ApiService', ['addLeave', 'deleteLeave']);

    await TestBed.configureTestingModule({
      imports: [LeaveEntriesComponent, HttpClientTestingModule],
      providers: [{ provide: ApiService, useValue: apiSpy }]
    }).compileComponents();

    fixture = TestBed.createComponent(LeaveEntriesComponent);
    component = fixture.componentInstance;
    apiService = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;

    // Default setup
    component.leaveEntries = [];
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
      component.newLeaveEntry = { leaveType: 'Sick', leaveDuration: 4, status: 'Approved' };

      component.toggleAddForm();

      expect(component.showAddForm).toBeFalse();
      expect(component.newLeaveEntry.leaveType).toBe('PTO');
      expect(component.newLeaveEntry.leaveDuration).toBe(8);
      expect(component.newLeaveEntry.status).toBe('Pending');
    });
  });

  describe('resetForm', () => {
    it('resets the form to default values', () => {
      component.newLeaveEntry = { leaveType: 'Vacation', leaveDuration: 16, status: 'Approved' };

      component.resetForm();

      expect(component.newLeaveEntry).toEqual({
        leaveType: 'PTO',
        leaveDuration: 8,
        status: 'Pending'
      });
    });
  });

  // ========== leaveTypes Tests ==========
  describe('leaveTypes', () => {
    it('has expected leave type options', () => {
      expect(component.leaveTypes).toContain('PTO');
      expect(component.leaveTypes).toContain('Sick');
      expect(component.leaveTypes).toContain('Vacation');
      expect(component.leaveTypes).toContain('Personal');
      expect(component.leaveTypes).toContain('UPTO');
      expect(component.leaveTypes.length).toBe(5);
    });
  });

  // ========== addLeaveEntry Tests ==========
  describe('addLeaveEntry', () => {
    it('calls api.addLeave with correct payload', () => {
      component.newLeaveEntry = { leaveType: 'Sick', leaveDuration: 4, status: 'Pending' };
      apiService.addLeave.and.returnValue(of({}));
      spyOn(component.refresh, 'emit');

      component.addLeaveEntry();

      expect(apiService.addLeave).toHaveBeenCalled();
      const payload = apiService.addLeave.calls.mostRecent().args[0];
      expect(payload.LeaveEntry).toBeDefined();
      expect(payload.LeaveEntry.AppUserId).toBe('test-user-123');
      expect(payload.LeaveEntry.LeaveType).toBe('Sick');
      expect(payload.LeaveEntry.LeaveDuration).toBe(4);
      expect(payload.LeaveEntry.Status).toBe('Pending');
    });

    it('includes date information in payload', () => {
      apiService.addLeave.and.returnValue(of({}));

      component.addLeaveEntry();

      const payload = apiService.addLeave.calls.mostRecent().args[0];
      expect(payload.LeaveEntry.WeekOf).toBeTruthy();
      expect(payload.LeaveEntry.Date).toBeTruthy();
      expect(payload.LeaveEntry.DayName).toBeTruthy();
    });

    it('emits refresh and resets form on success', () => {
      apiService.addLeave.and.returnValue(of({}));
      spyOn(component.refresh, 'emit');
      component.showAddForm = true;
      component.newLeaveEntry = { leaveType: 'Vacation', leaveDuration: 16, status: 'Pending' };

      component.addLeaveEntry();

      expect(component.refresh.emit).toHaveBeenCalled();
      expect(component.showAddForm).toBeFalse();
      expect(component.newLeaveEntry.leaveType).toBe('PTO');
      expect(component.newLeaveEntry.leaveDuration).toBe(8);
    });

    it('does not emit refresh on error', () => {
      apiService.addLeave.and.returnValue(throwError(() => new Error('API Error')));
      spyOn(component.refresh, 'emit');
      spyOn(console, 'error');

      component.addLeaveEntry();

      expect(component.refresh.emit).not.toHaveBeenCalled();
      expect(console.error).toHaveBeenCalled();
    });
  });

  // ========== deleteLeaveEntry Tests ==========
  describe('deleteLeaveEntry', () => {
    it('calls api.deleteLeave on confirmation', () => {
      spyOn(window, 'confirm').and.returnValue(true);
      apiService.deleteLeave.and.returnValue(of({}));
      spyOn(component.refresh, 'emit');

      component.deleteLeaveEntry(789);

      expect(apiService.deleteLeave).toHaveBeenCalledWith(789);
      expect(component.refresh.emit).toHaveBeenCalled();
    });

    it('does not call API when user cancels', () => {
      spyOn(window, 'confirm').and.returnValue(false);

      component.deleteLeaveEntry(789);

      expect(apiService.deleteLeave).not.toHaveBeenCalled();
    });

    it('does not emit refresh on error', () => {
      spyOn(window, 'confirm').and.returnValue(true);
      apiService.deleteLeave.and.returnValue(throwError(() => new Error('API Error')));
      spyOn(component.refresh, 'emit');
      spyOn(console, 'error');

      component.deleteLeaveEntry(789);

      expect(component.refresh.emit).not.toHaveBeenCalled();
      expect(console.error).toHaveBeenCalled();
    });
  });

  // ========== Input/Output Tests ==========
  describe('inputs', () => {
    it('accepts leaveEntries input', () => {
      const entries = [
        { id: 1, leaveType: 'PTO', leaveDuration: 8, status: 'Approved' },
        { id: 2, leaveType: 'Sick', leaveDuration: 4, status: 'Pending' }
      ];
      component.leaveEntries = entries;

      expect(component.leaveEntries).toEqual(entries);
    });

    it('accepts userId input', () => {
      component.userId = 'custom-user-456';
      expect(component.userId).toBe('custom-user-456');
    });
  });

  // ========== Default Values Tests ==========
  describe('default values', () => {
    it('initializes with default form values', () => {
      // Create fresh component to test defaults
      const freshFixture = TestBed.createComponent(LeaveEntriesComponent);
      const freshComponent = freshFixture.componentInstance;

      expect(freshComponent.showAddForm).toBeFalse();
      expect(freshComponent.newLeaveEntry.leaveType).toBe('PTO');
      expect(freshComponent.newLeaveEntry.leaveDuration).toBe(8);
      expect(freshComponent.newLeaveEntry.status).toBe('Pending');
    });
  });
});
