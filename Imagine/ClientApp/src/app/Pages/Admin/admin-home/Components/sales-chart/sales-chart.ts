import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminAnalyticsService, SalesOverview } from '../../../../../core/admin-analytics.service';
import { ApiResponse } from '../../../../../core/IApiResponse';

@Component({
  selector: 'app-sales-chart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sales-chart.html',
  styleUrl: './sales-chart.css',
})
export class SalesChart implements OnInit {
  private readonly analyticsService = inject(AdminAnalyticsService);

  activePeriod: 'month' | 'year' = 'month';
  overview: SalesOverview | null = null;
  loading = false;
  error = '';
  maxRevenue = 0;

  ngOnInit(): void {
    this.loadOverview('month');
  }

  get subtitle(): string {
    return this.activePeriod === 'month'
      ? 'Monthly sales performance (last 12 months)'
      : 'Yearly sales performance (last 5 years)';
  }

  onPeriodChange(period: 'month' | 'year'): void {
    if (this.activePeriod === period) {
      return;
    }
    this.loadOverview(period);
  }

  private loadOverview(period: 'month' | 'year'): void {
    this.activePeriod = period;
    this.loading = true;
    this.error = '';

    this.analyticsService.getSalesOverview(period).subscribe({
      next: (res: ApiResponse<SalesOverview>) => {
        this.loading = false;
        if (!res.success || !res.data) {
          this.overview = null;
          this.maxRevenue = 0;
          this.error = res.message || 'Failed to load sales overview.';
          return;
        }

        this.overview = res.data;
        this.maxRevenue = res.data.points.reduce(
          (max, p) => (p.totalRevenue > max ? p.totalRevenue : max),
          0,
        );
      },
      error: () => {
        this.loading = false;
        this.overview = null;
        this.maxRevenue = 0;
        this.error = 'Failed to load sales overview.';
      },
    });
  }
}
