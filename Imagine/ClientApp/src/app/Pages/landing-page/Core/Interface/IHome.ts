import { IProduct } from '../../../Admin/products/Core/Interface/IProduct';
import { ICategory } from '../../../Admin/category/Core/Interface/ICategory';

export interface HeroSectionModel {
  titleLine1: string;
  titleLine2: string;
  subtitle: string;
  badgeText: string;
  primaryCtaText: string;
  secondaryCtaText: string;
  bannerImageUrl: string;
}

export interface HomeData {
  hero: HeroSectionModel;
  featuredProducts: IProduct[];
  latestProducts: IProduct[];
  popularProducts: IProduct[];
  featuredCategories: ICategory[];
}
