import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { NgbAlertModule } from '@ng-bootstrap/ng-bootstrap';
import { AuthService } from '../../core/auth.service';
import { ToastService } from '../../core/toast.service';

interface RegisterModel {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  phone?: string;
  profileImageFile?: File | null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, NgbAlertModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css'],
})
export class Register {
  model: RegisterModel = {
    fullName: '',
    email: '',
    password: '',
    confirmPassword: '',
    phone: '',
  };

  submitting = false;
  showPassword = false;
  readonly phonePattern = '^\\+?[0-9\\s-]{7,20}$';

  constructor(
    private readonly authService: AuthService,
    private readonly toast: ToastService,
  ) {}

  get passwordsMismatch(): boolean {
    return !!this.model.password &&
      !!this.model.confirmPassword &&
      this.model.password !== this.model.confirmPassword;
  }

  onSubmit(form: NgForm) {
    if (this.submitting) {
      return;
    }

    if (form.invalid) {
      Object.values(form.controls).forEach(control => control.markAsTouched());
      this.toast.error('Please fix the highlighted fields before continuing.');
      return;
    }

    if (this.passwordsMismatch) {
      this.toast.error('Passwords do not match.');
      return;
    }

    this.submitting = true;

    this.authService
      .register({
        fullName: this.model.fullName,
        email: this.model.email,
        password: this.model.password,
        confirmPassword: this.model.confirmPassword,
        phoneNumber: this.model.phone,
        profileImageFile: this.model.profileImageFile ?? null,
      })
      .subscribe({
        next: (res) => {
          this.submitting = false;
          if (!res.success) {
            this.toast.error(res.message || 'Unable to create account. Please try again.');
            return;
          }
          this.toast.success(res.message || 'Account created successfully. You can now log in.');
          this.model.password = '';
          this.model.confirmPassword = '';
          this.model.profileImageFile = null;
        },
        error: (err) => {
          this.submitting = false;
          const message = err?.error?.message || 'Unable to create account. Please try again.';
          this.toast.error(message);
        },
      });
  }

  onProfileImageSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files && input.files.length > 0 ? input.files[0] : null;
    this.model.profileImageFile = file;
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }
}
