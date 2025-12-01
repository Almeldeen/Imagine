import { Component, inject, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule, NgModel } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { CategoryService } from '../Category-service/CategoryService.service';
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
        alert('Please select a valid image file (PNG, JPG, JPEG)');
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
    const formData = new FormData();

    formData.append("name", this.categoryobj.name);
    formData.append("description", this.categoryobj.description);
    formData.append("isActive", this.categoryobj.isActive);
    formData.append("displayOrder", this.categoryobj.displayOrder);
    formData.append("ProductCount", this.categoryobj.productCount);

    console.log('Sending Category Data (FormData):', {
      name: this.categoryobj.name,
      ProductCount: this.categoryobj.productCount,
      displayOrder: this.categoryobj.displayOrder,
      imagePath: this.categoryobj.imagePath,
      isActive: this.categoryobj.isActive,
      description: this.categoryobj.description
    });

    if (this.selectedFile) {
      formData.append("ImageFile", this.selectedFile, this.selectedFile.name);
    }

    if (this.category && this.category.id) {
      if (this.selectedFile) {
        // Image update: send FormData
        this.CategoryService.update(this.category.id, formData).subscribe({
          next: (res) => {
            alert("Category updated successfully");
            this.activeModal.close(res); 
          },
          error: (err) => {
            alert("Error: " + JSON.stringify(err));
          }
        });
      } else {
        // Text-only update: send JSON
        const updateData = {
          id: this.category.id,
          name: this.categoryobj.name,
          description: this.categoryobj.description,
          isActive: this.categoryobj.isActive,
          displayOrder: this.categoryobj.displayOrder,
          productCount: this.categoryobj.productCount, 
          imagePath: this.category.imagePath 
        };

        this.CategoryService.update(this.category.id, updateData).subscribe({
          next: (res) => {
            alert("Category updated successfully");
            this.activeModal.close(res); 
          },
          error: (err) => {
            alert("Error: " + JSON.stringify(err));
          }
        });
      }
    } else {
        if (this.selectedFile) {
          this.CategoryService.create(formData).subscribe({
            next: (res) => {
              alert("Category created successfully");
              this.activeModal.close(res); 
            },
            error: (err) => {
              alert("Error: " + JSON.stringify(err));
            }
          });
        } else {
          const createData = {
            name: this.categoryobj.name,
            description: this.categoryobj.description,
            isActive: this.categoryobj.isActive,
            ProductCount: this.categoryobj.productCount, // PascalCase for Create
            displayOrder: this.categoryobj.displayOrder
          };
          
          console.log('Sending Category Data (JSON):', createData);

          this.CategoryService.create(createData).subscribe({
            next: (res) => {
              alert("Category created successfully");
              this.activeModal.close(res); 
            },
            error: (err) => {
              alert("Error: " + JSON.stringify(err));
            }
          });
        }
    }
  }



}



