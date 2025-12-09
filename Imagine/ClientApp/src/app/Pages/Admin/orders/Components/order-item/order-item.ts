import { Component, TemplateRef, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModalModule } from '@ng-bootstrap/ng-bootstrap';
import { AdminOrder, OrderService } from '../../../../../core/order.service';
import { ToastService } from '../../../../../core/toast.service';

@Component({
  selector: 'app-order-item',
  imports: [CommonModule, NgbModalModule, FormsModule],
  templateUrl: './order-item.html',
  styleUrls: ['./order-item.css'],
})
export class OrderItem {
  @Input() order!: AdminOrder;
  private modalService = inject(NgbModal);
  private orderService = inject(OrderService);
  private toast = inject(ToastService);

  orderDetails: AdminOrder | null = null;
  detailsLoading = false;

  // Update status modal state
  selectedStatus: string = 'Pending';
  trackingNumberInput = '';
  statusNotes = '';

  get displayedOrder(): AdminOrder {
    return this.orderDetails ?? this.order;
  }

  get statusLower(): string {
    return (this.displayedOrder.status || '').toLowerCase();
  }

  get paymentStatusLower(): string {
    return (this.displayedOrder.paymentStatus || '').toLowerCase();
  }

  openOrderDetails(content: TemplateRef<any>) {
    this.detailsLoading = true;
    this.orderDetails = null;

    const modalRef = this.modalService.open(content, {
      size: 'xl',
      centered: true,
      scrollable: true,
    });

    this.orderService.getOrderById(this.order.orderId).subscribe({
      next: (res) => {
        this.detailsLoading = false;

        if (!res.success || !res.data) {
          this.toast.error(res.message || 'Failed to load order details.');
          modalRef.dismiss();
          return;
        }

        this.orderDetails = res.data;
      },
      error: () => {
        this.detailsLoading = false;
        this.toast.error('Failed to load order details.');
        modalRef.dismiss();
      },
    });
  }

  openStatusModal(content: TemplateRef<any>) {
    this.selectedStatus = this.order.status || 'Pending';
    this.trackingNumberInput = this.order.trackingNumber || '';
    this.statusNotes = '';

    this.modalService.open(content, {
      size: 'lg',
      centered: true,
    });
  }

  setStatus(status: string) {
    this.selectedStatus = status;
  }

  saveStatus(modalRef: any) {
    const status = this.selectedStatus || this.order.status;

    this.orderService
      .updateOrderStatus(this.order.orderId, {
        status,
        trackingNumber: this.trackingNumberInput || undefined,
      })
      .subscribe({
        next: (res) => {
          if (!res.success || !res.data) {
            this.toast.error(res.message || 'Failed to update status.');
            return;
          }

          // Update local order with latest data
          this.order.status = res.data.status;
          this.order.trackingNumber = res.data.trackingNumber;
          this.order.totalAmount = res.data.totalAmount;
          this.orderDetails = res.data;

          this.toast.success('Order status updated.');
          modalRef.close();
        },
        error: () => {
          this.toast.error('Failed to update status.');
        },
      });
  }

  openTrackingModal(content: TemplateRef<any>) {
    this.modalService.open(content, {
      size: 'lg',
      centered: true
    });
  }

  get trackingLabel(): string {
    const s = (this.order.status || '').toLowerCase();
    switch (s) {
      case 'pending':
        return 'Pending';
      case 'processing':
        return 'Processing';
      case 'shipped':
        return 'In Transit';
      case 'delivered':
        return 'Delivered';
      case 'cancelled':
        return 'Cancelled';
      case 'refunded':
        return 'Refunded';
      default:
        return this.order.status;
    }
  }

  get trackingPercent(): number {
    const s = (this.order.status || '').toLowerCase();
    switch (s) {
      case 'pending':
        return 10;
      case 'processing':
        return 30;
      case 'shipped':
        return 60;
      case 'delivered':
      case 'refunded':
        return 100;
      case 'cancelled':
        return 0;
      default:
        return 0;
    }
  }

  openCustomPreviewModal(content: TemplateRef<any>) {
    this.modalService.open(content, {
      size: 'lg',
      centered: true,
      scrollable: true
    });
  }
}
