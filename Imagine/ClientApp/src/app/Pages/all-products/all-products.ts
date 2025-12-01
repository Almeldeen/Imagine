import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductsFilter } from './Components/products-filter/products-filter';
import { ProductsCard } from './Components/products-card/products-card';
import { ProductsPagination } from './Components/products-pagination/products-pagination';

interface ProductColor {
  name: string;
  value: string;
  extraPrice?: number;
}

interface Product {
  id: number;
  name: string;
  category: string;
  basePrice: number;
  image: string;
  colors: ProductColor[];
  isAiPowered?: boolean;
  isFeatured?: boolean;
}

@Component({
  selector: 'app-all-products',
  standalone: true,
  imports: [CommonModule, ProductsFilter, ProductsCard, ProductsPagination],
  templateUrl: './all-products.html',
  styleUrl: './all-products.css',
})
export class AllProducts {
  products: Product[] = [
    {
      id: 1,
      name: 'Classic AI Hoodie',
      category: 'Hoodies',
      basePrice: 49.99,
      image: '/assets/images/Hoodie.png',
      isAiPowered: true,
      isFeatured: true,
      colors: [
        { name: 'Black', value: 'black' },
        { name: 'White', value: 'white', extraPrice: 3 },
        { name: 'Blue', value: 'blue', extraPrice: 5 },
      ],
    },
    {
      id: 2,
      name: 'Premium AI T-Shirt',
      category: 'T-Shirts',
      basePrice: 34.99,
      image: '/assets/images/T-Shirt.png',
      isAiPowered: true,
      colors: [
        { name: 'Black', value: 'black' },
        { name: 'White', value: 'white' },
        { name: 'Purple', value: 'purple', extraPrice: 4 },
      ],
    },
    {
      id: 3,
      name: 'Designer AI Hoodie',
      category: 'Hoodies',
      basePrice: 54.99,
      image: '/assets/images/Hoodie.png',
      isAiPowered: true,
      colors: [
        { name: 'Navy', value: 'navy' },
        { name: 'Gray', value: 'gray' },
        { name: 'Pink', value: 'pink', extraPrice: 6 },
      ],
    },
    {
      id: 4,
      name: 'Minimalist Tote Bag',
      category: 'Accessories',
      basePrice: 24.99,
      image: '/assets/images/Tote.png',
      colors: [
        { name: 'Sand', value: 'sand' },
        { name: 'Graphite', value: 'graphite' },
      ],
    },
  ];

  filteredProducts: Product[] = this.products;

  currentPage = 1;
  pageSize = 9;

  get pagedProducts(): Product[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredProducts.slice(start, start + this.pageSize);
  }

  onSearchChange(term: string) {
    this.applyFilters({ search: term });
  }

  onFilterChange(filter: { category?: string; aiOnly?: boolean }) {
    this.applyFilters(filter);
  }

  onSortChange(sortKey: string) {
    const products = [...this.filteredProducts];
    if (sortKey === 'price') {
      products.sort((a, b) => a.basePrice - b.basePrice);
    } else if (sortKey === 'name') {
      products.sort((a, b) => a.name.localeCompare(b.name));
    }
    this.filteredProducts = products;
    this.currentPage = 1;
  }

  onPageChange(page: number) {
    this.currentPage = page;
  }

  private applyFilters(options: { search?: string; category?: string; aiOnly?: boolean }) {
    const search = options.search?.toLowerCase() ?? '';
    const category = options.category ?? 'all';
    const aiOnly = options.aiOnly ?? false;

    this.filteredProducts = this.products.filter((p) => {
      const matchesSearch = !search || p.name.toLowerCase().includes(search);
      const matchesCategory = category === 'all' || p.category === category;
      const matchesAi = !aiOnly || !!p.isAiPowered;
      return matchesSearch && matchesCategory && matchesAi;
    });

    this.currentPage = 1;
  }
}
