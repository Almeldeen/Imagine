import { Component, HostListener, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth.service';

@Component({
  selector: 'app-admin-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './admin-navbar.html',
  styleUrl: './admin-navbar.css',
})
export class AdminNavbar implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  isProfileMenuOpen = false;

  ngOnInit(): void {
    // Ensure admin-only area still checks authentication status if needed later
  }

  get profileImageUrl(): string {
    return this.authService.getProfileImageUrl();
  }

  get fullName(): string {
    return this.authService.getFullName() ?? 'Admin User';
  }

  toggleProfileMenu(event: MouseEvent): void {
    event.stopPropagation();
    this.isProfileMenuOpen = !this.isProfileMenuOpen;
  }

  logout(): void {
    this.authService.clearSession();
    this.isProfileMenuOpen = false;
    this.router.navigate(['/Home']);
  }

  @HostListener('document:click')
  onDocumentClick(): void {
    if (this.isProfileMenuOpen) {
      this.isProfileMenuOpen = false;
    }
  }
}
