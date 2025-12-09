import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderItem } from '../order-item/order-item';
import { AdminOrder } from '../../../../../core/order.service';

@Component({
  selector: 'app-order-list',
  imports: [CommonModule, OrderItem],
  templateUrl: './order-list.html',
  styleUrls: ['./order-list.css'],
})
export class OrderList {
  @Input() orders: AdminOrder[] = [];
  @Input() viewMode: 'grid' | 'compact' | 'list' = 'grid';
}
