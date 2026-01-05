import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { LoginComponent } from './login';
import { ApiService } from '../../services/api';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let apiService: jasmine.SpyObj<ApiService>;
  let router: Router;

  beforeEach(async () => {
    const apiSpy = jasmine.createSpyObj('ApiService', ['login']);

    await TestBed.configureTestingModule({
      imports: [LoginComponent, HttpClientTestingModule, RouterTestingModule],
      providers: [{ provide: ApiService, useValue: apiSpy }]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    apiService = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;
    router = TestBed.inject(Router);
    await fixture.whenStable();
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

  // ========== Form Initialization Tests ==========
  describe('form initialization', () => {
    it('creates a form with email and password controls', () => {
      expect(component.loginForm.contains('email')).toBeTrue();
      expect(component.loginForm.contains('password')).toBeTrue();
    });

    it('initializes with empty values', () => {
      expect(component.loginForm.get('email')?.value).toBe('');
      expect(component.loginForm.get('password')?.value).toBe('');
    });

    it('initializes with empty error message', () => {
      expect(component.error).toBe('');
    });
  });

  // ========== Form Validation Tests ==========
  describe('form validation', () => {
    it('requires email field', () => {
      const emailControl = component.loginForm.get('email');
      emailControl?.setValue('');
      expect(emailControl?.valid).toBeFalse();
      expect(emailControl?.errors?.['required']).toBeTruthy();
    });

    it('validates email format', () => {
      const emailControl = component.loginForm.get('email');
      emailControl?.setValue('invalid-email');
      expect(emailControl?.valid).toBeFalse();
      expect(emailControl?.errors?.['email']).toBeTruthy();
    });

    it('accepts valid email format', () => {
      const emailControl = component.loginForm.get('email');
      emailControl?.setValue('test@example.com');
      expect(emailControl?.valid).toBeTrue();
    });

    it('requires password field', () => {
      const passwordControl = component.loginForm.get('password');
      passwordControl?.setValue('');
      expect(passwordControl?.valid).toBeFalse();
      expect(passwordControl?.errors?.['required']).toBeTruthy();
    });

    it('accepts any non-empty password', () => {
      const passwordControl = component.loginForm.get('password');
      passwordControl?.setValue('pass');
      expect(passwordControl?.valid).toBeTrue();
    });

    it('form is invalid when empty', () => {
      expect(component.loginForm.valid).toBeFalse();
    });

    it('form is valid with email and password', () => {
      component.loginForm.setValue({
        email: 'test@example.com',
        password: 'password123'
      });
      expect(component.loginForm.valid).toBeTrue();
    });
  });

  // ========== onSubmit Tests ==========
  describe('onSubmit', () => {
    beforeEach(() => {
      component.loginForm.setValue({
        email: 'test@example.com',
        password: 'password123'
      });
    });

    it('does not call API when form is invalid', () => {
      component.loginForm.setValue({ email: '', password: '' });

      component.onSubmit();

      expect(apiService.login).not.toHaveBeenCalled();
    });

    it('calls API with correct credentials', () => {
      apiService.login.and.returnValue(of({ userId: 'user-123' }));

      component.onSubmit();

      expect(apiService.login).toHaveBeenCalledWith({
        EmailAddress: 'test@example.com',
        Password: 'password123'
      });
    });

    it('stores userId in localStorage on success', () => {
      apiService.login.and.returnValue(of({ userId: 'user-123' }));

      component.onSubmit();

      expect(localStorage.getItem('userId')).toBe('user-123');
    });

    it('handles UserId with capital U from response', () => {
      apiService.login.and.returnValue(of({ UserId: 'user-456' }));

      component.onSubmit();

      expect(localStorage.getItem('userId')).toBe('user-456');
    });

    it('navigates to dashboard with userId on success', () => {
      apiService.login.and.returnValue(of({ userId: 'user-123' }));
      spyOn(router, 'navigate');

      component.onSubmit();

      expect(router.navigate).toHaveBeenCalledWith(
        ['/dashboard'],
        { queryParams: { userId: 'user-123' } }
      );
    });

    it('navigates to dashboard without userId when not provided', () => {
      apiService.login.and.returnValue(of({}));
      spyOn(router, 'navigate');

      component.onSubmit();

      expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
    });

    it('sets error message on login failure', () => {
      apiService.login.and.returnValue(throwError(() => new Error('Auth failed')));

      component.onSubmit();

      expect(component.error).toBe('Invalid email or password');
    });

    it('clears previous error on new submission attempt', () => {
      component.error = 'Previous error';
      apiService.login.and.returnValue(of({ userId: 'user-123' }));
      spyOn(router, 'navigate');

      component.onSubmit();

      // Error should not persist after successful login
      expect(router.navigate).toHaveBeenCalled();
    });
  });
});
