import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-add-to-cart-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './add-to-cart-button.html',
  styleUrl: './add-to-cart-button.css',
})
export class AddToCartButton {
  @Input() inStock = true;
  @Input() quantity = 1;
  @Input() finalPricePerItem = 0;
  @Input() totalPrice = 0;
  @Input() canAdd = true;
  @Input() hint: string | null = null;

  @Output() quantityChange = new EventEmitter<number>();
  @Output() addToCart = new EventEmitter<void>();

  onIncrement() {
    this.updateQuantity(this.quantity + 1);
  }

  onDecrement() {
    this.updateQuantity(this.quantity - 1);
  }

  onQuantityInput(value: string) {
    const parsed = parseInt(value, 10);
    if (!isNaN(parsed)) {
      this.updateQuantity(parsed);
    }
  }

  private updateQuantity(qty: number) {
    const safe = Math.max(1, qty);
    this.quantityChange.emit(safe);
  }

  onAddToCartClick() {
    if (!this.inStock || !this.canAdd) {
      return;
    }
    this.addToCart.emit();
  }
}
