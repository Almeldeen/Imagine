import { Component, Output, EventEmitter, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-customer-header',
  imports: [CommonModule],
  templateUrl: './customer-header.html',
  styleUrls: ['./customer-header.css'],
})
export class CustomerHeader {
  @Input() currentFilter: 'all' | 'active' | 'inactive' | 'premium' = 'all';
  @Input() currentView: 'grid' | 'list' = 'grid';
  @Input() currentSort: 'name' | 'date' | 'status' = 'date';

  @Input() allCount = 0;
  @Input() activeCount = 0;
  @Input() inactiveCount = 0;
  @Input() premiumCount = 0;

  @Output() filterChange = new EventEmitter<string>();
  @Output() viewChange = new EventEmitter<string>();
  @Output() sortChange = new EventEmitter<string>();
  @Output() searchChange = new EventEmitter<string>();
  @Output() exportClick = new EventEmitter<void>();
  @Output() importClick = new EventEmitter<void>();

  onFilterChange(filter: string) {
    this.currentFilter = filter as any;
    this.filterChange.emit(filter);
  }

  onViewChange(view: string) {
    this.currentView = view as any;
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

  onExportClick() {
    this.exportClick.emit();
  }

  onImportClick() {
    this.importClick.emit();
  }
}
