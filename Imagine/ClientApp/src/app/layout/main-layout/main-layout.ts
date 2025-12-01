import { Component, OnInit, HostListener } from '@angular/core';
import { LandingPage } from "../../Pages/landing-page/landing-page";
import { Navbar } from "../navbar/navbar";
import { Footer } from "../footer/footer";
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-main-layout',
  imports: [Navbar, Footer, RouterOutlet],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css',
})
export class MainLayout implements OnInit {
  
  ngOnInit(): void {
    // Initialize any startup animations or effects
    this.initializeAnimations();
    // Initialize color switcher for products
    this.initializeColorSwitcher();
    // Initialize scroll animations
    this.initializeScrollAnimations();
    // Initialize back to top button
    this.initializeBackToTop();
  }

  // Handle scroll events for navbar
  @HostListener('window:scroll', [])
  onWindowScroll(): void {
    const navbar = document.getElementById('mainNavbar');
    if (navbar) {
      if (window.pageYOffset > 50) {
        navbar.classList.add('scrolled');
      } else {
        navbar.classList.remove('scrolled');
      }
    }
  }

  // Initialize animations
  private initializeAnimations(): void {
    // Add smooth scroll behavior for navigation links
    const navLinks = document.querySelectorAll('.nav-link');
    navLinks.forEach(link => {
      link.addEventListener('click', (e) => {
        const href = link.getAttribute('href');
        if (href && href.startsWith('#')) {
          e.preventDefault();
          const target = document.querySelector(href);
          if (target) {
            target.scrollIntoView({ behavior: 'smooth', block: 'start' });
          }
        }
      });
    });
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

  // Initialize scroll-triggered animations
  private initializeScrollAnimations(): void {
    const observerOptions = {
      threshold: 0.1,
      rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          entry.target.classList.add('aos-animate');
        }
      });
    }, observerOptions);

    // Observe all elements with data-aos attribute
    setTimeout(() => {
      const animatedElements = document.querySelectorAll('[data-aos]');
      animatedElements.forEach(el => observer.observe(el));
    }, 100);
  }

  // Initialize back to top button
  private initializeBackToTop(): void {
    setTimeout(() => {
      const backToTopButton = document.getElementById('backToTop');
      
      if (backToTopButton) {
        // Show/hide button based on scroll position
        window.addEventListener('scroll', () => {
          if (window.pageYOffset > 300) {
            backToTopButton.classList.add('visible');
          } else {
            backToTopButton.classList.remove('visible');
          }
        });

        // Scroll to top when clicked
        backToTopButton.addEventListener('click', () => {
          window.scrollTo({
            top: 0,
            behavior: 'smooth'
          });
        });
      }
    }, 100);
  }
}
