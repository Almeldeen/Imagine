import { CategoryService } from '../../Core/Service/category.service';
import { ICategory } from '../../Core/Interface/ICategory';
import { Component, inject, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CategoryItem } from "../category-item/category-item";
import { CategoryForm } from '../category-form/category-form';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { environment } from '../../../../../../environments/environment';
import { ConfirmationModal } from '../../../../../shared/Components/confirmation-modal/confirmation-modal';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './category-list.html',
  styleUrl: './category-list.css',
})
export class CategoryList implements OnInit {


  @Input() categories: ICategory[] = [];
  @Input() viewMode: string = 'grid';
  @Output() refresh = new EventEmitter<void>();
  baseUrl=environment.apiUrl;
  
  constructor(private CategoryService: CategoryService , private modalService: NgbModal) {}

  ngOnInit() {
  }

  onAddCategory() {
  const ref = this.modalService.open(CategoryForm);

  ref.result.then((result) => {
    if (result) {
      this.refresh.emit();
    }
  });
}

deleteCategory(category: ICategory) {
  const modalRef = this.modalService.open(ConfirmationModal);
  modalRef.componentInstance.title = 'Delete Category';
  modalRef.componentInstance.message = `Are you sure you want to delete the category "${category.name}"?`;
  modalRef.componentInstance.confirmText = 'Delete';
  modalRef.componentInstance.confirmButtonClass = 'btn-danger';

  modalRef.result.then((result) => {
    if (result) {
      this.CategoryService.delete(category.id).subscribe({
        next: () => {
          this.refresh.emit();
        },
        error: (err: any) => console.error('Failed to delete category', err)
      });
    }
  }, () => {
    // Dismissed
  });
}
editCategory(category: ICategory) {
  const ref = this.modalService.open(CategoryForm);
  ref.componentInstance.category = category;

  ref.result.then((result) => {
    if (result) {
      this.refresh.emit();
    }
  });
}


}
