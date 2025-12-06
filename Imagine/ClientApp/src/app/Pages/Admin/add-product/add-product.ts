import { Component, inject, ViewChild, ElementRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { ProductService } from '../products/Core/Service/product.service';
import Swal from 'sweetalert2';
import { CreateProductRequestModel } from '../products/Core/Interface/IProduct';
import { CategoryService } from '../category/Core/Service/category.service';
import { ICategory } from '../category/Core/Interface/ICategory';

interface ProductColor {
  id?: number;
  colorName: string;
  colorHex: string;
  stock: number;
  additionalPrice: number;
  isAvailable: boolean;
  images: ProductImage[];
}

interface ProductImage {
  id?: number;
  imageUrl: string;
  altText: string;
  isMain: boolean;
  displayOrder: number;
  file?: File;
  fileKey?: string;
}

interface Product {
  categoryId: number;
  name: string;
  description: string;
  basePrice: number;
  mainImageUrl: string;
  isActive: boolean;
  isFeatured: boolean;
  isPopular: boolean;
  isLatest: boolean;
  colors: ProductColor[];
  mainImageFile?: File;
}

@Component({
  selector: 'app-add-product',
  imports: [CommonModule, FormsModule],
  templateUrl: './add-product.html',
  styleUrl: './add-product.css',
})
export class AddProduct implements OnInit {
  private modalService = inject(NgbModal);
  private router = inject(Router);
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);

  @ViewChild('mainImageInput') mainImageInput!: ElementRef<HTMLInputElement>;
  @ViewChild('colorImageInput') colorImageInput!: ElementRef<HTMLInputElement>;

  // Product model based on database structure
  product: Product = {
    categoryId: 0,
    name: '',
    description: '',
    basePrice: 0,
    mainImageUrl: '',
    isActive: true,
    isFeatured: false,
    isPopular: false,
    isLatest: false,
    colors: []
  };

  // Available categories (loaded from API)
  categories: ICategory[] = [];

  // Main image preview
  mainImagePreview: string | null = null;

  // Color management
  selectedColorIndex: number = -1;
  showColorPicker: boolean = false;

  constructor() {}

  ngOnInit(): void {
    this.loadCategories();
  }

  private loadCategories(): void {
    this.categoryService.getAll().subscribe({
      next: (res) => {
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

  // File input click handlers
  triggerMainImageInput() {
    this.mainImageInput.nativeElement.click();
  }

  triggerColorImageInput() {
    this.colorImageInput.nativeElement.click();
  }

  // Main image handling
  onMainImageSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      if (file.type.startsWith('image/')) {
        this.product.mainImageFile = file;
        
        const reader = new FileReader();
        reader.onload = (e) => {
          this.mainImagePreview = e.target?.result as string;
        };
        reader.readAsDataURL(file);
      }
    }
  }

  clearMainImage() {
    this.product.mainImageFile = undefined;
    this.mainImagePreview = null;
    this.product.mainImageUrl = '';
  }

  // Color management
  addNewColor() {
    const newColor: ProductColor = {
      colorName: '',
      colorHex: '#000000',
      stock: 0,
      additionalPrice: 0,
      isAvailable: true,
      images: []
    };
    this.product.colors.push(newColor);
    this.selectedColorIndex = this.product.colors.length - 1;
  }

  removeColor(index: number) {
    this.product.colors.splice(index, 1);
    if (this.selectedColorIndex >= this.product.colors.length) {
      this.selectedColorIndex = this.product.colors.length - 1;
    }
  }

  selectColor(index: number) {
    this.selectedColorIndex = index;
  }

  // Color image management
  onColorImageSelected(event: Event, colorIndex: number) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const files = Array.from(input.files);
      
      files.forEach((file, index) => {
        if (file.type.startsWith('image/')) {
          const reader = new FileReader();
          reader.onload = (e) => {
            const existingCount = this.product.colors[colorIndex].images.length;
            const fileKey = `color_${colorIndex}_image_${existingCount + index}`;
            const newImage: ProductImage = {
              imageUrl: e.target?.result as string,
              altText: `${this.product.colors[colorIndex].colorName} - Image ${this.product.colors[colorIndex].images.length + 1}`,
              isMain: this.product.colors[colorIndex].images.length === 0,
              displayOrder: this.product.colors[colorIndex].images.length,
              file: file,
              fileKey: fileKey
            };
            this.product.colors[colorIndex].images.push(newImage);
          };
          reader.readAsDataURL(file);
        }
      });
    }
  }

  removeColorImage(colorIndex: number, imageIndex: number) {
    this.product.colors[colorIndex].images.splice(imageIndex, 1);
    // Reorder remaining images
    this.product.colors[colorIndex].images.forEach((img, idx) => {
      img.displayOrder = idx;
      if (idx === 0) img.isMain = true;
      else img.isMain = false;
    });
  }

  setMainColorImage(colorIndex: number, imageIndex: number) {
    this.product.colors[colorIndex].images.forEach((img, idx) => {
      img.isMain = idx === imageIndex;
    });
  }

  moveColorImage(colorIndex: number, fromIndex: number, toIndex: number) {
    const images = this.product.colors[colorIndex].images;
    const item = images.splice(fromIndex, 1)[0];
    images.splice(toIndex, 0, item);
    
    // Update display order
    images.forEach((img, idx) => {
      img.displayOrder = idx;
    });
  }

  // Form validation
  isFormValid(): boolean {
    return !!(
      this.product.name.trim() &&
      this.product.basePrice > 0 &&
      this.product.categoryId > 0 &&
      this.product.colors.length > 0 &&
      this.product.colors.every(color => 
        color.colorName.trim() && 
        color.colorHex &&
        color.stock >= 0
      )
    );
  }

  // Form submission
  onSave() {
    if (!this.isFormValid()) {
      Swal.fire({
        icon: 'warning',
        title: 'Invalid form',
        text: 'Please fill all required fields before saving.',
      });
      return;
    }

    // Build create request model matching backend CreateProductRequestDto
    const createModel: CreateProductRequestModel = {
      categoryId: this.product.categoryId,
      name: this.product.name,
      description: this.product.description,
      price: this.product.basePrice,
      isActive: this.product.isActive,
      isFeatured: this.product.isFeatured,
      isPopular: this.product.isPopular,
      isLatest: this.product.isLatest,
      colors: this.product.colors.map((color, colorIndex) => ({
        colorName: color.colorName,
        colorHex: color.colorHex,
        stock: color.stock,
        additionalPrice: color.additionalPrice,
        isAvailable: color.isAvailable,
        images: color.images.map((img, imgIndex) => {
          // Ensure each image has a stable fileKey
          if (!img.fileKey) {
            img.fileKey = `color_${colorIndex}_image_${imgIndex}`;
          }
          return {
            fileKey: img.fileKey,
            altText: img.altText,
            isMain: img.isMain,
            displayOrder: img.displayOrder,
            file: img.file,
          };
        }),
      })),
    };

    this.productService.createFullProduct(createModel, this.product.mainImageFile).subscribe({
      next: (res) => {
        Swal.fire({
          icon: res.success ? 'success' : 'error',
          title: res.success ? 'Product created' : 'Create failed',
          text:
            res.message ||
            (res.success ? 'Product created successfully' : 'Failed to create product'),
        }).then(() => {
          if (res.success) {
            this.router.navigate(['/admin/products']);
          }
        });
      },
      error: (err) => {
        Swal.fire({
          icon: 'error',
          title: 'Create failed',
          text: 'Error: ' + (err?.error?.message || JSON.stringify(err)),
        });
      },
    });
  }

  onCancel() {
    this.router.navigate(['/admin/products']);
  }

  // Utility methods
  formatPrice(price: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(price);
  }

  getTotalPrice(basePrice: number, additionalPrice: number): string {
    return this.formatPrice(basePrice + additionalPrice);
  }

  getMaxAdditionalPrice(): number {
    if (this.product.colors.length === 0) return 0;
    return Math.max(...this.product.colors.map(color => color.additionalPrice));
  }
}
