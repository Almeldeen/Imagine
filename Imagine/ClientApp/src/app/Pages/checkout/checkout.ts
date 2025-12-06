import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { CartService, CartDto, CartItemDto } from '../../core/cart.service';
import { OrderService, CreateOrderRequest, OrderCreatedResponse } from '../../core/order.service';
import { ToastService } from '../../core/toast.service';
import { ApiResponse } from '../../core/IApiResponse';

interface CheckoutFormModel {
  fullName: string;
  phoneNumber: string;
  address: string;
  city: string;
  notes?: string;
}

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './checkout.html',
  styleUrls: ['./checkout.css'],
})
export class Checkout implements OnInit {
  private readonly cartService = inject(CartService);
  private readonly orderService = inject(OrderService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  items: CartItemDto[] = [];
  isLoading = false;
  submitting = false;

  model: CheckoutFormModel = {
    fullName: '',
    phoneNumber: '',
    address: '',
    city: '',
    notes: '',
  };

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
    return this.subtotal * 0.15;
  }

  get shipping(): number {
    return this.hasItems ? 9.99 : 0;
  }

  get total(): number {
    return this.subtotal + this.tax + this.shipping;
  }

  onSubmit(form: NgForm): void {
    if (this.submitting) {
      return;
    }

    if (!this.hasItems) {
      this.toast.error('Your cart is empty. Add some products before checking out.');
      return;
    }

    if (form.invalid) {
      Object.values(form.controls).forEach((control) => control.markAsTouched());
      this.toast.error('Please fill in all required fields.');
      return;
    }

    const cartKey = this.cartService.getUserOrSessionIdSnapshot();

    const payload: CreateOrderRequest = {
      fullName: this.model.fullName,
      phoneNumber: this.model.phoneNumber,
      address: this.model.address,
      city: this.model.city,
      notes: this.model.notes,
      cartUserOrSessionId: cartKey,
      grandTotal: this.total,
    };

    this.submitting = true;

    this.orderService.createOrder(payload).subscribe({
      next: (res: ApiResponse<OrderCreatedResponse>) => {
        this.submitting = false;
        if (!res.success || res.data == null) {
          this.toast.error(res.message || 'Failed to place your order.');
          return;
        }

        const created = res.data;

        this.toast.success('Your order has been submitted successfully.');

        this.cartService.clearCart().subscribe();

        this.router.navigate(['/client/orders/confirmation', created.orderId], {
          queryParams: { orderNumber: created.orderNumber },
        });
      },
      error: () => {
        this.submitting = false;
        this.toast.error('Something went wrong while placing your order.');
      },
    });
  }

  private loadCart(): void {
    this.isLoading = true;

    this.cartService.getCart().subscribe({
      next: (res: ApiResponse<CartDto>) => {
        const cart = res.data;
        this.items = (cart?.items as CartItemDto[]) ?? [];
        this.isLoading = false;
      },
      error: () => {
        this.items = [];
        this.isLoading = false;
        this.toast.error('Failed to load your cart for checkout.');
      },
    });
  }
}
