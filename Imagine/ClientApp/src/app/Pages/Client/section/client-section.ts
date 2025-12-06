import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-client-section',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './client-section.html',
  styleUrl: './client-section.css',
})
export class ClientSection {
  title = '';
  description = '';

  constructor(route: ActivatedRoute) {
    route.data.subscribe(data => {
      this.title = data['title'] ?? '';
      this.description = data['description'] ?? '';
    });
  }
}
