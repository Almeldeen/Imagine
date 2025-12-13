import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IProduct } from '../../Core/Interface/IProduct';
import { environment } from '../../../../../../environments/environment';

@Component({
  selector: 'app-product-item',
  imports: [CommonModule],
  templateUrl: './product-item.html',
  styleUrl: './product-item.css',
})
export class ProductItem {
  @Input() product!: IProduct;
  @Output() onEdit = new EventEmitter<number>();
  @Output() onDelete = new EventEmitter<number>();
  @Output() onView = new EventEmitter<number>();

  baseUrl: string = environment.apiUrl;

  get imageSrc(): string {
    const url = this.product?.imageUrl;

    if (!url) {
      return '/assets/images/hero-banner.png';
    }

    if (url.startsWith('http')) {
      return url;
    }

    if (url.startsWith('/')) {
      return this.baseUrl + url;
    }

    return this.baseUrl + '/' + url;
  }

  getTotalStock(): string {
    if (!this.product?.colors || this.product.colors.length === 0) {
      return '--';
    }
    const totalStock = this.product.colors.reduce((sum, color) => sum + (color.stock || 0), 0);
    return totalStock.toString();
  }
}
