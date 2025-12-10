import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ICategory } from '../../../Admin/category/Core/Interface/ICategory';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './categories.html',
  styleUrl: './categories.css',
})
export class Categories {
  @Input() categories: ICategory[] = [];
}
