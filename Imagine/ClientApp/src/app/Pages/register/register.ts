import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { NgbAlertModule } from '@ng-bootstrap/ng-bootstrap';

interface RegisterModel {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  phone?: string;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, NgbAlertModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
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
  errorMessage = '';
  successMessage = '';
  showPassword = false;

  onSubmit(form: NgForm) {
    if (this.submitting) {
      return;
    }

    if (form.invalid) {
      this.errorMessage = 'Please fill in all required fields.';
      this.successMessage = '';
      return;
    }

    if (this.model.password !== this.model.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      this.successMessage = '';
      return;
    }

    this.errorMessage = '';
    this.submitting = true;

    setTimeout(() => {
      this.submitting = false;
      this.successMessage = 'Account created successfully. You can now log in.';
      // Placeholder: handle real registration here
    }, 900);
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }
}
