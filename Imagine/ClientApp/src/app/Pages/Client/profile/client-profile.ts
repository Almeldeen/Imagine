import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/auth.service';
import { ToastService } from '../../../core/toast.service';

@Component({
  selector: 'app-client-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './client-profile.html',
  styleUrl: './client-profile.css',
})
export class ClientProfile {
  fullName = 'Guest';
  profileImageUrl = '/assets/images/hero-banner.png';
  isUploading = false;

  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);

  constructor() {
    const name = this.auth.getFullName();
    if (name) {
      this.fullName = name;
    }

    this.profileImageUrl = this.auth.getProfileImageUrl();
  }

  onAvatarError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = '/assets/images/hero-banner.png';
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      return;
    }

    const file = input.files[0];

    if (!file.type.startsWith('image/')) {
      this.toast.error('Please choose a valid image file.');
      input.value = '';
      return;
    }

    this.isUploading = true;

    this.auth.updateProfileImage(file).subscribe({
      next: (res) => {
        this.isUploading = false;

        if (!res.success) {
          this.toast.error(res.message || 'Failed to update profile image.');
          return;
        }

        this.profileImageUrl = this.auth.getProfileImageUrl();
        this.toast.success('Profile image updated successfully.');
        input.value = '';
      },
      error: () => {
        this.isUploading = false;
        this.toast.error('Failed to update profile image.');
      },
    });
  }
}
