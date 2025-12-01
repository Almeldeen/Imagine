import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-products-filter',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './products-filter.html',
  styleUrl: './products-filter.css',
})
export class ProductsFilter {
  @Output() searchChange = new EventEmitter<string>();
  @Output() filterChange = new EventEmitter<{ category?: string; aiOnly?: boolean }>();
  @Output() sortChange = new EventEmitter<string>();

  search = '';
  selectedCategory = 'all';
  aiOnly = false;
  sortKey = 'name';

  onSearchChange() {
    this.searchChange.emit(this.search);
  }

  setCategory(category: string) {
    this.selectedCategory = category;
    this.emitFilter();
  }

  toggleAiOnly() {
    this.aiOnly = !this.aiOnly;
    this.emitFilter();
  }

  onSortChange(value: string) {
    this.sortKey = value;
    this.sortChange.emit(this.sortKey);
  }

  private emitFilter() {
    this.filterChange.emit({ category: this.selectedCategory, aiOnly: this.aiOnly });
  }
}
