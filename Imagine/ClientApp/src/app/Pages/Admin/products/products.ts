import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { ProductHeader } from './Components/product-header/product-header';
import { ProductList } from './Components/product-list/product-list';
import { ProductEmptyState } from './Components/product-empty-state/product-empty-state';
import { ProductsPagination } from '../../all-products/Components/products-pagination/products-pagination';
import { ProductService, ProductsListQuery } from './Core/Service/product.service';
import { ApiResponse } from '../../../core/IApiResponse';
import { IProduct } from './Core/Interface/IProduct';

@Component({
  selector: 'app-products',
  imports: [CommonModule, ProductHeader, ProductList, ProductEmptyState, ProductsPagination],
  templateUrl: './products.html',
  styleUrl: './products.css',
})
export class Products implements OnInit {
  private productService = inject(ProductService);
  private router = inject(Router);

  products: IProduct[] = [];
  hasProducts = false;
  currentView: 'grid' | 'list' = 'grid';
  currentSort: string = 'name';

  // Filters
  searchTerm: string = '';
  currentFilter: 'all' | 'active' | 'draft' | 'archived' = 'all';

  // Basic pagination state (single page in UI, but we respect backend metadata)
  currentPage = 1;
  pageSize = 20;
  totalItems = 0;

  // Global counts for header badges
  allCount = 0;
  activeCount = 0;
  draftCount = 0;
  archivedCount = 0;

  ngOnInit() {
    this.loadCounts();
    this.loadProducts();
  }

  private loadProducts() {
    const { sortBy, sortDirection } = this.mapSortKey(this.currentSort);

    const query: ProductsListQuery = {
      searchTerm: this.searchTerm || undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy,
      sortDirection,
      isActive: this.mapFilterToIsActive(this.currentFilter),
    };

    this.productService.getAll(query).subscribe({
      next: (res: ApiResponse<IProduct[]>) => {
        this.products = res.data ?? [];
        this.hasProducts = this.products.length > 0;

        // Update pagination metadata if provided
        this.currentPage = res.currentPage ?? this.currentPage;
        this.pageSize = res.pageSize ?? this.pageSize;
        this.totalItems = res.totalItems ?? this.products.length;
      },
      error: (err: any) => {
        console.error('Failed to load products', err);
        this.products = [];
        this.hasProducts = false;
      },
    });
  }

  private loadCounts() {
    const basePage: ProductsListQuery = {
      pageNumber: 1,
      pageSize: 1,
      searchTerm: this.searchTerm || undefined,
    };

    // All products count
    this.productService.getAll(basePage).subscribe({
      next: (res: ApiResponse<IProduct[]>) => {
        this.allCount = res.totalItems ?? (res.data?.length ?? 0);
      },
      error: () => {
        this.allCount = 0;
      },
    });

    // Active products count
    this.productService.getAll({ ...basePage, isActive: true }).subscribe({
      next: (res: ApiResponse<IProduct[]>) => {
        this.activeCount = res.totalItems ?? (res.data?.length ?? 0);
      },
      error: () => {
        this.activeCount = 0;
      },
    });

    // Inactive products count (used for both Draft and Archived badges for now)
    this.productService.getAll({ ...basePage, isActive: false }).subscribe({
      next: (res: ApiResponse<IProduct[]>) => {
        const inactive = res.totalItems ?? (res.data?.length ?? 0);
        this.draftCount = inactive;
        this.archivedCount = inactive;
      },
      error: () => {
        this.draftCount = 0;
        this.archivedCount = 0;
      },
    });
  }

  private mapSortKey(sortKey: string): { sortBy?: string; sortDirection?: 'Asc' | 'Desc' } {
    switch (sortKey) {
      case 'price':
        return { sortBy: 'Price', sortDirection: 'Asc' };
      case 'created':
        return { sortBy: 'CreatedAt', sortDirection: 'Desc' };
      case 'sales':
        return { sortBy: 'ViewCount', sortDirection: 'Desc' };
      default:
        // includes 'name', 'stock', or anything unknown
        return { sortBy: 'Name', sortDirection: 'Asc' };
    }
  }

  private mapFilterToIsActive(filter: 'all' | 'active' | 'draft' | 'archived'): boolean | undefined {
    if (filter === 'active') return true;
    if (filter === 'draft' || filter === 'archived') return false;
    return undefined; // 'all' or unknown -> no status filter
  }

  onRefreshRequested(): void {
    // When a product is deleted from the list, reload counts and products
    this.loadCounts();
    this.loadProducts();
  }

  onSearchChange(term: string) {
    this.searchTerm = term;
    this.currentPage = 1;
    this.loadCounts();
    this.loadProducts();
  }

  onFilterChange(filter: string) {
    this.currentFilter = (filter as any) || 'all';
    this.currentPage = 1;
    this.loadProducts();
  }

  onViewChange(view: string) {
    this.currentView = view === 'list' ? 'list' : 'grid';
  }

  onSortChange(sort: string) {
    this.currentSort = sort;
    this.currentPage = 1;
    this.loadProducts();
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadProducts();
  }

  onEdit(id: number) {
    this.router.navigate(['/admin/products/edit', id]);
  }

  onView(id: number) {
    // Navigate to product details with admin context
    this.router.navigate(['/Product', id], { queryParams: { from: 'admin' } });
  }

  onDelete(id: number) {
    Swal.fire({
      title: 'Are you sure?',
      text: "You won't be able to revert this!",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
      if (result.isConfirmed) {
        this.productService.delete(id).subscribe({
          next: (res) => {
            if (res.success) {
              Swal.fire(
                'Deleted!',
                'Product has been deleted.',
                'success'
              );
              this.loadProducts();
              this.loadCounts();
            } else {
              Swal.fire(
                'Error!',
                'Failed to delete product.',
                'error'
              );
            }
          },
          error: (err) => {
             Swal.fire(
                'Error!',
                'Failed to delete product.',
                'error'
              );
          }
        });
      }
    });
  }
}
