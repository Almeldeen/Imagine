import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ContactForm } from './components/contact-form/contact-form';
import { ContactInfoCard } from './components/contact-info-card/contact-info-card';
import { ContactMap } from './components/contact-map/contact-map';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, ContactForm, ContactInfoCard, ContactMap],
  templateUrl: './contact.html',
  styleUrl: './contact.css',
})
export class Contact {
  contactInfoItems = [
    {
      icon: 'fas fa-envelope',
      label: 'Email',
      value: 'support@imagine.ai',
      description: 'Reach out for questions about orders, custom products, or partnerships.',
    },
    {
      icon: 'fas fa-phone-alt',
      label: 'Phone',
      value: '+1 (555) 123-4567',
      description: 'Mon–Fri, 9:00–18:00 (GMT+2).',
    },
    {
      icon: 'fab fa-discord',
      label: 'Community',
      value: 'Join our Discord',
      description: 'Share feedback, request features, and talk with other creators.',
      link: '#',
    },
  ];
}
