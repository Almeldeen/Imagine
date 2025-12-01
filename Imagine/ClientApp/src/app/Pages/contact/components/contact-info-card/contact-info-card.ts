import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-contact-info-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './contact-info-card.html',
  styleUrl: './contact-info-card.css',
})
export class ContactInfoCard {
  @Input() icon = '';
  @Input() label = '';
  @Input() value = '';
  @Input() description = '';
  @Input() link?: string;
}
