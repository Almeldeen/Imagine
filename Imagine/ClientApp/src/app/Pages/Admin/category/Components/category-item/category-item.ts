import { NgForOf } from '@angular/common';
import { CategoryService } from './../Category-service/CategoryService.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-category-item',
  imports: [NgForOf],
  templateUrl: './category-item.html',
  styleUrl: './category-item.css',
})
export class CategoryItem implements OnInit{
[x: string]: any;
  categories: any[] = [];  
  constructor(private CategoryServiceService: CategoryService) {}

  ngOnInit() {
    this.loadCategories();
  }

  loadCategories() {
    this.CategoryServiceService.getAll().subscribe({
      next: (res) => {
        this.categories = res.data;
      },
      error: (err) => console.error(err)
    });
  }

}
