import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import type { CustomizationOption } from '../../product-details';

@Component({
  selector: 'app-product-customization-options',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-customization-options.html',
  styleUrl: './product-customization-options.css',
})
export class ProductCustomizationOptions {
  @Input() options: CustomizationOption[] = [];
  @Input() selectedId: string | null = null;

  @Output() selectedChange = new EventEmitter<string | null>();

  onSelect(id: string) {
    this.selectedChange.emit(id);
  }
}
