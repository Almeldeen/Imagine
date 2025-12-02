import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CategoryHeader } from './Components/category-header/category-header';
import { CategoryList } from './Components/category-list/category-list';
import { CategoryEmptyState } from './Components/category-empty-state/category-empty-state';
import { CategoryForm } from './Components/category-form/category-form';
import { CategoryService } from './Core/Service/category.service';
import { ApiResponse } from '../../../core/IApiResponse';
import { ICategory } from './Core/Interface/ICategory';

@Component({
  selector: 'app-category',
  imports: [CommonModule, CategoryHeader, CategoryList, CategoryEmptyState],
  templateUrl: './category.html',
  styleUrl: './category.css',
})
export class Category implements OnInit {
  private modalService = inject(NgbModal);
  private categoryService = inject(CategoryService) as CategoryService;

  allCategories: ICategory[] = [];
  categories: ICategory[] = [];
  hasCategories = false;
  searchTerm: string = '';
  currentFilter: string = 'All';

  ngOnInit() {
    this.loadCategories();
  }

  openAddCategoryForm() {
    const modalRef = this.modalService.open(CategoryForm, { size: 'lg', backdrop: 'static' });
    modalRef.result.then(
      (result) => {
        if (result) {
          this.loadCategories();
        }
      },
      (reason) => {
        // Modal dismissed
      }
    );
  }

  loadCategories() {
    this.categoryService.getAll().subscribe({
      next: (res: ApiResponse<ICategory[]>) => {
        this.allCategories = res.data;
        this.filterCategories();
      },
      error: (err: any) => console.error('Failed to load categories', err)
    });
  }

  filterCategories() {
    let filtered = this.allCategories;

    // Apply search
    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(cat => 
        cat.name.toLowerCase().includes(term) || 
        cat.description.toLowerCase().includes(term)
      );
    }

    // Apply status filter
    if (this.currentFilter === 'Active') {
      filtered = filtered.filter(cat => cat.isActive);
    } else if (this.currentFilter === 'Archived') {
      filtered = filtered.filter(cat => !cat.isActive);
    }

    this.categories = filtered;
    this.hasCategories = this.categories.length > 0;
  }

  onSearchChange(term: string) {
    this.searchTerm = term;
    this.filterCategories();
  }

  onFilterChange(filter: string) {
    this.currentFilter = filter;
    this.filterCategories();
  }

  currentView: string = 'grid';

  onViewChange(view: string) {
    this.currentView = view;
  }
}
