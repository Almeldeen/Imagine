import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbAccordionDirective, NgbAccordionItem } from '@ng-bootstrap/ng-bootstrap';
import { AboutSectionHeader } from './Components/about-section-header/about-section-header';
import { AboutTextBlock } from './Components/about-text-block/about-text-block';
import { AboutTeamCard } from './Components/about-team-card/about-team-card';

interface TeamMember {
  name: string;
  role: string;
  avatar: string;
  bio: string;
}

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, NgbAccordionDirective, AboutSectionHeader, AboutTextBlock, AboutTeamCard],
  templateUrl: './about.html',
  styleUrl: './about.css',
})
export class About {
  mission = 'Empower creators to transform ideas into premium, AI-enhanced products in just a few clicks.';
  vision = 'Make custom apparel and merchandise as intuitive and inspiring as working with your favorite design tool.';

  values = [
    {
      title: 'Innovation First',
      text: 'We blend AI with craftsmanship to unlock new creative possibilities for every user.'
    },
    {
      title: 'Quality & Detail',
      text: 'From digital mockup to final fabric, we obsess over detail, color accuracy, and comfort.'
    },
    {
      title: 'Creators at the Center',
      text: 'We design every interaction to feel fast, clear, and empowering for designers and non-designers alike.'
    }
  ];

  team: TeamMember[] = [
    {
      name: 'Alex Carter',
      role: 'Product Vision & AI Experience',
      avatar: '/assets/images/team-1.png',
      bio: 'Leads the Imagine experience, turning complex AI into smooth, playful flows for everyday creators.'
    },
    {
      name: 'Sara Mitchell',
      role: 'Design Systems & Branding',
      avatar: '/assets/images/team-2.png',
      bio: 'Shapes the neon-dark visual language, ensuring every screen feels cohesive and premium.'
    },
    {
      name: 'Omar Hassan',
      role: 'Engineering & Performance',
      avatar: '/assets/images/team-3.png',
      bio: 'Builds the infrastructure that keeps previews fast, responsive, and production-ready.'
    }
  ];
}
