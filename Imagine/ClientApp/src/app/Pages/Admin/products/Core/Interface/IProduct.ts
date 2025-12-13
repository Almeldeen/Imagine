export interface IProductImage {
  id: number;
  productColorId: number;
  imageUrl: string;
  altText?: string;
  isMain: boolean;
  displayOrder: number;
}

export interface IProductColor {
  id: number;
  productId: number;
  colorName: string;
  colorHex?: string;
  stock: number;
  additionalPrice: number;
  isAvailable: boolean;
  images?: IProductImage[];
}

// Single interface used for both list and details views
export interface IProduct {
  id: number;
  categoryId?: number;
  categoryName?: string;
  name: string;
  description?: string;
  price: number;
  isActive: boolean;
  isFeatured?: boolean;
  isPopular?: boolean;
  isLatest?: boolean;
  viewCount?: number;
  imageUrl?: string;
  createdAt: string;
  updatedAt?: string;
  availableSizes?: string;
  allowAiCustomization?: boolean;
  colors?: IProductColor[];
}

// Create models used when creating a full product (product + colors + images) in one request
export interface CreateProductImageModel {
  fileKey: string;
  altText?: string;
  isMain: boolean;
  displayOrder: number;
  file?: File;
}

export interface CreateProductColorModel {
  colorName: string;
  colorHex?: string;
  stock: number;
  additionalPrice: number;
  isAvailable: boolean;
  images: CreateProductImageModel[];
}

export interface CreateProductRequestModel {
  categoryId: number;
  name: string;
  description?: string;
  price: number;
  isActive: boolean;
  isFeatured: boolean;
  isPopular: boolean;
  isLatest: boolean;
  allowAiCustomization: boolean;
  availableSizes?: string;
  colors: CreateProductColorModel[];
}
