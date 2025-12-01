import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';
import { CartItemComponent } from './components/cart-item/cart-item';
import { CartSummary } from './components/cart-summary/cart-summary';
import { CartEmptyState } from './components/cart-empty/cart-empty';

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
export class Cart {
  items: CartItemModel[] = [
    {
      id: 1,
      name: 'Classic AI Hoodie',
      image: '/assets/images/Hoodie.png',
      color: 'Midnight Black',
      size: 'L',
      basePrice: 49.99,
      quantity: 1,
      isAiPowered: true,
    },
    {
      id: 2,
      name: 'Premium AI T-Shirt',
      image: '/assets/images/T-Shirt.png',
      color: 'Neon Purple',
      size: 'M',
      basePrice: 34.99,
      quantity: 2,
      isAiPowered: true,
    },
  ];

  taxRate = 0.15;
  shippingFlat = 9.99;

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
    this.items = this.items.map((item) =>
      item.id === update.id
        ? { ...item, quantity: Math.max(1, update.quantity) }
        : item
    );
  }

  onRemoveItem(id: number) {
    this.items = this.items.filter((item) => item.id !== id);
  }

  onCheckout() {
    // Placeholder for future integration with checkout flow
  }
}
