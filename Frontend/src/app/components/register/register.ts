import { Component } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class RegisterComponent {
  registerForm: FormGroup;
  error: string = '';
  success: string = '';

  constructor(private fb: FormBuilder, private api: ApiService, private router: Router) {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      employeeNumber: ['', [Validators.required, Validators.pattern('^[0-9]+$')]],
      group: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.registerForm.valid) {
      const formValue = this.registerForm.value;
      
      if (formValue.password !== formValue.confirmPassword) {
        this.error = 'Passwords do not match';
        return;
      }

      this.api.register({
        EmailAddress: formValue.email,
        Password: formValue.password,
        ConfirmPassword: formValue.confirmPassword,
        FirstName: formValue.firstName,
        LastName: formValue.lastName,
        EmployeeNumber: parseInt(formValue.employeeNumber, 10),
        Group: formValue.group
      }).subscribe({
        next: (res) => {
          this.success = 'Registration successful! Redirecting to login...';
          setTimeout(() => this.router.navigate(['/login']), 2000);
        },
        error: (err) => {
          if (err.error && typeof err.error === 'string') {
            this.error = err.error;
          } else if (err.error && err.error.errors) {
            this.error = Object.values(err.error.errors).join(', ');
          } else {
            this.error = 'Registration failed';
          }
        }
      });
    }
  }
}
