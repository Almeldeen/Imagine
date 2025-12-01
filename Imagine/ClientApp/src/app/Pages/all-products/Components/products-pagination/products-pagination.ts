import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-products-pagination',
  standalone: true,
  imports: [CommonModule, NgbPaginationModule],
  templateUrl: './products-pagination.html',
  styleUrl: './products-pagination.css',
})
export class ProductsPagination {
  @Input() page = 1;
  @Input() pageSize = 9;
  @Input() total = 0;
  @Output() pageChange = new EventEmitter<number>();
}
