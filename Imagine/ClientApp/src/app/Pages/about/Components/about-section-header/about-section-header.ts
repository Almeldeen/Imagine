import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-about-section-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './about-section-header.html',
  styleUrl: './about-section-header.css',
})
export class AboutSectionHeader {
  @Input() badgeLabel = '';
  @Input() title = '';
  @Input() highlight = '';
  @Input() subtitle = '';
  @Input() align: 'left' | 'center' = 'center';
}
