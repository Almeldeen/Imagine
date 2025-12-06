import { Component, TemplateRef, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbModal, NgbModalModule } from '@ng-bootstrap/ng-bootstrap';
import { AdminOrder } from '../../../../../core/order.service';

@Component({
  selector: 'app-order-item',
  imports: [CommonModule, NgbModalModule],
  templateUrl: './order-item.html',
  styleUrls: ['./order-item.css'],
})
export class OrderItem {
  @Input() order!: AdminOrder;
  private modalService = inject(NgbModal);

  openOrderDetails(content: TemplateRef<any>) {
    this.modalService.open(content, {
      size: 'xl',
      centered: true,
      scrollable: true
    });
  }

  openStatusModal(content: TemplateRef<any>) {
    this.modalService.open(content, {
      size: 'lg',
      centered: true
    });
  }

  openTrackingModal(content: TemplateRef<any>) {
    this.modalService.open(content, {
      size: 'lg',
      centered: true
    });
  }

  openCustomPreviewModal(content: TemplateRef<any>) {
    this.modalService.open(content, {
      size: 'lg',
      centered: true,
      scrollable: true
    });
  }
}
