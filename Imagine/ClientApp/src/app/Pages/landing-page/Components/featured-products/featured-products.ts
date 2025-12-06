import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IProduct } from '../../../Admin/products/Core/Interface/IProduct';
import { ProductCard } from '../../../../shared/Components/product-card/product-card';

@Component({
  selector: 'app-featured-products',
  standalone: true,
  imports: [CommonModule, ProductCard],
  templateUrl: './featured-products.html',
  styleUrl: './featured-products.css',
})
export class FeaturedProducts {
  @Input() products: IProduct[] = [];

  get topProducts(): IProduct[] {
    return (this.products || []).slice(0, 4);
  }
}
