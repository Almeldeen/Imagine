import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { NgbAlertModule } from '@ng-bootstrap/ng-bootstrap';

interface LoginModel {
  email: string;
  password: string;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, NgbAlertModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  model: LoginModel = {
    email: '',
    password: '',
  };

  submitting = false;
  errorMessage = '';
  showPassword = false;

  onSubmit(form: NgForm) {
    if (this.submitting) {
      return;
    }

    if (form.invalid) {
      this.errorMessage = 'Please fill in all required fields.';
      return;
    }

    this.errorMessage = '';
    this.submitting = true;

    setTimeout(() => {
      this.submitting = false;
      // Placeholder: handle real login here
    }, 800);
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }
}
