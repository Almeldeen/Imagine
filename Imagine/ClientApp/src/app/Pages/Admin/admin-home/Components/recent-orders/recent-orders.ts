import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiResponse } from '../../../../../core/IApiResponse';
import { AdminOrder, AdminOrdersQuery, OrderService } from '../../../../../core/order.service';

@Component({
  selector: 'app-recent-orders',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './recent-orders.html',
  styleUrl: './recent-orders.css',
})
export class RecentOrders implements OnInit {
  private readonly orderService = inject(OrderService);

  orders: AdminOrder[] = [];

  ngOnInit(): void {
    this.loadRecentOrders();
  }

  private loadRecentOrders(): void {
    const query: AdminOrdersQuery = {
      pageNumber: 1,
      pageSize: 5,
      sortKey: 'date-desc',
      status: 'all',
    };

    this.orderService.getAllOrdersForAdmin(query).subscribe({
      next: (res: ApiResponse<AdminOrder[]>) => {
        if (res.success && res.data) {
          this.orders = res.data;
        } else {
          this.orders = [];
        }
      },
      error: () => {
        this.orders = [];
      },
    });
  }
}
