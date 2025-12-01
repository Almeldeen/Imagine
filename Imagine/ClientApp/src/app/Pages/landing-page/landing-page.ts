import { Component } from '@angular/core';
import { HeroSection } from './Components/hero-section/hero-section';
import { Categories } from './Components/categories/categories';
import { HowItWorks } from './Components/how-it-works/how-it-works';
import { Trending } from './Components/trending/trending';
import { WhyChooseUs } from './Components/why-choose-us/why-choose-us';
import { Testimonials } from './Components/testimonials/testimonials';
import { CallToAction } from './Components/call-to-action/call-to-action';

@Component({
  selector: 'app-landing-page',
  imports: [HeroSection, Categories, HowItWorks, Trending, WhyChooseUs, Testimonials, CallToAction],
  templateUrl: './landing-page.html',
  styleUrl: './landing-page.css',
})
export class LandingPage {

}
