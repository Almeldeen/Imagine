import { Component, Output, EventEmitter, inject } from '@angular/core';
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
  @Output() filterChange = new EventEmitter<string>();
  @Output() viewChange = new EventEmitter<string>();
  @Output() sortChange = new EventEmitter<string>();

  onAddProduct() {
    this.router.navigate(['/admin/products/add']);
  }

  onFilterChange(filter: string) {
    this.filterChange.emit(filter);
  }

  onViewChange(view: string) {
    this.viewChange.emit(view);
  }

  onSortChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.sortChange.emit(select.value);
  }
}
