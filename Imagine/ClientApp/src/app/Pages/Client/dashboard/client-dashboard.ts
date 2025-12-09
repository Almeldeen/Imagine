import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ProductCard } from '../../../shared/Components/product-card/product-card';
import { IProduct } from '../../Admin/products/Core/Interface/IProduct';
import { ProductService } from '../../Admin/products/Core/Service/product.service';
import { CartService } from '../../../core/cart.service';
import { OrderService, UserOrderSummary } from '../../../core/order.service';
import { AuthService } from '../../../core/auth.service';
import { ToastService } from '../../../core/toast.service';
import { ApiResponse } from '../../../core/IApiResponse';

type OrderStatus = 'Pending' | 'Processing' | 'Shipped' | 'Delivered' | 'Cancelled' | 'Refunded';

interface ClientOrder {
  id: number;
  orderNumber: string;
  createdAt: string;
  status: OrderStatus;
  total: number;
  itemsCount: number;
  thumbnailUrl: string;
}

@Component({
  selector: 'app-client-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, ProductCard],
  templateUrl: './client-dashboard.html',
  styleUrl: './client-dashboard.css',
})
export class ClientDashboard implements OnInit {
  fullName = 'Guest';
  recentOrders: ClientOrder[] = [];
  recommendedProducts: IProduct[] = [];
  loadingRecommendations = false;
  recommendationsError = '';

  stats = {
    totalOrders: 0,
    pendingOrders: 0,
    cartItems: 0,
    wishlistCount: 0,
  };

  profileImageUrl = '/assets/images/hero-banner.png';

  private readonly productService = inject(ProductService);
  private readonly cartService = inject(CartService);
  private readonly orderService = inject(OrderService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);

  ngOnInit(): void {
    if (typeof window !== 'undefined') {
      const storedName = window.localStorage.getItem('userFullName');
      if (storedName) {
        this.fullName = storedName;
      }
    }

    this.profileImageUrl = this.auth.getProfileImageUrl();

    this.initializeRecentOrders();
    this.loadRecommendedProducts();
    this.syncCartStats();
  }

  private initializeRecentOrders(): void {
    this.orderService.getMyOrders().subscribe({
      next: (res: ApiResponse<UserOrderSummary[]>) => {
        if (!res.success || !res.data) {
          this.recentOrders = [];
          this.stats.totalOrders = 0;
          this.stats.pendingOrders = 0;
          return;
        }

        const mapped = res.data.map<ClientOrder>((o) => ({
          id: o.id,
          orderNumber: o.orderNumber,
          createdAt: o.orderDate,
          status: (o.status as OrderStatus),
          total: o.totalAmount,
          itemsCount: o.itemsCount,
          thumbnailUrl: o.thumbnailUrl || '/assets/images/Hoodie.png',
        }));

        this.recentOrders = mapped.slice(0, 3);
        this.stats.totalOrders = res.data.length;
        this.stats.pendingOrders = res.data.filter(o => o.status === 'Pending' || o.status === 'Processing').length;
      },
      error: () => {
        this.recentOrders = [];
        this.stats.totalOrders = 0;
        this.stats.pendingOrders = 0;
        this.toast.error('Unable to load your recent orders.');
      },
    });
  }

  onRecentOrderClick(order: ClientOrder): void {
    this.router.navigate(['/client/orders'], {
      queryParams: { orderNumber: order.orderNumber },
    });
  }

  private loadRecommendedProducts(): void {
    this.loadingRecommendations = true;
    this.recommendationsError = '';
    this.productService.getFeaturedProducts(3).subscribe({
      next: response => {
        this.loadingRecommendations = false;
        this.recommendedProducts = response.data ?? [];
      },
      error: () => {
        this.loadingRecommendations = false;
        this.recommendationsError = 'Unable to load recommendations right now.';
      },
    });
  }

  private syncCartStats(): void {
    this.cartService.refreshCart().subscribe({
      next: response => {
        const totalItems = response.data?.totalItems ?? 0;
        this.stats.cartItems = totalItems;
      },
      error: () => {
        this.stats.cartItems = 0;
      },
    });
  }
}
