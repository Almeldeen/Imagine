import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { NgbAlertModule } from '@ng-bootstrap/ng-bootstrap';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';
import { ToastService } from '../../core/toast.service';

interface LoginModel {
  identifier: string;
  password: string;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, NgbAlertModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
})
export class Login {
  model: LoginModel = {
    identifier: '',
    password: '',
  };

  submitting = false;
  showPassword = false;

  constructor(
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly authService: AuthService,
    private readonly toast: ToastService,
  ) {}

  onSubmit(form: NgForm) {
    if (this.submitting) {
      return;
    }

    if (form.invalid) {
      Object.values(form.controls).forEach(control => control.markAsTouched());
      this.toast.error('Please fix the highlighted fields before continuing.');
      return;
    }

    this.submitting = true;

    this.authService.login({
      identifier: this.model.identifier,
      password: this.model.password,
    }).subscribe({
      next: (res) => {
        this.submitting = false;
        if (!res.success || !res.data) {
          this.toast.error(res.message || 'Invalid email/phone or password.');
          return;
        }
        const roles = res.data.roles || [];
        this.toast.success('Welcome back.');
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        if (roles.includes('Admin')) {
          this.router.navigate(['/admin/Home']);
        } else if (returnUrl) {
          this.router.navigateByUrl(returnUrl);
        } else {
          this.router.navigate(['/client/dashboard']);
        }
      },
      error: () => {
        this.submitting = false;
        this.toast.error('Invalid email/phone or password.');
      },
    });
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }
}
