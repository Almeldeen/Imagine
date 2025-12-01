import { Component, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-category-empty-state',
  imports: [],
  templateUrl: './category-empty-state.html',
  styleUrl: './category-empty-state.css',
})
export class CategoryEmptyState {
  @Output() createCategory = new EventEmitter<void>();

  onCreateCategory() {
    this.createCategory.emit();
  }
}
