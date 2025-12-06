import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-client-order-confirmation',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './client-order-confirmation.html',
  styleUrls: ['./client-order-confirmation.css'],
})
export class ClientOrderConfirmation implements OnInit {
  orderId?: number;
  orderNumber?: string | null;

  constructor(private readonly route: ActivatedRoute, private readonly router: Router) {}

  ngOnInit(): void {
    this.orderId = Number(this.route.snapshot.paramMap.get('id')) || undefined;
    this.orderNumber = this.route.snapshot.queryParamMap.get('orderNumber');
  }

  goToOrders(): void {
    this.router.navigate(['/client/orders']);
  }
}
