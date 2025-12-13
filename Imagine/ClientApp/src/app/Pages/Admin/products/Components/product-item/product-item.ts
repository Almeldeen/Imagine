import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { IProduct } from '../../Core/Interface/IProduct';
import { environment } from '../../../../../../environments/environment';
import { ProductService } from '../../Core/Service/product.service';
import { ToastService } from '../../../../../core/toast.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-product-item',
  imports: [CommonModule],
  templateUrl: './product-item.html',
  styleUrl: './product-item.css',
})
export class ProductItem {
  @Input() product!: IProduct;
  @Output() onEdit = new EventEmitter<number>();
  @Output() onDelete = new EventEmitter<number>();
  @Output() onView = new EventEmitter<number>();

  @Output() refresh = new EventEmitter<void>();

  baseUrl: string = environment.apiUrl;
  private readonly router = inject(Router);
  private readonly productService = inject(ProductService);
  private readonly toast = inject(ToastService);

  get imageSrc(): string {
    const url = this.product?.imageUrl;

    if (!url) {
      return '/assets/images/hero-banner.png';
    }

    if (url.startsWith('http')) {
      return url;
    }

    if (url.startsWith('/')) {
      return this.baseUrl + url;
    }

    return this.baseUrl + '/' + url;
  }

  viewProduct(): void {
    if (!this.product?.id) {
      return;
    }
    this.router.navigate(['/admin/products/view', this.product.id]);
  }

  editProduct(): void {
    if (!this.product?.id) {
      return;
    }
    this.router.navigate(['/admin/products/edit', this.product.id]);
  }

  duplicateProduct(): void {
    if (!this.product?.id) {
      return;
    }

    this.router.navigate(['/admin/products/add'], {
      queryParams: { sourceId: this.product.id },
    });
  }

  deleteProduct(): void {
    if (!this.product?.id) {
      return;
    }

    Swal.fire({
      icon: 'warning',
      title: 'Delete product?',
      text: `This will permanently delete "${this.product.name}".`,
      showCancelButton: true,
      confirmButtonText: 'Yes, delete',
      cancelButtonText: 'Cancel',
    }).then((result) => {
      if (!result.isConfirmed) {
        return;
      }

      this.productService.delete(this.product.id).subscribe({
        next: (res) => {
          if (!res.success) {
            this.toast.error(res.message || 'Failed to delete product.');
            return;
          }

          this.toast.success('Product deleted.');
          this.refresh.emit();
        },
        error: () => {
          this.toast.error('Failed to delete product.');
        },
      });
    });
  }
}
