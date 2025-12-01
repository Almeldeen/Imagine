import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomerHeader } from './Components/customer-header/customer-header';
import { CustomerList } from './Components/customer-list/customer-list';
import { CustomerEmptyState } from './Components/customer-empty-state/customer-empty-state';

@Component({
  selector: 'app-customers',
  imports: [CommonModule, CustomerHeader, CustomerList, CustomerEmptyState],
  templateUrl: './customers.html',
  styleUrl: './customers.css',
})
export class Customers {
  hasCustomers = true; // Toggle this to show empty state

  onFilterChange(filter: string) {
    console.log('Filter changed:', filter);
    // Handle filter change
  }

  onViewChange(view: string) {
    console.log('View changed:', view);
    // Handle view change
  }

  onSortChange(sort: string) {
    console.log('Sort changed:', sort);
    // Handle sort change
  }

  onSearchChange(search: string) {
    console.log('Search changed:', search);
    // Handle search change
  }
}
