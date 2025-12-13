import { Component, inject, ViewChild, ElementRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Router, ActivatedRoute } from '@angular/router';
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
  allowAiCustomization: boolean;
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
  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);

  @ViewChild('mainImageInput') mainImageInput!: ElementRef<HTMLInputElement>;
  @ViewChild('colorImageInput') colorImageInput!: ElementRef<HTMLInputElement>;

  isEditMode = false;
  currentProductId: number | null = null;

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
     allowAiCustomization: false,
    colors: []
  };

  // Available categories (loaded from API)
  categories: ICategory[] = [];

  allSizes: string[] = ['XS', 'S', 'M', 'L', 'XL', 'XXL', 'XXXL'];
  selectedSizes: string[] = [];

  // Main image preview
  mainImagePreview: string | null = null;

  // Color management
  selectedColorIndex: number = -1;
  showColorPicker: boolean = false;

  constructor() {}

  ngOnInit(): void {
    this.loadCategories();
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.isEditMode = true;
        this.currentProductId = +id;
        this.loadProductData(this.currentProductId);
      }
    });
  }

  private loadProductData(id: number) {
    this.productService.getById(id).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const data = res.data;
          console.log('Loaded product data:', data);
          console.log('CategoryId from API:', data.categoryId, 'Type:', typeof data.categoryId);
          
          // Ensure categoryId is properly set
          let loadedCategoryId = 0;
          if (data.categoryId !== undefined && data.categoryId !== null) {
            loadedCategoryId = Number(data.categoryId);
            if (isNaN(loadedCategoryId)) {
              console.error('CategoryId is not a valid number:', data.categoryId);
              loadedCategoryId = 0;
            }
          } else {
            console.warn('CategoryId is missing from API response. Product may need a category assigned.');
          }
          
          // Ensure price is properly set
          let loadedPrice = 0;
          if (data.price !== undefined && data.price !== null) {
            loadedPrice = Number(data.price);
            if (isNaN(loadedPrice)) {
              console.error('Price is not a valid number:', data.price);
              loadedPrice = 0;
            }
          } else {
            console.warn('Price is missing from API response.');
          }
          
          // Ensure name is properly set
          const loadedName = data.name || '';
          if (!loadedName) {
            console.warn('Product name is missing from API response.');
          }
          
          this.product = {
            categoryId: loadedCategoryId,
            name: loadedName,
            description: data.description || '',
            basePrice: loadedPrice,
            mainImageUrl: data.imageUrl || '', // API returns 'imageUrl'
            isActive: data.isActive ?? true,
            isFeatured: data.isFeatured ?? false,
            isPopular: data.isPopular ?? false,
            isLatest: data.isLatest ?? false,
            allowAiCustomization: data.allowAiCustomization ?? false,
            colors: [],
          };
          
          console.log('Product after mapping:', this.product);
          console.log('Data validation:', {
            categoryId: {
              original: data.categoryId,
              converted: loadedCategoryId,
              type: typeof loadedCategoryId,
              isValid: loadedCategoryId > 0
            },
            price: {
              original: data.price,
              converted: loadedPrice,
              type: typeof loadedPrice,
              isValid: loadedPrice > 0
            },
            name: {
              original: data.name,
              converted: loadedName,
              isValid: loadedName.length > 0
            }
          });
          
          // Show warning if critical data is missing
          if (loadedCategoryId === 0) {
            Swal.fire({
              icon: 'warning',
              title: 'Category Missing',
              text: 'This product does not have a category assigned. Please select a category before saving.',
              timer: 5000,
              showConfirmButton: true
            });
          }
          
          if (loadedPrice === 0) {
            Swal.fire({
              icon: 'warning',
              title: 'Price Missing',
              text: 'This product does not have a price. Please set a price before saving.',
              timer: 5000,
              showConfirmButton: true
            });
          }
          
          if (!loadedName) {
            Swal.fire({
              icon: 'warning',
              title: 'Name Missing',
              text: 'This product does not have a name. Please set a name before saving.',
              timer: 5000,
              showConfirmButton: true
            });
          }
          
          this.mainImagePreview = this.product.mainImageUrl;
          
          // Map colors from IProductColor interface
          if (data.colors && Array.isArray(data.colors)) {
             this.product.colors = data.colors.map((c) => ({
                id: c.id,
                colorName: c.colorName || '',
                colorHex: c.colorHex || '#000000',
                stock: c.stock || 0,
                additionalPrice: c.additionalPrice || 0,
                isAvailable: c.isAvailable ?? true,
                images: (c.images || []).map((img) => ({
                   id: img.id,
                   imageUrl: img.imageUrl || '',
                   altText: img.altText || '',
                   isMain: !!img.isMain,
                   displayOrder: img.displayOrder || 0,
                   fileKey: undefined, // Will be set if new file is added
                   file: undefined // Will be set if new file is added
                }))
             }));
          }
        }
      },
      error: (err) => {
        Swal.fire('Error', 'Failed to load product', 'error');
        this.router.navigate(['/admin/products']);
      }
    });
  }

  isSizeSelected(size: string): boolean {
    return this.selectedSizes.includes(size);
  }

  toggleSize(size: string) {
    if (this.isSizeSelected(size)) {
      this.selectedSizes = this.selectedSizes.filter((s) => s !== size);
    } else {
      this.selectedSizes = [...this.selectedSizes, size];
    }
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

  onCategoryChange(value: any): void {
    // Ensure categoryId is always a number
    this.product.categoryId = value ? Number(value) : 0;
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

    // Prepare payload (reused for both create and update)
    // Note: For update, backend might expect the same structure in a 'Payload' field
    const createModel: CreateProductRequestModel = {
      categoryId: this.product.categoryId,
      name: this.product.name,
      description: this.product.description,
      price: this.product.basePrice,
      isActive: this.product.isActive,
      isFeatured: this.product.isFeatured,
      isPopular: this.product.isPopular,
      isLatest: this.product.isLatest,
      allowAiCustomization: this.product.allowAiCustomization,
      availableSizes: this.selectedSizes.length ? this.selectedSizes.join(',') : undefined,
      colors: this.product.colors.map((color, colorIndex) => ({
        colorName: color.colorName,
        colorHex: color.colorHex,
        stock: color.stock,
        additionalPrice: color.additionalPrice,
        isAvailable: color.isAvailable,
        images: color.images.map((img, imgIndex) => {
          if (!img.fileKey) { // Only generate key if new file or missing
             img.fileKey = `color_${colorIndex}_image_${imgIndex}`;
          }
          return {
            fileKey: img.fileKey || `color_${colorIndex}_image_${imgIndex}`, // Fallback to ensure string
            altText: img.altText,
            isMain: img.isMain,
            displayOrder: img.displayOrder,
            file: img.file,
          };
        }),
      })),
    };

    if (this.isEditMode && this.currentProductId) {
       // UPDATE MODE
       // We'll reuse createFullProduct logic but call update. 
       // However, product.service.update takes (id, formData). 
       // We need to construct FormData same as createFullProduct.
       
       // Validate required fields before constructing payload
       const name = (this.product.name || '').trim();
       const categoryId = this.product.categoryId ? Number(this.product.categoryId) : 0;
       const price = this.product.basePrice ? Number(this.product.basePrice) : 0;
       
       console.log('Validation Check:', {
         name: name,
         nameLength: name.length,
         categoryId: categoryId,
         categoryIdType: typeof categoryId,
         price: price,
         priceType: typeof price,
         productCategoryId: this.product.categoryId,
         productBasePrice: this.product.basePrice
       });
       
       if (!name || name.length === 0) {
         Swal.fire({
           icon: 'error',
           title: 'Validation Error',
           text: 'Product name is required.',
         });
         return;
       }
       
       if (!categoryId || categoryId <= 0 || isNaN(categoryId)) {
         Swal.fire({
           icon: 'error',
           title: 'Validation Error',
           text: `Please select a valid category. Current value: ${this.product.categoryId}`,
         });
         return;
       }
       
       if (!price || price <= 0 || isNaN(price)) {
         Swal.fire({
           icon: 'error',
           title: 'Validation Error',
           text: `Price must be greater than 0. Current value: ${this.product.basePrice}`,
         });
         return;
       }
       
       const form = new FormData();
       
       // Transform payload for backend (remove file objects, keep structure)
       // Use validated values directly from product - ensure all are correct types
       // Match the exact format used in createFullProduct
       const apiPayload = {
          categoryId: Number(categoryId), // Ensure it's a number
          name: String(name).trim(), // Ensure it's a string
          description: (this.product.description || '').trim() || undefined,
          price: Number(price), // Ensure it's a number
          isActive: Boolean(this.product.isActive ?? true),
          isFeatured: Boolean(this.product.isFeatured ?? false),
          isPopular: Boolean(this.product.isPopular ?? false),
          isLatest: Boolean(this.product.isLatest ?? false),
          colors: (this.product.colors || []).map(color => {
            const colorPayload: any = {
              colorName: color.colorName.trim(),
              colorHex: color.colorHex,
              stock: Number(color.stock),
              additionalPrice: Number(color.additionalPrice),
              isAvailable: color.isAvailable,
              images: (color.images || [])
                .filter(img => img.id || (img.file && img.fileKey)) // Only include images with id or new file
                .map(img => {
                  const imagePayload: any = {
                    altText: img.altText || '',
                    isMain: img.isMain,
                    displayOrder: Number(img.displayOrder),
                  };
                  // Only include fileKey if it's a new image (has file), otherwise include id for existing images
                  if (img.file && img.fileKey) {
                    imagePayload.fileKey = img.fileKey;
                  } else if (img.id) {
                    imagePayload.id = img.id;
                  }
                  return imagePayload;
                }),
            };
            // Only include color id if it exists (for updates)
            if (color.id) {
              colorPayload.id = color.id;
            }
            return colorPayload;
          }),
        };

       // Final validation check before sending
       if (!apiPayload.name || apiPayload.name.length === 0) {
         Swal.fire({
           icon: 'error',
           title: 'Validation Error',
           text: 'Product name cannot be empty after processing.',
         });
         return;
       }
       
       if (!apiPayload.categoryId || apiPayload.categoryId <= 0) {
         Swal.fire({
           icon: 'error',
           title: 'Validation Error',
           text: `Category ID is invalid: ${apiPayload.categoryId}. Please select a valid category.`,
         });
         return;
       }
       
       if (!apiPayload.price || apiPayload.price <= 0) {
         Swal.fire({
           icon: 'error',
           title: 'Validation Error',
           text: `Price is invalid: ${apiPayload.price}. Price must be greater than 0.`,
         });
         return;
       }

       // Debug: Log the payload to console
       console.log('=== UPDATE PAYLOAD DEBUG ===');
       console.log('Product Data (before validation):', {
         name: this.product.name,
         categoryId: this.product.categoryId,
         basePrice: this.product.basePrice,
         categoryIdType: typeof this.product.categoryId,
         basePriceType: typeof this.product.basePrice
       });
       console.log('Validated Values:', {
         name: name,
         nameLength: name.length,
         categoryId: categoryId,
         categoryIdType: typeof categoryId,
         price: price,
         priceType: typeof price
       });
       console.log('API Payload Object:', apiPayload);
       console.log('API Payload Types:', {
         categoryId: typeof apiPayload.categoryId,
         name: typeof apiPayload.name,
         price: typeof apiPayload.price
       });
       console.log('API Payload JSON:', JSON.stringify(apiPayload, null, 2));
       console.log('===========================');

       form.append('Payload', JSON.stringify(apiPayload));

       if (this.product.mainImageFile) {
         form.append('MainImageFile', this.product.mainImageFile);
       }

       // Append color images
       (this.product.colors || []).forEach(color => {
          (color.images || []).forEach(img => {
            if (img.file && img.fileKey) {
              form.append(img.fileKey, img.file);
            }
          });
        });

       this.productService.update(this.currentProductId, form).subscribe({
          next: (res) => {
             Swal.fire({
              icon: res.success ? 'success' : 'error',
              title: res.success ? 'Product updated' : 'Update failed',
              text: res.message || (res.success ? 'Product updated successfully' : 'Failed to update product'),
            }).then(() => {
              if (res.success) this.router.navigate(['/admin/products']);
            });
          },
          error: (err) => {
             console.error('Update error:', err);
             const errorMessage = err?.error?.message || err?.error?.errors || err?.message || 'Unknown error';
             Swal.fire({
               icon: 'error',
               title: 'Update failed',
               text: `Error: ${errorMessage}`,
               footer: err?.error ? JSON.stringify(err.error, null, 2) : undefined
             });
          }
       });

    } else {
      // CREATE MODE
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
  }

  onCancel() {
    this.router.navigate(['/admin/products']);
  }

  // Utility methods
  formatPrice(price: number): string {
    // Format price in English with EGP currency code
    // Use toFixed to ensure 2 decimal places, then format with EGP
    const formattedNumber = Number(price).toFixed(2);
    return `EGP ${formattedNumber}`;
  }

  getTotalPrice(basePrice: number, additionalPrice: number): string {
    return this.formatPrice(basePrice + additionalPrice);
  }

  getMaxAdditionalPrice(): number {
    if (this.product.colors.length === 0) return 0;
    return Math.max(...this.product.colors.map(color => color.additionalPrice));
  }
}
