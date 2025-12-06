import { Component, Output, EventEmitter, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-product-header',
  imports: [CommonModule],
  templateUrl: './product-header.html',
  styleUrls: ['./product-header.css'],
})
export class ProductHeader {
  private router = inject(Router);
  @Input() allCount: number = 0;
  @Input() activeCount: number = 0;
  @Input() draftCount: number = 0;
  @Input() archivedCount: number = 0;
  @Output() searchChange = new EventEmitter<string>();
  @Output() filterChange = new EventEmitter<string>();
  @Output() viewChange = new EventEmitter<string>();
  @Output() sortChange = new EventEmitter<string>();

  currentFilter: string = 'all';
  currentView: string = 'grid';

  onSearchChange(term: string) {
    this.searchChange.emit(term);
  }

  onAddProduct() {
    this.router.navigate(['/admin/products/add']);
  }

  onFilterChange(filter: string) {
    this.currentFilter = filter;
    this.filterChange.emit(filter);
  }

  onViewChange(view: string) {
    this.currentView = view;
    this.viewChange.emit(view);
  }

  onSortChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.sortChange.emit(select.value);
  }
}
