import { Component } from '@angular/core';
import { StatsCards } from './Components/stats-cards/stats-cards';
import { SalesChart } from './Components/sales-chart/sales-chart';
import { TopProducts } from './Components/top-products/top-products';
import { RecentOrders } from './Components/recent-orders/recent-orders';

@Component({
  selector: 'app-admin-home',
  imports: [StatsCards, SalesChart, TopProducts, RecentOrders],
  templateUrl: './admin-home.html',
  styleUrl: './admin-home.css',
})
export class AdminHome {

}
