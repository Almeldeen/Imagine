import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

interface ProductColor {
  name: string;
  value: string;
  extraPrice?: number;
}

interface Product {
  id: number;
  name: string;
  category: string;
  basePrice: number;
  image: string;
  colors: ProductColor[];
  isAiPowered?: boolean;
  isFeatured?: boolean;
}

@Component({
  selector: 'app-products-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './products-card.html',
  styleUrl: './products-card.css',
})
export class ProductsCard {
  @Input() product!: Product;

  activeColor: ProductColor | null = null;

  ngOnInit() {
    this.activeColor = this.product.colors[0] ?? null;
  }

  selectColor(color: ProductColor, event: Event) {
    event.stopPropagation();
    this.activeColor = color;
  }

  get displayPrice(): number {
    const extra = this.activeColor?.extraPrice ?? 0;
    return this.product.basePrice + extra;
  }
}
