import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService, UserOrderSummary } from '../../../core/order.service';
import { ToastService } from '../../../core/toast.service';
import { ApiResponse } from '../../../core/IApiResponse';

@Component({
  selector: 'app-client-orders',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './client-orders.html',
  styleUrls: ['./client-orders.css'],
})
export class ClientOrders implements OnInit {
  private readonly orderService = inject(OrderService);
  private readonly toast = inject(ToastService);
  private readonly route = inject(ActivatedRoute);

  orders: UserOrderSummary[] = [];
  loading = false;
  error = '';
  focusedOrderNumber: string | null = null;

  ngOnInit(): void {
    this.focusedOrderNumber = this.route.snapshot.queryParamMap.get('orderNumber');
    this.loadOrders();
  }

  get hasOrders(): boolean {
    return this.orders.length > 0;
  }

  loadOrders(): void {
    this.loading = true;
    this.error = '';

    this.orderService.getMyOrders().subscribe({
      next: (res: ApiResponse<UserOrderSummary[]>) => {
        this.loading = false;
        if (!res.success || !res.data) {
          this.error = res.message || 'Failed to load your orders.';
          return;
        }
        this.orders = res.data;
      },
      error: () => {
        this.loading = false;
        this.error = 'Failed to load your orders.';
        this.toast.error('Could not load your orders. Please try again.');
      },
    });
  }
}
