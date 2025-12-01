import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-product-empty-state',
  imports: [CommonModule],
  templateUrl: './product-empty-state.html',
  styleUrls: ['./product-empty-state.css'],
})
export class ProductEmptyState {
  private router = inject(Router);

  onCreateProduct() {
    this.router.navigate(['/admin/products/add']);
  }
}
