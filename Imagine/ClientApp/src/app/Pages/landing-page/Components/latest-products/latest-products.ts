import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IProduct } from '../../../Admin/products/Core/Interface/IProduct';
import { ProductCard } from '../../../../shared/Components/product-card/product-card';

@Component({
  selector: 'app-latest-products',
  standalone: true,
  imports: [CommonModule, ProductCard],
  templateUrl: './latest-products.html',
  styleUrl: './latest-products.css',
})
export class LatestProducts {
  @Input() products: IProduct[] = [];

  get recentProducts(): IProduct[] {
    return (this.products || []).slice(0, 4);
  }
}
