import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-cart-empty-state',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './cart-empty.html',
  styleUrl: './cart-empty.css',
})
export class CartEmptyState {}
