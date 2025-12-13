import { Component, OnInit, inject, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../Core/Service/product.service';
import { CategoryService } from '../../category/Core/Service/category.service';
import { ICategory } from '../../category/Core/Interface/ICategory';
import { IProduct, IProductColor, IProductImage } from '../Core/Interface/IProduct';
import { ApiResponse } from '../../../../core/IApiResponse';
import { environment } from '../../../../../environments/environment';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-admin-edit-product',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-edit-product.html',
  styleUrl: './admin-edit-product.css',
})
export class AdminEditProduct implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);

  productId = 0;
  isLoading = false;
  isSaving = false;
  error = '';

  categories: ICategory[] = [];

  // Loaded colors (with images) for this product
  colors: IProductColor[] = [];

  // Currently selected color index for editing
  selectedColorIndex: number = -1;

  form = {
    name: '',
    description: '',
    price: 0,
    isActive: true,
    isFeatured: false,
    isPopular: false,
    isLatest: false,
    categoryId: 0,
    allowAiCustomization: false,
  };

  mainImageFile: File | null = null;
  mainImagePreview: string | null = null;
  readonly apiBase = environment.apiUrl;

  allSizes: string[] = ['XS', 'S', 'M', 'L', 'XL', 'XXL', 'XXXL'];
  selectedSizes: string[] = [];

  @ViewChild('mainImageInput') mainImageInput!: ElementRef<HTMLInputElement>;
  @ViewChild('colorImageInput') colorImageInput!: ElementRef<HTMLInputElement>;

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    const id = idParam ? Number(idParam) : NaN;

    if (!id || Number.isNaN(id)) {
      this.error = 'Invalid product id.';
      return;
    }

    this.productId = id;
    this.loadCategories();
    this.loadProduct();
  }

  isSizeSelected(size: string): boolean {
    return this.selectedSizes.includes(size);
  }

  toggleSize(size: string): void {
    if (this.isSizeSelected(size)) {
      this.selectedSizes = this.selectedSizes.filter((s) => s !== size);
    } else {
      this.selectedSizes = [...this.selectedSizes, size];
    }
  }

  private loadCategories(): void {
    this.categoryService.getAll().subscribe({
      next: (res: any) => {
        if (res?.success && Array.isArray(res.data)) {
          this.categories = res.data;
        } else {
          this.categories = [];
        }
      },
      error: () => {
        this.categories = [];
      },
    });
  }

  private loadProduct(): void {
    this.isLoading = true;
    this.productService.getById(this.productId).subscribe({
      next: (res: ApiResponse<IProduct>) => {
        this.isLoading = false;
        if (!res.success || !res.data) {
          this.error = res.message || 'Failed to load product.';
          return;
        }

        const p = res.data;
        this.form = {
          name: p.name,
          description: p.description || '',
          price: p.price,
          isActive: p.isActive,
          isFeatured: !!p.isFeatured,
          isPopular: !!p.isPopular,
          isLatest: !!p.isLatest,
          categoryId: p.categoryId ?? 0,
          allowAiCustomization: !!p.allowAiCustomization,
        };

        if (p.availableSizes) {
          this.selectedSizes = p.availableSizes.split(',').map((s) => s.trim()).filter((s) => !!s);
        } else {
          this.selectedSizes = [];
        }

        this.mainImagePreview = p.imageUrl ? this.resolveImageUrl(p.imageUrl) : null;

        this.colors = (p.colors ?? []).map((c) => ({
          ...c,
          images: (c.images ?? []) as IProductImage[],
        }));

        if (this.colors.length > 0) {
          this.selectedColorIndex = 0;
        }
      },
      error: () => {
        this.isLoading = false;
        this.error = 'Failed to load product.';
      },
    });
  }

  triggerMainImageInput(): void {
    if (this.mainImageInput) {
      this.mainImageInput.nativeElement.click();
    }
  }

  onMainImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      if (!file.type.startsWith('image/')) {
        return;
      }

      this.mainImageFile = file;

      const reader = new FileReader();
      reader.onload = (e) => {
        this.mainImagePreview = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  triggerColorImageInput(): void {
    if (this.colorImageInput) {
      this.colorImageInput.nativeElement.click();
    }
  }

  async onSave(): Promise<void> {
    if (!this.productId || this.isSaving) {
      return;
    }

    this.isSaving = true;
    this.error = '';

    try {
      // First, ensure all colors (including newly added ones) are persisted
      for (const color of this.colors) {
        const payload = {
          colorName: color.colorName,
          colorHex: color.colorHex,
          stock: color.stock,
          additionalPrice: color.additionalPrice,
          isAvailable: color.isAvailable,
        };

        if (color.id && color.id > 0) {
          const res = await firstValueFrom(this.productService.updateColor(color.id, payload));
          if (!res.success) {
            throw new Error(res.message || 'Failed to update color.');
          }
        } else {
          const res = await firstValueFrom(this.productService.addColor(this.productId, payload));
          if (!res.success || !res.data) {
            throw new Error(res.message || 'Failed to create color.');
          }

          // Assign the generated id so subsequent operations (like adding images) work correctly
          color.id = res.data;
        }
      }

      // Then update the main product fields and optional main image
      const formData = new FormData();
      formData.append('Name', this.form.name);
      formData.append('Description', this.form.description);
      formData.append('Price', String(this.form.price));
      formData.append('IsActive', String(this.form.isActive));
      formData.append('IsFeatured', String(this.form.isFeatured));
      formData.append('IsPopular', String(this.form.isPopular));
      formData.append('IsLatest', String(this.form.isLatest));
      formData.append('CategoryId', String(this.form.categoryId));
      formData.append('AllowAiCustomization', String(this.form.allowAiCustomization));
      formData.append('AvailableSizes', this.selectedSizes.join(','));

      if (this.mainImageFile) {
        formData.append('ImageFile', this.mainImageFile);
      }

      const updateRes = await firstValueFrom(this.productService.update(this.productId, formData));
      if (!updateRes.success) {
        throw new Error(updateRes.message || 'Failed to update product.');
      }

      this.router.navigate(['/admin/products']);
    } catch (err: any) {
      this.error = err?.message || 'Failed to save product changes.';
    } finally {
      this.isSaving = false;
    }
  }

  onCancel(): void {
    this.router.navigate(['/admin/products']);
  }

  isFormValid(): boolean {
    return !!(
      this.form.name.trim() &&
      this.form.price > 0 &&
      this.form.categoryId > 0 &&
      this.colors.length > 0 &&
      this.colors.every((color) => color.colorName.trim() && color.stock >= 0)
    );
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(price);
  }

  getTotalPrice(basePrice: number, additionalPrice: number): string {
    return this.formatPrice(basePrice + additionalPrice);
  }

  getMaxAdditionalPrice(): number {
    if (this.colors.length === 0) return 0;
    return Math.max(...this.colors.map((c) => c.additionalPrice));
  }

  resolveImageUrl(url: string | null | undefined): string {
    if (!url) {
      return '/assets/images/product-placeholder.jpg';
    }

    if (url.startsWith('http')) {
      return url;
    }

    if (url.startsWith('/')) {
      return this.apiBase + url;
    }

    return this.apiBase + '/' + url;
  }

  // === Color management ===

  addColor(): void {
    const newColor: IProductColor = {
      id: 0,
      productId: this.productId,
      colorName: '',
      colorHex: '#000000',
      stock: 0,
      additionalPrice: 0,
      isAvailable: true,
      images: [],
    };

    this.colors.push(newColor);
    this.selectedColorIndex = this.colors.length - 1;
  }

  selectColor(index: number): void {
    this.selectedColorIndex = index;
  }

  saveColor(index: number): void {
    const color = this.colors[index];
    if (!color) {
      return;
    }

    const payload = {
      colorName: color.colorName,
      colorHex: color.colorHex,
      stock: color.stock,
      additionalPrice: color.additionalPrice,
      isAvailable: color.isAvailable,
    };

    // Existing color -> update
    if (color.id && color.id > 0) {
      this.productService.updateColor(color.id, payload).subscribe({
        error: () => {
          this.error = 'Failed to update color.';
        },
      });
      return;
    }

    // New color -> add
    this.productService.addColor(this.productId, payload).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          color.id = res.data;
        } else {
          this.error = res.message || 'Failed to create color.';
        }
      },
      error: () => {
        this.error = 'Failed to create color.';
      },
    });
  }

  deleteColor(index: number): void {
    const color = this.colors[index];
    if (!color) {
      return;
    }

    // If color not yet saved, just remove locally
    if (!color.id || color.id <= 0) {
      this.colors.splice(index, 1);
      if (this.selectedColorIndex >= this.colors.length) {
        this.selectedColorIndex = this.colors.length - 1;
      }
      return;
    }

    this.productService.deleteColor(color.id).subscribe({
      next: (res) => {
        if (!res.success) {
          this.error = res.message || 'Failed to delete color.';
          return;
        }

        this.colors.splice(index, 1);
        if (this.selectedColorIndex >= this.colors.length) {
          this.selectedColorIndex = this.colors.length - 1;
        }
      },
      error: () => {
        this.error = 'Failed to delete color.';
      },
    });
  }

  // === Color images ===

  onColorImageSelected(event: Event, colorIndex: number): void {
    const input = event.target as HTMLInputElement;
    const color = this.colors[colorIndex];
    if (!color) {
      return;
    }

    if (!color.id || color.id <= 0) {
      this.error = 'Please save the color before adding images.';
      input.value = '';
      return;
    }

    if (input.files && input.files.length > 0) {
      const files = Array.from(input.files);

      files.forEach((file) => {
        if (!file.type.startsWith('image/')) {
          return;
        }

        const formData = new FormData();
        formData.append('ImageFile', file);

        this.productService.addImage(color.id!, formData).subscribe({
          next: (res) => {
            if (!res.success || !res.data) {
              this.error = res.message || 'Failed to add image.';
              return;
            }

            const reader = new FileReader();
            reader.onload = (e) => {
              const newImg: IProductImage = {
                id: res.data,
                productColorId: color.id!,
                imageUrl: (e.target?.result as string) || '',
                altText: '',
                isMain: color.images?.length ? false : true,
                displayOrder: color.images?.length || 0,
              };

              if (!color.images) {
                color.images = [];
              }
              color.images.push(newImg);
            };
            reader.readAsDataURL(file);
          },
          error: () => {
            this.error = 'Failed to add image.';
          },
        });
      });

      // reset input so same file can be selected again later
      input.value = '';
    }
  }

  removeColorImage(colorIndex: number, imageIndex: number): void {
    const color = this.colors[colorIndex];
    if (!color || !color.images || !color.images[imageIndex]) {
      return;
    }

    const img = color.images[imageIndex];

    // If image not yet persisted, just remove locally
    if (!img.id || img.id <= 0) {
      color.images.splice(imageIndex, 1);
      return;
    }

    this.productService.deleteImage(img.id).subscribe({
      next: (res) => {
        if (!res.success) {
          this.error = res.message || 'Failed to delete image.';
          return;
        }
        color.images!.splice(imageIndex, 1);
      },
      error: () => {
        this.error = 'Failed to delete image.';
      },
    });
  }
}
