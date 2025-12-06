import { Component, Input } from '@angular/core';
import { HeroSectionModel } from '../../Core/Interface/IHome';

@Component({
  selector: 'app-hero-section',
  imports: [],
  templateUrl: './hero-section.html',
  styleUrl: './hero-section.css',
})
export class HeroSection {
  @Input() hero: HeroSectionModel | null = null;
}
