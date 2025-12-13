import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductImageGallery } from './components/product-image-gallery/product-image-gallery';
import { ProductColorSelector } from './components/product-color-selector/product-color-selector';
import { ProductCustomizationOptions } from './components/product-customization-options/product-customization-options';
import { ProductInfo } from './components/product-info/product-info';
import { AddToCartButton } from './components/add-to-cart-button/add-to-cart-button';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../Admin/products/Core/Service/product.service';
import { IProduct, IProductColor } from '../Admin/products/Core/Interface/IProduct';
import { CartService } from '../../core/cart.service';
import Swal from 'sweetalert2';
import { environment } from '../../../environments/environment';

export interface ProductColorVariant {
  key: string;
  label: string;
  swatch: string;
  extraPrice?: number;
  imageUrl: string;
}

export interface CustomizationOption {
  id: string;
  name: string;
  description: string;
  extraPrice: number;
  badge?: string;
}

export interface ProductDetailsModel {
  name: string;
  basePrice: number;
  shortDescription: string;
  longDescription: string;
  inStock: boolean;
  stockLabel: string;
  isAiPowered?: boolean;
  availableSizes: string[];
  colors: ProductColorVariant[];
  customizations: CustomizationOption[];
}

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [
    CommonModule,
    ProductImageGallery,
    ProductColorSelector,
    ProductCustomizationOptions,
    ProductInfo,
    AddToCartButton,
  ],
  templateUrl: './product-details.html',
  styleUrl: './product-details.css',
})
export class ProductDetails implements OnInit {
  product: ProductDetailsModel = {
    name: 'Classic AI Hoodie',
    basePrice: 49.99,
    shortDescription:
      'A soft, heavyweight hoodie tuned for AI-generated art with rich, accurate color reproduction.',
    longDescription:
      'Built for creators who want their AI concepts to look as good in the real world as they do on screen. Premium cotton blend, double-stitched seams, and a print surface optimized for detailed, high-contrast artwork.',
    inStock: true,
    stockLabel: 'In stock - Ships in 3-5 business days',
    isAiPowered: true,
    availableSizes: ['XS', 'S', 'M', 'L', 'XL'],
    colors: [
      {
        key: 'black',
        label: 'Midnight Black',
        swatch: '#020617',
        imageUrl: '/assets/images/Hoodie.png',
      },
      {
        key: 'white',
        label: 'Cloud White',
        swatch: '#e5e7eb',
        extraPrice: 3,
        imageUrl: '/assets/images/Hoodie.png',
      },
      {
        key: 'purple',
        label: 'Neon Purple',
        swatch: '#8b5cf6',
        extraPrice: 5,
        imageUrl: '/assets/images/Hoodie.png',
      },
    ],
    customizations: [
      {
        id: 'baseline',
        name: 'Studio baseline',
        description: 'Clean, minimal print with subtle gradients. Great for everyday wear.',
        extraPrice: 0,
        badge: 'Included',
      },
      {
        id: 'ai-remix',
        name: 'AI remix',
        description: 'Let our model remix your artwork into a bolder, more experimental composition.',
        extraPrice: 7,
        badge: '+AI',
      },
      {
        id: 'signature-drop',
        name: 'Signature drop',
        description:
          'Limited-run layout with layered effects and a small creator signature lockup.',
        extraPrice: 12,
        badge: 'Limited',
      },
    ],
  };

  selectedColorKey = this.product.colors[0]?.key;
  selectedCustomizationId: string | null = this.product.customizations[0]?.id ?? null;
  quantity = 1;
  wishlistActive = false;
  selectedSize: string | null = null;
  hasUserSelectedColor = false;

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private productService = inject(ProductService);
  private cartService = inject(CartService);

  backendProduct: IProduct | null = null;
  isLoading = false;
  loadError = false;
  isAddingToCart = false;
  isAdminContext = false;

  goBack() {
    // Check if we're in admin context by checking query params or referrer
    const fromParam = this.route.snapshot.queryParams['from'];
    const referrer = document.referrer;
    const isAdminContext = fromParam === 'admin' || 
                          !!(referrer && referrer.includes('/admin/products'));
    
    if (isAdminContext) {
      // Navigate back to admin products page
      this.router.navigate(['/admin/products']);
    } else {
      // Navigate back to client products page
      this.router.navigate(['/Products']);
    }
  }

  private resolveImageUrl(url: string | null | undefined): string | null {
    if (!url) {
      return null;
    }

    if (/^https?:\/\//i.test(url)) {
      return url;
    }

    if (url.startsWith('/uploads/')) {
      return `${environment.apiUrl}${url}`;
    }

    return url;
  }

  ngOnInit(): void {
    // Check if we're in admin context
    const fromParam = this.route.snapshot.queryParams['from'];
    const referrer = document.referrer;
    this.isAdminContext = fromParam === 'admin' || 
                          !!(referrer && referrer.includes('/admin/products'));

    const idParam = this.route.snapshot.paramMap.get('id');
    const id = idParam ? Number(idParam) : NaN;

    if (!id || Number.isNaN(id)) {
      return;
    }

    this.isLoading = true;
    this.productService.getById(id).subscribe({
      next: (res) => {
        const p = res.data as IProduct | null;
        if (p) {
          this.backendProduct = p;
          this.applyBackendProduct(p);
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load product details', err);
        this.isLoading = false;
        this.loadError = true;
      },
    });
  }

  onStartCustomDesign(): void {
    if (!this.backendProduct) {
      return;
    }

    const hasBackendColors = !!(
      this.backendProduct &&
      this.backendProduct.colors &&
      this.backendProduct.colors.length
    );

    if (hasBackendColors && !this.hasUserSelectedColor) {
      Swal.fire({
        icon: 'warning',
        title: 'Choose a color',
        text: 'Please select a color before starting a custom design.',
      });
      return;
    }

    if (this.hasSizes && !this.selectedSize) {
      Swal.fire({
        icon: 'warning',
        title: 'Choose a size',
        text: 'Please select a size before starting a custom design.',
      });
      return;
    }

    const selectedColor = this.selectedBackendColor;

    if (hasBackendColors && !selectedColor) {
      Swal.fire({
        icon: 'error',
        title: 'Cannot start custom design',
        text: 'Please select a valid color before starting a custom design.',
      });
      return;
    }

    const studioContext: any = {
      productId: this.backendProduct.id,
      size: this.selectedSize || undefined,
      baseImageUrl: this.resolveImageUrl(this.imageUrl) ?? this.imageUrl,
    };

    if (selectedColor) {
      studioContext.colorId = selectedColor.id;
      studioContext.colorName = selectedColor.colorName;
      studioContext.colorHex = selectedColor.colorHex;
    }

    try {
      sessionStorage.setItem('studioContext', JSON.stringify(studioContext));
    } catch {
      // ignore
    }

    this.router.navigate(['/client/customize'], { state: { studioContext } });
  }

  get hasSizes(): boolean {
    return !!(this.product.availableSizes && this.product.availableSizes.length);
  }

  get selectedColor(): ProductColorVariant | undefined {
    return this.product.colors.find((c) => c.key === this.selectedColorKey) ?? this.product.colors[0];
  }

  get selectedCustomization(): CustomizationOption | undefined {
    return this.product.customizations.find((c) => c.id === this.selectedCustomizationId);
  }

  get imageUrl(): string {
    return this.selectedColor?.imageUrl ?? this.product.colors[0].imageUrl;
  }

  get finalPricePerItem(): number {
    const colorExtra = this.selectedColor?.extraPrice ?? 0;
    const customExtra = this.selectedCustomization?.extraPrice ?? 0;
    return this.product.basePrice + colorExtra + customExtra;
  }

  get totalPrice(): number {
    return this.finalPricePerItem * this.quantity;
  }

  get canUseAiCustomization(): boolean {
    return !!(this.backendProduct && this.backendProduct.allowAiCustomization);
  }

  get canAddToCart(): boolean {
    const hasBackendColors = !!(
      this.backendProduct &&
      this.backendProduct.colors &&
      this.backendProduct.colors.length
    );

    const colorOk = !hasBackendColors || this.hasUserSelectedColor;
    const sizeOk = !this.hasSizes || !!this.selectedSize;

    return this.product.inStock && colorOk && sizeOk;
  }

  get addToCartHint(): string | null {
    if (!this.product.inStock) {
      return null;
    }

    const hasBackendColors = !!(
      this.backendProduct &&
      this.backendProduct.colors &&
      this.backendProduct.colors.length
    );

    const needsColor = hasBackendColors && !this.hasUserSelectedColor;
    const needsSize = this.hasSizes && !this.selectedSize;

    if (needsColor && needsSize) {
      return 'Please choose a color and size to add this item to your cart.';
    }

    if (needsColor) {
      return 'Please choose a color to add this item to your cart.';
    }

    if (needsSize) {
      return 'Please choose a size to add this item to your cart.';
    }

    return null;
  }

  private applyBackendProduct(p: IProduct): void {
    const colors: ProductColorVariant[] = (p.colors ?? []).map((c) => ({
      key: String(c.id),
      label: c.colorName,
      swatch: c.colorHex ?? '#4A90E2',
      extraPrice: c.additionalPrice,
      imageUrl: this.resolveImageUrl(
        c.images?.find((i) => i.isMain)?.imageUrl ??
          c.images?.[0]?.imageUrl ??
          p.imageUrl ??
          this.product.colors[0]?.imageUrl ??
          '/assets/images/Hoodie.png'
      ) ?? '/assets/images/Hoodie.png',
    }));

    this.product = {
      ...this.product,
      name: p.name,
      basePrice: p.price,
      shortDescription: p.description ?? this.product.shortDescription,
      longDescription: p.description ?? this.product.longDescription,
      inStock: p.isActive,
      stockLabel: p.isActive
        ? 'In stock - Ships in 3-5 business days'
        : 'Currently unavailable',
      availableSizes: p.availableSizes
        ? p.availableSizes.split(',').map((s) => s.trim()).filter((s) => !!s)
        : this.product.availableSizes,
      colors,
      // keep customizations from the existing model
    };

    this.selectedColorKey = this.product.colors[0]?.key;
    this.selectedCustomizationId = this.product.customizations[0]?.id ?? null;
    this.hasUserSelectedColor = false;
    this.selectedSize = null;
  }

  private get selectedBackendColor(): IProductColor | undefined {
    if (!this.backendProduct || !this.selectedColorKey) {
      return undefined;
    }

    const id = Number(this.selectedColorKey);
    if (!id || Number.isNaN(id)) {
      return undefined;
    }

    return (this.backendProduct.colors ?? []).find((c) => c.id === id);
  }

  onColorSelected(key: string) {
    this.selectedColorKey = key;
    this.hasUserSelectedColor = true;
  }

  onCustomizationSelected(id: string | null) {
    this.selectedCustomizationId = id;
  }

  onQuantityChange(qty: number) {
    this.quantity = Math.max(1, qty);
  }

  onAddToCart() {
    const hasBackendColors = !!(
      this.backendProduct &&
      this.backendProduct.colors &&
      this.backendProduct.colors.length
    );

    if (hasBackendColors && !this.hasUserSelectedColor) {
      Swal.fire({
        icon: 'warning',
        title: 'Choose a color',
        text: 'Please select a color before adding this item to your cart.',
      });
      return;
    }

    if (this.hasSizes && !this.selectedSize) {
      Swal.fire({
        icon: 'warning',
        title: 'Choose a size',
        text: 'Please select a size before adding this item to your cart.',
      });
      return;
    }

    const selectedColor = this.selectedBackendColor;

    if (!selectedColor) {
      Swal.fire({
        icon: 'error',
        title: 'Cannot add to cart',
        text: 'Please select a valid color before adding this item to your cart.',
      });
      return;
    }

    const quantity = Math.max(1, this.quantity);
    this.isAddingToCart = true;

    this.cartService.addToCart(selectedColor.id, quantity, this.selectedSize || undefined).subscribe({
      next: () => {
        this.isAddingToCart = false;
        Swal.fire({
          toast: true,
          position: 'top-end',
          icon: 'success',
          title: 'Added to cart',
          text: `${quantity} × ${this.product.name}`,
          showConfirmButton: false,
          timer: 2000,
        });
      },
      error: (err) => {
        console.error('Failed to add product to cart from details page', err);
        this.isAddingToCart = false;
        Swal.fire({
          icon: 'error',
          title: 'Could not add to cart',
          text: 'Something went wrong while adding this item to your cart. Please try again.',
        });
      },
    });
  }

  onToggleWishlist() {
    this.wishlistActive = !this.wishlistActive;
  }
}
