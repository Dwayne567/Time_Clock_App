import { Component } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common'; // Import CommonModule for *ngIf
import { ApiService } from '../../services/api';

@Component({
  selector: 'app-login',
  standalone: true, // Should be true if using standalone components
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css'] // Note: styleUrl -> styleUrls (array) or styleUrl (string) in newer versions. styleUrl is valid in v17+
})
export class LoginComponent {
  loginForm: FormGroup;
  error: string = '';

  constructor(private fb: FormBuilder, private api: ApiService, private router: Router) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      this.api.login({
        EmailAddress: this.loginForm.value.email,
        Password: this.loginForm.value.password
      }).subscribe({
        next: (res) => {
          console.log('Login success', res);
          const resolvedUserId = res.userId || res.UserId;
          if (resolvedUserId) {
            localStorage.setItem('userId', resolvedUserId);
          }
          // Navigate to dashboard
          // If the backend requires UserId in the URL for dashboard (it does based on my refactor),
          // pass it.
          // Note: My DashboardController API [HttpGet("Index/{userId?}")] accepts it.
          if (resolvedUserId) {
            this.router.navigate(['/dashboard'], { queryParams: { userId: resolvedUserId } });
          } else {
            this.router.navigate(['/dashboard']);
          }
        },
        error: (err) => {
          console.error('Login error', err);
          this.error = 'Invalid email or password';
        }
      });
    }
  }
}
