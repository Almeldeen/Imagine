import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SalesChart } from '../admin-home/Components/sales-chart/sales-chart';
import { ApiResponse } from '../../../core/IApiResponse';
import { AdminAnalyticsService, SalesOverview } from '../../../core/admin-analytics.service';

@Component({
  selector: 'app-admin-analytics',
  standalone: true,
  imports: [CommonModule, SalesChart],
  templateUrl: './admin-analytics.html',
  styleUrl: './admin-analytics.css',
})
export class AdminAnalytics implements OnInit {
  private readonly analyticsService = inject(AdminAnalyticsService);

  monthly: SalesOverview | null = null;
  yearly: SalesOverview | null = null;
  loading = false;
  error = '';

  ngOnInit(): void {
    this.loadData();
  }

  private loadData(): void {
    this.loading = true;
    this.error = '';

    this.analyticsService.getSalesOverview('month').subscribe({
      next: (res: ApiResponse<SalesOverview>) => {
        if (res.success && res.data) {
          this.monthly = res.data;
        } else {
          this.monthly = null;
        }
      },
      error: () => {
        this.monthly = null;
      },
    });

    this.analyticsService.getSalesOverview('year').subscribe({
      next: (res: ApiResponse<SalesOverview>) => {
        this.loading = false;
        if (res.success && res.data) {
          this.yearly = res.data;
        } else {
          this.yearly = null;
          this.error = res.message || '';
        }
      },
      error: () => {
        this.loading = false;
        this.yearly = null;
        this.error = 'Failed to load analytics data.';
      },
    });
  }
}
