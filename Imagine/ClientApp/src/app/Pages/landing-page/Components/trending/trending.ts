import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { IProduct } from '../../../Admin/products/Core/Interface/IProduct';
import { ProductCard } from '../../../../shared/Components/product-card/product-card';

@Component({
  selector: 'app-trending',
  standalone: true,
  imports: [CommonModule, ProductCard, RouterLink],
  templateUrl: './trending.html',
  styleUrl: './trending.css',
})
export class Trending {
  @Input() popularProducts: IProduct[] = [];

  get topPopular(): IProduct[] {
    return (this.popularProducts || []).slice(0, 3);
  }

  // Initialize color switcher for product cards
  private initializeColorSwitcher(): void {
    // kept for compatibility; logic now handled inside shared product card
  }
}
