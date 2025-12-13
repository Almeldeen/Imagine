import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges, SimpleChanges, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { IProduct, IProductColor, IProductImage } from '../../../Pages/Admin/products/Core/Interface/IProduct';
import { CartService } from '../../../core/cart.service';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './product-card.html',
  styleUrl: './product-card.css',
})
export class ProductCard implements OnChanges {
  @Input() product!: IProduct;

  activeColor: IProductColor | null = null;
  activeImage: IProductImage | null = null;

  private cartService = inject(CartService);
  isAdding = false;
  addSuccess = false;
  addError = false;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['product'] && this.product) {
      this.initializeSelection();
    }
  }

  get colors(): IProductColor[] {
    return this.product?.colors ?? [];
  }

  get displayPrice(): string {
    const extra = this.activeColor?.additionalPrice ?? 0;
    return (this.product.price + extra).toFixed(2);
  }

  selectColor(color: IProductColor, event?: Event): void {
    event?.stopPropagation();
    this.activeColor = color;
    this.updateActiveImage();
  }

   addToCart(event?: Event): void {
    event?.stopPropagation();

    if (!this.activeColor || this.isAdding) {
      return;
    }

    this.isAdding = true;
    this.addSuccess = false;
    this.addError = false;

    this.cartService.addToCart(this.activeColor.id, 1).subscribe({
      next: () => {
        this.isAdding = false;
        this.addSuccess = true;

        setTimeout(() => {
          this.addSuccess = false;
        }, 2000);
      },
      error: (err: any) => {
        console.error('Failed to add product to cart', err);
        this.isAdding = false;
        this.addError = true;

        setTimeout(() => {
          this.addError = false;
        }, 3000);
      },
    });
  }

  private initializeSelection(): void {
    const availableColors = this.colors;
    this.activeColor = availableColors.length ? availableColors[0] : null;
    this.updateActiveImage();
  }

  private updateActiveImage(): void {
    const images = this.activeColor?.images ?? [];

    if (!images.length) {
      this.activeImage = null;
      return;
    }

    this.activeImage = images.find((i) => i.isMain) ?? images[0];
  }
}
