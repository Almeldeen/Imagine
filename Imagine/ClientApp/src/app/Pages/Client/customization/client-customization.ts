import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

type ProductType = 'tshirt' | 'hoodie';

@Component({
  selector: 'app-client-customization',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './client-customization.view.html',
  styleUrl: './client-customization.css',
})
export class ClientCustomization {
  prompt = '';
  selectedType: ProductType = 'hoodie';
  isGenerating = false;
  hasPreview = false;

  get selectedLabel(): string {
    return this.selectedType === 'hoodie' ? 'Hoodie' : 'T-shirt';
  }

  onSelectType(type: ProductType) {
    if (this.selectedType === type) {
      return;
    }
    this.selectedType = type;
  }

  onGenerate() {
    if (this.isGenerating) {
      return;
    }

    this.isGenerating = true;

    // Frontend-only mock: simulate an AI generation delay.
    setTimeout(() => {
      this.hasPreview = true;
      this.isGenerating = false;
    }, 1200);
  }

  onClear() {
    this.prompt = '';
    this.hasPreview = false;
  }
}
