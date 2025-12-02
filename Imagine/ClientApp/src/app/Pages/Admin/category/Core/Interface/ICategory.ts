export interface ICategory {
  id: number;
  name: string;
  description: string;
  imageUrl: string;
  isActive: boolean;
  displayOrder: number;
  productCount: number;
  createdAt: string;
  salesCount?: number;
}
