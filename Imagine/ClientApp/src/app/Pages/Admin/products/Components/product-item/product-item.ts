import { Component, Input } from '@angular/core';
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

  baseUrl: string = environment.apiUrl;

  get imageSrc(): string {
    const url = this.product?.imageUrl;

    if (!url) {
      return '/assets/images/product-placeholder.jpg';
    }

    if (url.startsWith('http')) {
      return url;
    }

    if (url.startsWith('/')) {
      return this.baseUrl + url;
    }

    return this.baseUrl + '/' + url;
  }
}
