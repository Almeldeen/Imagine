import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';
import { CartItemComponent } from './components/cart-item/cart-item';
import { CartSummary } from './components/cart-summary/cart-summary';
import { CartEmptyState } from './components/cart-empty/cart-empty';
import { CartService, CartDto, CartItemDto } from '../../core/cart.service';
import { Router } from '@angular/router';
import { ApiResponse } from '../../core/IApiResponse';

export interface CartItemModel {
  id: number;
  name: string;
  image: string;
  color: string;
  size?: string;
  basePrice: number;
  quantity: number;
  isAiPowered?: boolean;
}

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, NgbTooltipModule, CartItemComponent, CartSummary, CartEmptyState],
  templateUrl: './cart.html',
  styleUrl: './cart.css',
})
export class Cart implements OnInit {
  private cartService = inject(CartService);
  private router = inject(Router);

  items: CartItemModel[] = [];

  taxRate = 0.15;
  shippingFlat = 30.00;

  isLoading = false;
  hasError = false;
  errorMessage = '';

  ngOnInit(): void {
    this.loadCart();
  }

  get hasItems(): boolean {
    return this.items.length > 0;
  }

  get itemCount(): number {
    return this.items.reduce((sum, item) => sum + item.quantity, 0);
  }

  get subtotal(): number {
    return this.items.reduce((sum, item) => sum + item.basePrice * item.quantity, 0);
  }

  get tax(): number {
    return this.subtotal * this.taxRate;
  }

  get shipping(): number {
    return this.hasItems ? this.shippingFlat : 0;
  }

  get total(): number {
    return this.subtotal + this.tax + this.shipping;
  }

  onQuantityChange(update: { id: number; quantity: number }) {
    this.cartService.updateItemQuantity(update.id, Math.max(1, update.quantity)).subscribe({
      next: () => {
        this.items = this.items.map((item) =>
          item.id === update.id
            ? { ...item, quantity: Math.max(1, update.quantity) }
            : item
        );
      },
      error: (err: any) => {
        console.error('Failed to update cart item quantity', err);
      },
    });
  }

  onRemoveItem(id: number) {
    this.cartService.removeItem(id).subscribe({
      next: () => {
        this.items = this.items.filter((item) => item.id !== id);
      },
      error: (err: any) => {
        console.error('Failed to remove cart item', err);
      },
    });
  }

  onCheckout() {
    this.router.navigate(['/Checkout']);
  }

  private loadCart(): void {
    this.isLoading = true;
    this.hasError = false;

    this.cartService.getCart().subscribe({
      next: (res: ApiResponse<CartDto>) => {
        const cart = res.data;
        this.items = (cart?.items as CartItemModel[]) ?? [];
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Failed to load cart', err);
        this.items = [];
        this.isLoading = false;
        this.hasError = true;
        this.errorMessage = 'Failed to load your cart. Please try again.';
      },
    });
  }
}
