import { Component, Input } from '@angular/core';
import { ICategory } from '../../../Admin/category/Core/Interface/ICategory';

@Component({
  selector: 'app-categories',
  imports: [],
  templateUrl: './categories.html',
  styleUrl: './categories.css',
})
export class Categories {
  @Input() categories: ICategory[] = [];
}
