import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { OrderHeader } from './Components/order-header/order-header';
import { OrderList } from './Components/order-list/order-list';
import { OrderEmptyState } from './Components/order-empty-state/order-empty-state';
import { AdminOrder, AdminOrdersQuery, OrderService } from '../../../core/order.service';
import { ProductsPagination } from '../../all-products/Components/products-pagination/products-pagination';
import { ApiResponse } from '../../../core/IApiResponse';
import { ToastService } from '../../../core/toast.service';

@Component({
  selector: 'app-orders',
  imports: [CommonModule, OrderHeader, OrderList, OrderEmptyState, ProductsPagination],
  templateUrl: './orders.html',
  styleUrl: './orders.css',
})
export class Orders implements OnInit {
  private readonly orderService = inject(OrderService);
  private readonly toast = inject(ToastService);
  private readonly route = inject(ActivatedRoute);

  orders: AdminOrder[] = [];
  filteredOrders: AdminOrder[] = [];
  loading = false;
  error = '';

  // status filter and view mode
  selectedStatus: 'all' | 'pending' | 'processing' | 'shipped' | 'delivered' | 'cancelled' | 'refunded' = 'all';
  viewMode: 'grid' | 'compact' | 'list' = 'grid';
  currentSort: 'date-desc' | 'date-asc' | 'amount-desc' | 'amount-asc' | 'status' | 'customer' = 'date-desc';
  currentSearch = '';
  dateRange: { from?: string; to?: string } = {};

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalItems = 0;

  // Counts for header badges and stats
  allCount = 0;
  pendingCount = 0;
  processingCount = 0;
  shippedCount = 0;
  deliveredCount = 0;
  cancelledCount = 0;
  refundedCount = 0;

  ngOnInit(): void {
    const defaultStatus = this.route.snapshot.data['defaultStatus'] as
      | 'all'
      | 'pending'
      | 'processing'
      | 'shipped'
      | 'delivered'
      | 'cancelled'
      | 'refunded'
      | undefined;

    if (defaultStatus) {
      this.selectedStatus = defaultStatus;
    }

    this.loadCounts();
    this.loadOrders();
  }

  get hasOrders(): boolean {
    return this.filteredOrders.length > 0;
  }

  private loadOrders(): void {
    this.loading = true;
    this.error = '';

    const query: AdminOrdersQuery = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortKey: this.currentSort,
      status: this.selectedStatus,
      searchTerm: this.currentSearch || undefined,
    };

    this.orderService.getAllOrdersForAdmin(query).subscribe({
      next: (res: ApiResponse<AdminOrder[]>) => {
        this.loading = false;
        if (!res.success || !res.data) {
          this.orders = [];
          this.filteredOrders = [];
          this.error = res.message || 'Failed to load orders.';
          return;
        }

        this.orders = res.data;
        this.filteredOrders = this.orders;

        // Update pagination metadata if provided
        this.currentPage = res.currentPage ?? this.currentPage;
        this.pageSize = res.pageSize ?? this.pageSize;
        this.totalItems = res.totalItems ?? this.orders.length;
      },
      error: () => {
        this.loading = false;
        this.orders = [];
        this.filteredOrders = [];
        this.error = 'Failed to load orders.';
        this.toast.error('Unable to load orders. Please try again.');
      },
    });
  }

  private loadCounts(): void {
    const base: AdminOrdersQuery = {
      pageNumber: 1,
      pageSize: 1,
      searchTerm: this.currentSearch || undefined,
    };

    // All
    this.orderService.getAllOrdersForAdmin({ ...base, status: 'all' }).subscribe({
      next: (res: ApiResponse<AdminOrder[]>) => {
        this.allCount = res.totalItems ?? (res.data?.length ?? 0);
      },
      error: () => (this.allCount = 0),
    });

    // Pending
    this.orderService.getAllOrdersForAdmin({ ...base, status: 'pending' }).subscribe({
      next: (res: ApiResponse<AdminOrder[]>) => {
        this.pendingCount = res.totalItems ?? (res.data?.length ?? 0);
      },
      error: () => (this.pendingCount = 0),
    });

    // Processing
    this.orderService.getAllOrdersForAdmin({ ...base, status: 'processing' }).subscribe({
      next: (res: ApiResponse<AdminOrder[]>) => {
        this.processingCount = res.totalItems ?? (res.data?.length ?? 0);
      },
      error: () => (this.processingCount = 0),
    });

    // Shipped
    this.orderService.getAllOrdersForAdmin({ ...base, status: 'shipped' }).subscribe({
      next: (res: ApiResponse<AdminOrder[]>) => {
        this.shippedCount = res.totalItems ?? (res.data?.length ?? 0);
      },
      error: () => (this.shippedCount = 0),
    });

    // Delivered
    this.orderService.getAllOrdersForAdmin({ ...base, status: 'delivered' }).subscribe({
      next: (res: ApiResponse<AdminOrder[]>) => {
        this.deliveredCount = res.totalItems ?? (res.data?.length ?? 0);
      },
      error: () => (this.deliveredCount = 0),
    });

    // Cancelled
    this.orderService.getAllOrdersForAdmin({ ...base, status: 'cancelled' }).subscribe({
      next: (res: ApiResponse<AdminOrder[]>) => {
        this.cancelledCount = res.totalItems ?? (res.data?.length ?? 0);
      },
      error: () => (this.cancelledCount = 0),
    });

    // Refunded
    this.orderService.getAllOrdersForAdmin({ ...base, status: 'refunded' }).subscribe({
      next: (res: ApiResponse<AdminOrder[]>) => {
        this.refundedCount = res.totalItems ?? (res.data?.length ?? 0);
      },
      error: () => (this.refundedCount = 0),
    });
  }

  private applyFilters(): void {
    // Backend handles filtering & sorting; keep filteredOrders in sync
    this.filteredOrders = this.orders;
  }

  onFilterChange(filter: string) {
    if (this.selectedStatus === filter) {
      return;
    }

    this.selectedStatus = (filter as any) ?? 'all';
    this.currentPage = 1;
    this.loadOrders();
  }

  onViewChange(view: string) {
    if (this.viewMode === view) {
      return;
    }
    this.viewMode = (view as any) ?? 'grid';
  }

  onSortChange(sort: string) {
    if (this.currentSort === sort) {
      return;
    }
    this.currentSort = (sort as any) ?? 'date-desc';
    this.currentPage = 1;
    this.loadOrders();
  }

  onSearchChange(search: string) {
    this.currentSearch = search;
    this.currentPage = 1;
    this.loadCounts();
    this.loadOrders();
  }

  onDateRangeChange(dateRange: any) {
    this.dateRange = dateRange;
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadOrders();
  }
}
