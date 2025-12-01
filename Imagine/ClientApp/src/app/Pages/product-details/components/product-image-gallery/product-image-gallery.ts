import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-product-image-gallery',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-image-gallery.html',
  styleUrl: './product-image-gallery.css',
})
export class ProductImageGallery {
  @Input() imageUrl = '';
  @Input() productName = '';
  @Input() colorLabel = '';
}
