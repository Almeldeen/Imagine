import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-trending',
  imports: [],
  templateUrl: './trending.html',
  styleUrl: './trending.css',
})
export class Trending implements OnInit {

  ngOnInit(): void {
    this.initializeColorSwitcher();
  }

  // Initialize color switcher for product cards
  private initializeColorSwitcher(): void {
    // Wait for DOM to be ready
    setTimeout(() => {
      const colorSwatches = document.querySelectorAll('.color-swatch');
      
      colorSwatches.forEach(swatch => {
        swatch.addEventListener('click', (e) => {
          const button = e.currentTarget as HTMLElement;
          const colorValue = button.getAttribute('data-color');
          const productCard = button.closest('.product-card');
          
          if (productCard && colorValue) {
            // Remove active class from all swatches in this card
            const allSwatches = productCard.querySelectorAll('.color-swatch');
            allSwatches.forEach(s => s.classList.remove('active'));
            
            // Add active class to clicked swatch
            button.classList.add('active');
            
            // Switch product images
            const allImages = productCard.querySelectorAll('.product-image');
            allImages.forEach(img => {
              const imgColorValue = img.getAttribute('data-color');
              if (imgColorValue === colorValue) {
                img.classList.add('active');
              } else {
                img.classList.remove('active');
              }
            });
          }
        });
      });
    }, 100);
  }
}
