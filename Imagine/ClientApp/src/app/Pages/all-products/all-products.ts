import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductsFilter } from './Components/products-filter/products-filter';
import { ProductsPagination } from './Components/products-pagination/products-pagination';
import { ProductCard } from '../../shared/Components/product-card/product-card';
import { ProductService, ProductsListQuery } from '../Admin/products/Core/Service/product.service';
import { IProduct } from '../Admin/products/Core/Interface/IProduct';
import { CategoryService } from '../Admin/category/Core/Service/category.service';
import { ICategory } from '../Admin/category/Core/Interface/ICategory';
import { ApiResponse } from '../../core/IApiResponse';

@Component({
  selector: 'app-all-products',
  standalone: true,
  imports: [CommonModule, FormsModule, ProductsFilter, ProductsPagination, ProductCard],
  templateUrl: './all-products.html',
  styleUrl: './all-products.css',
})
export class AllProducts implements OnInit {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);

  products: IProduct[] = [];
  categories: ICategory[] = [];

  // Filters
  searchTerm = '';
  selectedCategoryId: number | null = null;
  selectedColorHex: string | null = null;
  minPrice?: number;
  maxPrice?: number;

  // Sorting
  sortKey: 'relevance' | 'latest' | 'popular' | 'priceAsc' | 'priceDesc' = 'relevance';

  // Pagination
  currentPage = 1;
  pageSize = 9;
  totalItems = 0;

  // UI state
  isLoading = false;
  hasError = false;
  errorMessage = '';

  // Simple color palette for filtering
  readonly colorOptions: { name: string; hex: string }[] = [
    { name: 'Black', hex: '#000000' },
    { name: 'White', hex: '#ffffff' },
    { name: 'Blue', hex: '#3b82f6' },
    { name: 'Purple', hex: '#8b5cf6' },
    { name: 'Pink', hex: '#ec4899' },
  ];

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();
  }

  onSearchChange(term: string): void {
    this.searchTerm = term;
    this.currentPage = 1;
    this.loadProducts();
  }

  onSortChange(sortKey: string): void {
    this.sortKey = (sortKey as any) || 'relevance';
    this.currentPage = 1;
    this.loadProducts();
  }

  onCategoryChange(categoryId: number | null): void {
    this.selectedCategoryId = categoryId;
    this.currentPage = 1;
    this.loadProducts();
  }

  onColorSelect(hex: string | null): void {
    this.selectedColorHex = hex;
    this.currentPage = 1;
    this.loadProducts();
  }

  onPriceRangeApply(): void {
    this.currentPage = 1;
    this.loadProducts();
  }

  onResetFilters(): void {
    this.selectedCategoryId = null;
    this.selectedColorHex = null;
    this.minPrice = undefined;
    this.maxPrice = undefined;
    this.currentPage = 1;
    this.loadProducts();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadProducts();
  }

  get hasProducts(): boolean {
    return !this.isLoading && !this.hasError && this.products.length > 0;
  }

  private loadCategories(): void {
    this.categoryService.getAll().subscribe({
      next: (res: ApiResponse<ICategory[]>) => {
        this.categories = res.data ?? [];
      },
      error: (err: any) => {
        console.error('Failed to load categories', err);
        this.categories = [];
      },
    });
  }

  private loadProducts(): void {
    this.isLoading = true;
    this.hasError = false;

    const { sortBy, sortDirection } = this.mapSortKey(this.sortKey);

    const query: ProductsListQuery = {
      searchTerm: this.searchTerm || undefined,
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy,
      sortDirection,
      categoryId: this.selectedCategoryId ?? undefined,
      colorHex: this.selectedColorHex ?? undefined,
      minPrice: this.minPrice != null ? this.minPrice : undefined,
      maxPrice: this.maxPrice != null ? this.maxPrice : undefined,
    };

    this.productService.getAll(query).subscribe({
      next: (res: ApiResponse<IProduct[]>) => {
        this.products = res.data ?? [];
        this.currentPage = res.currentPage ?? this.currentPage;
        this.pageSize = res.pageSize ?? this.pageSize;
        this.totalItems = res.totalItems ?? this.products.length;
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Failed to load products', err);
        this.products = [];
        this.totalItems = 0;
        this.isLoading = false;
        this.hasError = true;
        this.errorMessage = 'Failed to load products. Please try again.';
      },
    });
  }

  private mapSortKey(sortKey: string): { sortBy?: string; sortDirection?: 'Asc' | 'Desc' } {
    switch (sortKey) {
      case 'latest':
        return { sortBy: 'CreatedAt', sortDirection: 'Desc' };
      case 'popular':
        return { sortBy: 'ViewCount', sortDirection: 'Desc' };
      case 'priceAsc':
        return { sortBy: 'Price', sortDirection: 'Asc' };
      case 'priceDesc':
        return { sortBy: 'Price', sortDirection: 'Desc' };
      default:
        // relevance: let backend default (Name asc)
        return {};
    }
  }
}
