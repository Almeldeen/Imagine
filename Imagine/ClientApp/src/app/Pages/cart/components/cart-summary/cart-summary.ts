import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-cart-summary',
  standalone: true,
  imports: [CommonModule, NgbTooltipModule],
  templateUrl: './cart-summary.html',
  styleUrl: './cart-summary.css',
})
export class CartSummary {
  @Input() itemCount = 0;
  @Input() subtotal = 0;
  @Input() tax = 0;
  @Input() shipping = 0;
  @Input() total = 0;

  @Output() checkout = new EventEmitter<void>();

  onCheckout() {
    this.checkout.emit();
  }
}
