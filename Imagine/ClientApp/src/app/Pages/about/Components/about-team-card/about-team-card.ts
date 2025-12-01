import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

interface TeamMember {
  name: string;
  role: string;
  avatar: string;
  bio: string;
}

@Component({
  selector: 'app-about-team-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './about-team-card.html',
  styleUrl: './about-team-card.css',
})
export class AboutTeamCard {
  @Input() member!: TeamMember;
}
