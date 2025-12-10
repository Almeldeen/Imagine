import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HeroSectionModel } from '../../Core/Interface/IHome';

@Component({
  selector: 'app-hero-section',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './hero-section.html',
  styleUrls: ['./hero-section.css'],
})
export class HeroSection {
  @Input() hero: HeroSectionModel | null = null;
}
