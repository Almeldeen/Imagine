import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-order-header',
  imports: [CommonModule],
  templateUrl: './order-header.html',
  styleUrls: ['./order-header.css'],
})
export class OrderHeader {
  @Output() filterChange = new EventEmitter<string>();
  @Output() viewChange = new EventEmitter<string>();
  @Output() sortChange = new EventEmitter<string>();
  @Output() searchChange = new EventEmitter<string>();
  @Output() dateRangeChange = new EventEmitter<any>();

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

  onSearchChange(event: Event) {
    const input = event.target as HTMLInputElement;
    this.searchChange.emit(input.value);
  }

  onDateRangeChange(event: Event) {
    const input = event.target as HTMLInputElement;
    this.dateRangeChange.emit(input.value);
  }
}
