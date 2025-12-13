import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ProductService } from '../Core/Service/product.service';
import { IProduct } from '../Core/Interface/IProduct';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-admin-product-view',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './admin-product-view.html',
  styleUrl: './admin-product-view.css',
})
export class AdminProductView implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly productService = inject(ProductService);

  product: IProduct | null = null;
  isLoading = false;
  error = '';
  readonly apiBase = environment.apiUrl;
  selectedImageUrl: string | null = null;

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    const id = idParam ? Number(idParam) : NaN;

    if (!id || Number.isNaN(id)) {
      this.error = 'Invalid product id.';
      return;
    }

    this.isLoading = true;
    this.productService.getById(id).subscribe({
      next: (res) => {
        this.isLoading = false;
        if (!res.success || !res.data) {
          this.error = res.message || 'Failed to load product details.';
          this.product = null;
          return;
        }
        this.product = res.data;
      },
      error: () => {
        this.isLoading = false;
        this.error = 'Failed to load product details.';
        this.product = null;
      },
    });
  }

  get mainImageUrl(): string {
    return this.resolveImageUrl(this.product?.imageUrl);
  }

  get galleryImages() {
    if (!this.product) {
      return [] as { url: string; alt?: string; isMain: boolean }[];
    }

    const images: { url: string; alt?: string; isMain: boolean }[] = [];

    if (this.product.imageUrl) {
      images.push({ url: this.product.imageUrl, alt: this.product.name, isMain: true });
    }

    for (const color of this.product.colors ?? []) {
      for (const img of color.images ?? []) {
        if (!img.imageUrl) continue;
        images.push({
          url: img.imageUrl,
          alt: img.altText || color.colorName,
          isMain: img.isMain,
        });
      }
    }

    const seen = new Set<string>();
    return images.filter((img) => {
      if (seen.has(img.url)) return false;
      seen.add(img.url);
      return true;
    });
  }

  openImageModal(url: string | null | undefined): void {
    if (!url) return;
    this.selectedImageUrl = url;
  }

  closeImageModal(): void {
    this.selectedImageUrl = null;
  }

  resolveImageUrl(url: string | null | undefined): string {
    if (!url) {
      return '/assets/images/hero-banner.png';
    }

    if (url.startsWith('http')) {
      return url;
    }

    if (url.startsWith('/')) {
      return this.apiBase + url;
    }

    return this.apiBase + '/' + url;
  }
}
