import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomerItem } from '../customer-item/customer-item';

@Component({
  selector: 'app-customer-list',
  imports: [CommonModule, CustomerItem],
  templateUrl: './customer-list.html',
  styleUrls: ['./customer-list.css'],
})
export class CustomerList {

}
