import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductHeader } from './Components/product-header/product-header';
import { ProductList } from './Components/product-list/product-list';
import { ProductEmptyState } from './Components/product-empty-state/product-empty-state';
import { ProductService, ProductsListQuery } from './Core/Service/product.service';
import { ApiResponse } from '../../../core/IApiResponse';
import { IProduct } from './Core/Interface/IProduct';

@Component({
  selector: 'app-products',
  imports: [CommonModule, ProductHeader, ProductList, ProductEmptyState],
  templateUrl: './products.html',
  styleUrl: './products.css',
})
export class Products implements OnInit {
  private productService = inject(ProductService);

  products: IProduct[] = [];
  hasProducts = false;
  currentView: 'grid' | 'list' = 'grid';
  currentSort: string = 'name';

  ngOnInit() {
    this.loadProducts();
  }

  private loadProducts() {
    const query: ProductsListQuery = {
      pageNumber: 1,
      pageSize: 20,
      sortBy: this.currentSort === 'price' ? 'Price' : 'Name',
      sortDirection: 'Asc',
    };

    this.productService.getAll(query).subscribe({
      next: (res: ApiResponse<IProduct[]>) => {
        this.products = res.data ?? [];
        this.hasProducts = this.products.length > 0;
      },
      error: (err: any) => {
        console.error('Failed to load products', err);
        this.products = [];
        this.hasProducts = false;
      },
    });
  }

  onFilterChange(filter: string) {
    // In future, map filter to query params (e.g., category, status)
    console.log('Filter changed:', filter);
  }

  onViewChange(view: string) {
    this.currentView = view === 'list' ? 'list' : 'grid';
  }

  onSortChange(sort: string) {
    this.currentSort = sort;
    this.loadProducts();
  }
}
