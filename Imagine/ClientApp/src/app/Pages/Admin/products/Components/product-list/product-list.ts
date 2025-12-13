import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductItem } from '../product-item/product-item';
import { IProduct } from '../../Core/Interface/IProduct';

@Component({
  selector: 'app-product-list',
  imports: [CommonModule, ProductItem],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css',
})
export class ProductList {
  @Input() products: IProduct[] = [];
  @Input() viewMode: 'grid' | 'list' = 'grid';

  @Output() onEdit = new EventEmitter<number>();
  @Output() onDelete = new EventEmitter<number>();
  @Output() onView = new EventEmitter<number>();
}
