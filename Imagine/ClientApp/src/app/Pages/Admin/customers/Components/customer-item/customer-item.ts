import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ICustomer } from '../../Core/Interface/ICustomer';

@Component({
  selector: 'app-customer-item',
  imports: [CommonModule],
  templateUrl: './customer-item.html',
  styleUrls: ['./customer-item.css'],
})
export class CustomerItem {
  @Input() customer!: ICustomer;
  @Output() view = new EventEmitter<ICustomer>();
  @Output() edit = new EventEmitter<ICustomer>();
  @Output() resetPassword = new EventEmitter<ICustomer>();
}
