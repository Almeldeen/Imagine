import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import type { ProductColorVariant } from '../../product-details';

@Component({
  selector: 'app-product-color-selector',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-color-selector.html',
  styleUrl: './product-color-selector.css',
})
export class ProductColorSelector {
  @Input() colors: ProductColorVariant[] = [];
  @Input() selectedKey: string | null = null;
  @Input() selectedLabel: string | null = null;

  @Output() selectedChange = new EventEmitter<string>();

  onSelect(key: string) {
    this.selectedChange.emit(key);
  }
}
