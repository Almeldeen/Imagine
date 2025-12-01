import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import type { CartItemModel } from '../../cart';
import { NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-cart-item',
  standalone: true,
  imports: [CommonModule, NgbTooltipModule],
  templateUrl: './cart-item.html',
  styleUrl: './cart-item.css',
})
export class CartItemComponent {
  @Input() item!: CartItemModel;

  @Output() quantityChange = new EventEmitter<{ id: number; quantity: number }>();
  @Output() remove = new EventEmitter<number>();

  get totalPrice(): number {
    return this.item.basePrice * this.item.quantity;
  }

  onIncrement() {
    this.updateQuantity(this.item.quantity + 1);
  }

  onDecrement() {
    this.updateQuantity(this.item.quantity - 1);
  }

  onQuantityInput(value: string) {
    const parsed = parseInt(value, 10);
    if (!isNaN(parsed)) {
      this.updateQuantity(parsed);
    }
  }

  private updateQuantity(quantity: number) {
    const safeQuantity = Math.max(1, quantity);
    this.quantityChange.emit({ id: this.item.id, quantity: safeQuantity });
  }

  onRemove() {
    this.remove.emit(this.item.id);
  }
}
