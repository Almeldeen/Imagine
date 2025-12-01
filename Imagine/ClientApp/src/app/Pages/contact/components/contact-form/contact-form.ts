import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbAlertModule } from '@ng-bootstrap/ng-bootstrap';

interface ContactFormData {
  name: string;
  email: string;
  subject: string;
  message: string;
}

@Component({
  selector: 'app-contact-form',
  standalone: true,
  imports: [CommonModule, FormsModule, NgbAlertModule],
  templateUrl: './contact-form.html',
  styleUrl: './contact-form.css',
})
export class ContactForm {
  model: ContactFormData = {
    name: '',
    email: '',
    subject: '',
    message: '',
  };

  submitting = false;
  submitted = false;

  onSubmit() {
    if (this.submitting) {
      return;
    }
    this.submitting = true;

    // Simulate submission; in a real app this would call an API.
    setTimeout(() => {
      this.submitting = false;
      this.submitted = true;
    }, 800);
  }
}
