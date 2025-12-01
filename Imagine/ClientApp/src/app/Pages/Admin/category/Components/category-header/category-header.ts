import { Component, Output, EventEmitter, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-category-header',
  imports: [CommonModule],
  templateUrl: './category-header.html',
  styleUrl: './category-header.css',
})
export class CategoryHeader {
  @Input() categories: any[] = [];
  @Input() currentFilter: string = 'All';
  @Output() addCategory = new EventEmitter<void>();
  @Output() filterChange = new EventEmitter<string>();
  @Output() viewChange = new EventEmitter<string>();
  @Output() searchChange = new EventEmitter<string>();

  onSearchChange(term: string) {
    this.searchChange.emit(term);
  }

  get activeCategoriesCount(): number {
    return this.categories.filter(cat => cat.isActive).length;
  }

  get archivedCategoriesCount(): number {
    return this.categories.filter(cat => !cat.isActive).length;
  }

  onAddCategory() {
    this.addCategory.emit();
  }

  onFilterChange(filter: string) {
    this.currentFilter = filter;
    this.filterChange.emit(filter);
  }

  currentView: string = 'grid';

  onViewChange(view: string) {
    this.currentView = view;
    this.viewChange.emit(view);
  }
}
