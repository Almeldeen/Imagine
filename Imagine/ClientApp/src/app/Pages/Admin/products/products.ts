import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductHeader } from './Components/product-header/product-header';
import { ProductList } from './Components/product-list/product-list';
import { ProductEmptyState } from './Components/product-empty-state/product-empty-state';

@Component({
  selector: 'app-products',
  imports: [CommonModule, ProductHeader, ProductList, ProductEmptyState],
  templateUrl: './products.html',
  styleUrl: './products.css',
})
export class Products {
  hasProducts = true; // Toggle this to show empty state

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
}
