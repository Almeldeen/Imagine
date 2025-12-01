import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderItem } from '../order-item/order-item';

@Component({
  selector: 'app-order-list',
  imports: [CommonModule, OrderItem],
  templateUrl: './order-list.html',
  styleUrls: ['./order-list.css'],
})
export class OrderList {

}
