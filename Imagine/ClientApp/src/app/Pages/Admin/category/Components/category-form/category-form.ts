import { Component, inject, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule, NgModel } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import Swal from 'sweetalert2';
import { CategoryService } from '../../Core/Service/category.service';
import { CategoryList } from '../category-list/category-list';

@Component({
  selector: 'app-category-form',
  imports: [CommonModule, FormsModule],
  templateUrl: './category-form.html',
  styleUrl: './category-form.css',
})
export class CategoryForm implements OnInit {
  @Input() category: any;
  
  constructor(private CategoryService:CategoryService , public activeModal: NgbActiveModal){
  }
 
categoryobj:any ={
  
    "name": "",
    "description": "",
    "imagePath": "",
    "isActive": true,
    "displayOrder": 0,
    "productCount": 0
  
}

  ngOnInit() {
    if (this.category) {
      this.categoryobj = { ...this.category };
      // If there is an existing image path, you might want to set it for preview or handle it appropriately
      // this.imagePreviewUrl = this.category.imagePath; 
    }
  }


  // Image upload properties
  selectedFile: File | null = null;
  imagePreviewUrl: string | null = null;

  /** =============================
   *  IMAGE HANDLING
   *  ============================== */

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      const file = input.files[0];

      if (file.type.startsWith('image/')) {
        this.selectedFile = file;

        const reader = new FileReader();
        reader.onload = (e) => (this.imagePreviewUrl = e.target?.result as string);
        reader.readAsDataURL(file);
      } else {
        this.clearImage();
        Swal.fire({
          icon: 'warning',
          title: 'Invalid image',
          text: 'Please select a valid image file (PNG, JPG, JPEG)',
        });
      }
    }
  }

  clearImage(event?: Event) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }

    this.selectedFile = null;
    this.imagePreviewUrl = null;

    const input = document.querySelector('.file-input') as HTMLInputElement;
    if (input) input.value = '';
  }

  onUploadAreaClick() {
    const fileInput = document.querySelector('.file-input') as HTMLInputElement;
    if (fileInput) fileInput.click();
  }



  onCancel() {
    this.activeModal.dismiss('cancel');
  }

  onSave(event: Event) {
    event.preventDefault();
    const form = new FormData();
    form.append('Name', this.categoryobj.name);
    form.append('Description', this.categoryobj.description || '');
    form.append('IsActive', String(this.categoryobj.isActive));
    form.append('DisplayOrder', String(this.categoryobj.displayOrder));

    if (this.selectedFile) {
      form.append('ImageFile', this.selectedFile);
    }

    if (this.category && this.category.id) {
      this.CategoryService.update(this.category.id, form).subscribe({
        next: (res) => {
          Swal.fire({
            icon: 'success',
            title: 'Category updated',
            text: res.message || 'Category updated successfully',
          }).then(() => {
            this.activeModal.close(res.success);
          });
        },
        error: (err) => {
          Swal.fire({
            icon: 'error',
            title: 'Update failed',
            text: 'Error: ' + (err?.error?.message || JSON.stringify(err)),
          });
        }
      });
    } else {
      this.CategoryService.create(form).subscribe({
        next: (res) => {
          Swal.fire({
            icon: 'success',
            title: 'Category created',
            text: res.message || 'Category created successfully',
          }).then(() => {
            this.activeModal.close(res.success);
          });
        },
        error: (err) => {
          Swal.fire({
            icon: 'error',
            title: 'Create failed',
            text: 'Error: ' + (err?.error?.message || JSON.stringify(err)),
          });
        }
      });
    }
  }

}



