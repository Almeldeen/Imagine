import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AdminDashboardService, AdminDashboardStats } from '../../../core/admin-dashboard.service';
import { ApiResponse } from '../../../core/IApiResponse';

@Component({
  selector: 'app-admin-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './admin-sidebar.html',
  styleUrl: './admin-sidebar.css',
})
export class AdminSidebar implements OnInit {
  private readonly dashboardService = inject(AdminDashboardService);

  stats: AdminDashboardStats | null = null;

  ngOnInit(): void {
    this.loadStats();
  }

  private loadStats(): void {
    this.dashboardService.getStats().subscribe({
      next: (res: ApiResponse<AdminDashboardStats>) => {
        if (res.success && res.data) {
          this.stats = res.data;
        } else {
          this.stats = null;
        }
      },
      error: () => {
        this.stats = null;
      },
    });
  }
}
