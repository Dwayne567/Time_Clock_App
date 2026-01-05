import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';

import { DayEntriesComponent } from './day-entries';
import { ApiService } from '../../services/api';

describe('DayEntriesComponent', () => {
  let component: DayEntriesComponent;
  let fixture: ComponentFixture<DayEntriesComponent>;
  let apiService: jasmine.SpyObj<ApiService>;

  beforeEach(async () => {
    const apiSpy = jasmine.createSpyObj('ApiService', ['clockInOut', 'deleteDay']);

    await TestBed.configureTestingModule({
      imports: [DayEntriesComponent, HttpClientTestingModule],
      providers: [{ provide: ApiService, useValue: apiSpy }]
    }).compileComponents();

    fixture = TestBed.createComponent(DayEntriesComponent);
    component = fixture.componentInstance;
    apiService = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;

    // Default setup
    component.dayEntries = [];
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

  // ========== isClockedIn Tests ==========
  describe('isClockedIn', () => {
    it('returns true when an open entry exists for today', () => {
      const today = new Date();
      component.dayEntries = [
        {
          date: today.toISOString(),
          dayStartTime: '08:00:00',
          dayEndTime: null
        }
      ];

      expect(component.isClockedIn).toBeTrue();
    });

    it('returns false when no entries exist', () => {
      component.dayEntries = [];
      expect(component.isClockedIn).toBeFalse();
    });

    it('returns false when entry has an end time', () => {
      const today = new Date();
      component.dayEntries = [
        {
          date: today.toISOString(),
          dayStartTime: '08:00:00',
          dayEndTime: '17:00:00'
        }
      ];

      expect(component.isClockedIn).toBeFalse();
    });

    it('returns false when entry is from a different day', () => {
      const yesterday = new Date();
      yesterday.setDate(yesterday.getDate() - 1);
      component.dayEntries = [
        {
          date: yesterday.toISOString(),
          dayStartTime: '08:00:00',
          dayEndTime: null
        }
      ];

      expect(component.isClockedIn).toBeFalse();
    });
  });

  // ========== findOpenEntry Tests ==========
  describe('findOpenEntry', () => {
    it('returns undefined when dayEntries is empty', () => {
      component.dayEntries = [];
      expect(component.findOpenEntry()).toBeUndefined();
    });

    it('returns null when dayEntries is null/undefined', () => {
      component.dayEntries = null as any;
      expect(component.findOpenEntry()).toBeNull();
    });

    it('returns entry with start time and no end time for today', () => {
      const today = new Date();
      component.dayEntries = [
        {
          id: 1,
          date: today.toISOString(),
          dayStartTime: '09:00:00',
          dayEndTime: null
        }
      ];

      const entry = component.findOpenEntry();
      expect(entry).toBeTruthy();
      expect(entry.id).toBe(1);
      expect(entry.dayStartTime).toBe('09:00:00');
    });

    it('returns undefined when all entries are closed', () => {
      const today = new Date();
      component.dayEntries = [
        {
          id: 1,
          date: today.toISOString(),
          dayStartTime: '08:00:00',
          dayEndTime: '12:00:00'
        },
        {
          id: 2,
          date: today.toISOString(),
          dayStartTime: '13:00:00',
          dayEndTime: '17:00:00'
        }
      ];

      expect(component.findOpenEntry()).toBeUndefined();
    });
  });

  // ========== findTodayEntry Tests ==========
  describe('findTodayEntry', () => {
    it('returns null when dayEntries is empty', () => {
      component.dayEntries = [];
      expect(component.findTodayEntry()).toBeNull();
    });

    it('returns the most recent entry for today', () => {
      const today = new Date();
      component.dayEntries = [
        {
          id: 1,
          date: today.toISOString(),
          dayStartTime: '08:00:00',
          dayEndTime: '12:00:00'
        },
        {
          id: 2,
          date: today.toISOString(),
          dayStartTime: '13:00:00',
          dayEndTime: '17:00:00'
        }
      ];

      const entry = component.findTodayEntry();
      expect(entry).toBeTruthy();
      expect(entry.id).toBe(2);
    });

    it('returns null when no entries exist for today', () => {
      const yesterday = new Date();
      yesterday.setDate(yesterday.getDate() - 1);
      component.dayEntries = [
        {
          id: 1,
          date: yesterday.toISOString(),
          dayStartTime: '08:00:00',
          dayEndTime: '17:00:00'
        }
      ];

      expect(component.findTodayEntry()).toBeNull();
    });
  });

  // ========== formatTime12Hour Tests ==========
  describe('formatTime12Hour', () => {
    it('converts 24-hour time to 12-hour format with AM', () => {
      expect(component.formatTime12Hour('09:30:00')).toBe('9:30 AM');
      expect(component.formatTime12Hour('00:00:00')).toBe('12:00 AM');
      expect(component.formatTime12Hour('11:59:00')).toBe('11:59 AM');
    });

    it('converts 24-hour time to 12-hour format with PM', () => {
      expect(component.formatTime12Hour('12:00:00')).toBe('12:00 PM');
      expect(component.formatTime12Hour('14:05:00')).toBe('2:05 PM');
      expect(component.formatTime12Hour('23:59:00')).toBe('11:59 PM');
    });

    it('returns empty string for null input', () => {
      expect(component.formatTime12Hour(null)).toBe('');
    });
  });

  // ========== clockIn Tests ==========
  describe('clockIn', () => {
    it('calls api.clockInOut with correct payload', () => {
      apiService.clockInOut.and.returnValue(of({}));
      spyOn(component.refresh, 'emit');

      component.clockIn();

      expect(apiService.clockInOut).toHaveBeenCalled();
      const payload = apiService.clockInOut.calls.mostRecent().args[0];
      expect(payload.DayEntry).toBeDefined();
      expect(payload.DayEntry.AppUserId).toBe('test-user-123');
      expect(payload.DayEntry.DayStartTime).toBeTruthy();
      expect(payload.DayEntry.DayEndTime).toBeNull();
    });

    it('emits refresh event on success', () => {
      apiService.clockInOut.and.returnValue(of({}));
      spyOn(component.refresh, 'emit');

      component.clockIn();

      expect(component.refresh.emit).toHaveBeenCalled();
    });

    it('does not emit refresh on error', () => {
      apiService.clockInOut.and.returnValue(throwError(() => new Error('API Error')));
      spyOn(component.refresh, 'emit');
      spyOn(console, 'error');

      component.clockIn();

      expect(component.refresh.emit).not.toHaveBeenCalled();
      expect(console.error).toHaveBeenCalled();
    });

    it('does not call API when userId cannot be resolved', () => {
      component.userId = null;
      spyOn(console, 'error');

      component.clockIn();

      expect(apiService.clockInOut).not.toHaveBeenCalled();
      expect(console.error).toHaveBeenCalledWith('Unable to determine the user ID for clock in.');
    });
  });

  // ========== clockOut Tests ==========
  describe('clockOut', () => {
    it('calls api.clockInOut with correct payload when open entry exists', () => {
      const today = new Date();
      component.dayEntries = [
        {
          id: 99,
          appUserId: 'test-user-123',
          date: today.toISOString(),
          dayStartTime: '08:00:00',
          dayEndTime: null
        }
      ];
      apiService.clockInOut.and.returnValue(of({}));
      spyOn(component.refresh, 'emit');

      component.clockOut();

      expect(apiService.clockInOut).toHaveBeenCalled();
      const payload = apiService.clockInOut.calls.mostRecent().args[0];
      expect(payload.DayEntry.Id).toBe(99);
      expect(payload.DayEntry.DayStartTime).toBe('08:00:00');
      expect(payload.DayEntry.DayEndTime).toBeTruthy();
    });

    it('emits refresh event on success', () => {
      const today = new Date();
      component.dayEntries = [
        {
          id: 1,
          date: today.toISOString(),
          dayStartTime: '08:00:00',
          dayEndTime: null
        }
      ];
      apiService.clockInOut.and.returnValue(of({}));
      spyOn(component.refresh, 'emit');

      component.clockOut();

      expect(component.refresh.emit).toHaveBeenCalled();
    });

    it('does not call API when no open entry exists', () => {
      component.dayEntries = [];
      spyOn(console, 'error');

      component.clockOut();

      expect(apiService.clockInOut).not.toHaveBeenCalled();
      expect(console.error).toHaveBeenCalledWith('No open entry found to clock out');
    });
  });

  // ========== deleteDayEntry Tests ==========
  describe('deleteDayEntry', () => {
    it('calls api.deleteDay on confirmation', () => {
      spyOn(window, 'confirm').and.returnValue(true);
      apiService.deleteDay.and.returnValue(of({}));
      spyOn(component.refresh, 'emit');

      component.deleteDayEntry(123);

      expect(apiService.deleteDay).toHaveBeenCalledWith(123);
      expect(component.refresh.emit).toHaveBeenCalled();
    });

    it('does not call API when user cancels', () => {
      spyOn(window, 'confirm').and.returnValue(false);

      component.deleteDayEntry(123);

      expect(apiService.deleteDay).not.toHaveBeenCalled();
    });

    it('does not emit refresh on error', () => {
      spyOn(window, 'confirm').and.returnValue(true);
      apiService.deleteDay.and.returnValue(throwError(() => new Error('API Error')));
      spyOn(component.refresh, 'emit');
      spyOn(console, 'error');

      component.deleteDayEntry(123);

      expect(component.refresh.emit).not.toHaveBeenCalled();
      expect(console.error).toHaveBeenCalled();
    });
  });
});
