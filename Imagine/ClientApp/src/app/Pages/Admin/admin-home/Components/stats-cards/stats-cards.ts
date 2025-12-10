import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiResponse } from '../../../../../core/IApiResponse';
import { AdminDashboardService, AdminDashboardStats } from '../../../../../core/admin-dashboard.service';

@Component({
  selector: 'app-stats-cards',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './stats-cards.html',
  styleUrl: './stats-cards.css',
})
export class StatsCards implements OnInit {
  private readonly dashboardService = inject(AdminDashboardService);

  stats: AdminDashboardStats | null = null;
  isLoading = false;
  placeholders = Array(4);

  ngOnInit(): void {
    this.loadStats();
  }

  private loadStats(): void {
    this.isLoading = true;

    this.dashboardService.getStats().subscribe({
      next: (res: ApiResponse<AdminDashboardStats>) => {
        this.isLoading = false;
        if (res.success && res.data) {
          this.stats = res.data;
        } else {
          this.stats = null;
        }
      },
      error: () => {
        this.isLoading = false;
        this.stats = null;
      },
    });
  }
}
