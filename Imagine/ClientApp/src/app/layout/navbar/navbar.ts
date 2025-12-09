import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from "@angular/router";
import { CommonModule } from '@angular/common';
import { map } from 'rxjs';
import { CartService } from '../../core/cart.service';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar implements OnInit {
  private readonly cartService = inject(CartService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly totalItems$ = this.cartService.cart$.pipe(
    map(cart => cart?.totalItems ?? 0)
  );

  isAuthenticated = false;

  ngOnInit(): void {
    this.cartService.refreshCart().subscribe();
    this.isAuthenticated = this.authService.isAuthenticated();
  }

  get profileImageUrl(): string {
    return this.authService.getProfileImageUrl();
  }

  get dashboardLink(): string {
    if (!this.isAuthenticated) {
      return '/Login';
    }

    if (this.authService.hasRole('Admin')) {
      return '/admin/Home';
    }

    return '/client/dashboard';
  }

  logout(): void {
    this.authService.clearSession();
    this.isAuthenticated = false;
    this.router.navigate(['/Home']);
  }

  onAvatarError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = '/assets/images/hero-banner.png';
  }
}
