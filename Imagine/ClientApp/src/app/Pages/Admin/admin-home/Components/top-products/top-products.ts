import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService } from '../../../products/Core/Service/product.service';
import { IProduct } from '../../../products/Core/Interface/IProduct';
import { ApiResponse } from '../../../../../core/IApiResponse';

@Component({
  selector: 'app-top-products',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './top-products.html',
  styleUrls: ['./top-products.css'],
})
export class TopProducts implements OnInit {
  private readonly productService = inject(ProductService);

  topProducts: IProduct[] = [];

  ngOnInit(): void {
    this.loadTopProducts();
  }

  private loadTopProducts(): void {
    this.productService.getPopularProducts(4).subscribe({
      next: (res: ApiResponse<IProduct[]>) => {
        if (res.success && res.data) {
          this.topProducts = res.data;
        } else {
          this.topProducts = [];
        }
      },
      error: () => {
        this.topProducts = [];
      },
    });
  }
}
