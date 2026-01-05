import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { RegisterComponent } from './register';
import { ApiService } from '../../services/api';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let apiService: jasmine.SpyObj<ApiService>;
  let router: Router;

  beforeEach(async () => {
    const apiSpy = jasmine.createSpyObj('ApiService', ['register']);

    await TestBed.configureTestingModule({
      imports: [RegisterComponent, HttpClientTestingModule, RouterTestingModule],
      providers: [{ provide: ApiService, useValue: apiSpy }]
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    apiService = TestBed.inject(ApiService) as jasmine.SpyObj<ApiService>;
    router = TestBed.inject(Router);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // ========== Form Initialization Tests ==========
  describe('form initialization', () => {
    it('creates a form with all required controls', () => {
      expect(component.registerForm.contains('email')).toBeTrue();
      expect(component.registerForm.contains('password')).toBeTrue();
      expect(component.registerForm.contains('confirmPassword')).toBeTrue();
      expect(component.registerForm.contains('firstName')).toBeTrue();
      expect(component.registerForm.contains('lastName')).toBeTrue();
      expect(component.registerForm.contains('employeeNumber')).toBeTrue();
      expect(component.registerForm.contains('group')).toBeTrue();
    });

    it('initializes with empty values', () => {
      expect(component.registerForm.get('email')?.value).toBe('');
      expect(component.registerForm.get('password')?.value).toBe('');
    });

    it('initializes with empty error and success messages', () => {
      expect(component.error).toBe('');
      expect(component.success).toBe('');
    });
  });

  // ========== Form Validation Tests ==========
  describe('form validation', () => {
    it('requires email field', () => {
      const control = component.registerForm.get('email');
      control?.setValue('');
      expect(control?.valid).toBeFalse();
    });

    it('validates email format', () => {
      const control = component.registerForm.get('email');
      control?.setValue('invalid-email');
      expect(control?.errors?.['email']).toBeTruthy();
    });

    it('requires password with minimum 6 characters', () => {
      const control = component.registerForm.get('password');
      control?.setValue('12345');
      expect(control?.valid).toBeFalse();
      expect(control?.errors?.['minlength']).toBeTruthy();
    });

    it('accepts password with 6 or more characters', () => {
      const control = component.registerForm.get('password');
      control?.setValue('123456');
      expect(control?.valid).toBeTrue();
    });

    it('requires confirmPassword field', () => {
      const control = component.registerForm.get('confirmPassword');
      control?.setValue('');
      expect(control?.valid).toBeFalse();
    });

    it('requires firstName field', () => {
      const control = component.registerForm.get('firstName');
      control?.setValue('');
      expect(control?.valid).toBeFalse();
    });

    it('requires lastName field', () => {
      const control = component.registerForm.get('lastName');
      control?.setValue('');
      expect(control?.valid).toBeFalse();
    });

    it('requires employeeNumber to be numeric', () => {
      const control = component.registerForm.get('employeeNumber');
      control?.setValue('abc');
      expect(control?.errors?.['pattern']).toBeTruthy();
    });

    it('accepts numeric employeeNumber', () => {
      const control = component.registerForm.get('employeeNumber');
      control?.setValue('12345');
      expect(control?.valid).toBeTrue();
    });

    it('requires group field', () => {
      const control = component.registerForm.get('group');
      control?.setValue('');
      expect(control?.valid).toBeFalse();
    });
  });

  // ========== onSubmit Tests ==========
  describe('onSubmit', () => {
    const validFormData = {
      email: 'test@example.com',
      password: 'password123',
      confirmPassword: 'password123',
      firstName: 'John',
      lastName: 'Doe',
      employeeNumber: '12345',
      group: 'Engineering'
    };

    beforeEach(() => {
      component.registerForm.setValue(validFormData);
    });

    it('does not call API when form is invalid', () => {
      component.registerForm.get('email')?.setValue('');

      component.onSubmit();

      expect(apiService.register).not.toHaveBeenCalled();
    });

    it('sets error when passwords do not match', () => {
      component.registerForm.get('confirmPassword')?.setValue('different');

      component.onSubmit();

      expect(component.error).toBe('Passwords do not match');
      expect(apiService.register).not.toHaveBeenCalled();
    });

    it('calls API with correct data when form is valid', () => {
      apiService.register.and.returnValue(of('Success'));

      component.onSubmit();

      expect(apiService.register).toHaveBeenCalledWith({
        EmailAddress: 'test@example.com',
        Password: 'password123',
        ConfirmPassword: 'password123',
        FirstName: 'John',
        LastName: 'Doe',
        EmployeeNumber: 12345,
        Group: 'Engineering'
      });
    });

    it('converts employeeNumber to integer', () => {
      apiService.register.and.returnValue(of('Success'));

      component.onSubmit();

      const callArgs = apiService.register.calls.mostRecent().args[0];
      expect(typeof callArgs.EmployeeNumber).toBe('number');
      expect(callArgs.EmployeeNumber).toBe(12345);
    });

    it('sets success message on registration success', () => {
      apiService.register.and.returnValue(of('Success'));

      component.onSubmit();

      expect(component.success).toBe('Registration successful! Redirecting to login...');
    });

    it('navigates to login after successful registration', fakeAsync(() => {
      apiService.register.and.returnValue(of('Success'));
      spyOn(router, 'navigate');

      component.onSubmit();
      tick(2000);

      expect(router.navigate).toHaveBeenCalledWith(['/login']);
    }));

    it('sets error message from string error response', () => {
      apiService.register.and.returnValue(
        throwError(() => ({ error: 'Email already exists' }))
      );

      component.onSubmit();

      expect(component.error).toBe('Email already exists');
    });

    it('sets error message from validation errors object', () => {
      apiService.register.and.returnValue(
        throwError(() => ({
          error: { errors: { Email: 'Invalid email', Password: 'Too weak' } }
        }))
      );

      component.onSubmit();

      expect(component.error).toContain('Invalid email');
      expect(component.error).toContain('Too weak');
    });

    it('sets generic error message for unknown error format', () => {
      apiService.register.and.returnValue(
        throwError(() => ({}))
      );

      component.onSubmit();

      expect(component.error).toBe('Registration failed');
    });
  });
});
