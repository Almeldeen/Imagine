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
  @Output() sortChange = new EventEmitter<string>();

  search = '';
  sortKey = 'relevance';

  onSearchChange() {
    this.searchChange.emit(this.search);
  }

  onSortChange(value: string) {
    this.sortKey = value;
    this.sortChange.emit(this.sortKey);
  }
}
