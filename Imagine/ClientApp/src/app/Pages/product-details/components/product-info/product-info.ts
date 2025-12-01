import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import type { ProductDetailsModel } from '../../product-details';

@Component({
  selector: 'app-product-info',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-info.html',
  styleUrl: './product-info.css',
})
export class ProductInfo {
  @Input() product!: ProductDetailsModel;
  @Input() finalPricePerItem = 0;
  @Input() totalPrice = 0;
  @Input() quantity = 1;
  @Input() selectedColorLabel = '';
  @Input() selectedCustomizationName = '';
  @Input() wishlistActive = false;

  @Output() wishlistToggle = new EventEmitter<void>();

  onToggleWishlist() {
    this.wishlistToggle.emit();
  }
}
