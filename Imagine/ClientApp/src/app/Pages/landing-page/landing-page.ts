import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HeroSection } from './Components/hero-section/hero-section';
import { Categories } from './Components/categories/categories';
import { HowItWorks } from './Components/how-it-works/how-it-works';
import { Trending } from './Components/trending/trending';
import { WhyChooseUs } from './Components/why-choose-us/why-choose-us';
import { Testimonials } from './Components/testimonials/testimonials';
import { CallToAction } from './Components/call-to-action/call-to-action';
import { FeaturedProducts } from './Components/featured-products/featured-products';
import { LatestProducts } from './Components/latest-products/latest-products';
import { HomeService } from './Core/Service/home.service';
import { HomeData } from './Core/Interface/IHome';

@Component({
  selector: 'app-landing-page',
  imports: [
    CommonModule,
    HeroSection,
    Categories,
    Trending,
    FeaturedProducts,
    LatestProducts,
    WhyChooseUs,
    HowItWorks,
    Testimonials,
    CallToAction,
  ],
  templateUrl: './landing-page.html',
  styleUrl: './landing-page.css',
})
export class LandingPage implements OnInit {
  private homeService = inject(HomeService);

  homeData: HomeData | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  ngOnInit(): void {
    this.loadHomeData();
  }

  private loadHomeData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.homeData = null;

    this.homeService.getHomeData().subscribe({
      next: (res) => {
        if (res?.success && res.data) {
          this.homeData = res.data;
        } else {
          this.errorMessage = res?.message || 'Failed to load home data.';
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err?.error?.message || 'Failed to load home data.';
      },
    });
  }
}
