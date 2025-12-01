import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductImageGallery } from './components/product-image-gallery/product-image-gallery';
import { ProductColorSelector } from './components/product-color-selector/product-color-selector';
import { ProductCustomizationOptions } from './components/product-customization-options/product-customization-options';
import { ProductInfo } from './components/product-info/product-info';
import { AddToCartButton } from './components/add-to-cart-button/add-to-cart-button';

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
export class ProductDetails {
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

  onColorSelected(key: string) {
    this.selectedColorKey = key;
  }

  onCustomizationSelected(id: string | null) {
    this.selectedCustomizationId = id;
  }

  onQuantityChange(qty: number) {
    this.quantity = Math.max(1, qty);
  }

  onAddToCart() {
    // Placeholder for future cart integration
  }

  onToggleWishlist() {
    this.wishlistActive = !this.wishlistActive;
  }
}
