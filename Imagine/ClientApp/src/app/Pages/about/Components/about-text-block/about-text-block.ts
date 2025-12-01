import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-about-text-block',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './about-text-block.html',
  styleUrl: './about-text-block.css',
})
export class AboutTextBlock {
  @Input() icon = '';
  @Input() label = '';
  @Input() title = '';
  @Input() text = '';
}
