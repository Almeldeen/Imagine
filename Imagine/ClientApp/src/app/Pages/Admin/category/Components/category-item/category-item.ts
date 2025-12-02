import { NgForOf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { CategoryService } from '../../Core/Service/category.service';
import { ICategory } from '../../Core/Interface/ICategory';
import { ApiResponse } from '../../../../../core/IApiResponse';

@Component({
  selector: 'app-category-item',
  imports: [NgForOf],
  templateUrl: './category-item.html',
  styleUrl: './category-item.css',
})
export class CategoryItem implements OnInit{
[x: string]: any;
  categories: ICategory[] = [];  
  constructor(private CategoryServiceService: CategoryService) {}

  ngOnInit() {
    this.loadCategories();
  }

  loadCategories() {
    this.CategoryServiceService.getAll().subscribe({
      next: (res: ApiResponse<ICategory[]>) => {
        this.categories = res.data;
      },
      error: (err: any) => console.error(err)
    });
  }

}
