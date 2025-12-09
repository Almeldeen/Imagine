import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TryOnService, TryOnJobStatus } from '../../../core/tryon.service';
import { ToastService } from '../../../core/toast.service';
import { ApiResponse } from '../../../core/IApiResponse';

type ProductType = 'tshirt' | 'hoodie';
type TryOnUiStatus = 'idle' | 'queued' | 'processing' | 'completed' | 'failed';

@Component({
  selector: 'app-client-customization',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './client-customization.view.html',
  styleUrl: './client-customization.css',
})
export class ClientCustomization {
  private readonly tryOnService = inject(TryOnService);
  private readonly toast = inject(ToastService);

  prompt = '';
  selectedType: ProductType = 'hoodie';
  isGenerating = false;
  hasPreview = false;

  // Try-on state
  garmentFile: File | null = null;
  personFile: File | null = null;
  garmentPreviewUrl: string | null = null;
  personPreviewUrl: string | null = null;
  preprocessedGarmentUrl: string | null = null;

  tryOnJobId: string | null = null;
  tryOnStatus: TryOnUiStatus = 'idle';
  tryOnImageUrl: string | null = null;
  tryOnError: string | null = null;

  isPreprocessing = false;
  isStartingTryOn = false;
  isPolling = false;
  private pollingStartTime: number | null = null;
  private pollTimeoutHandle: any = null;

  get selectedLabel(): string {
    return this.selectedType === 'hoodie' ? 'Hoodie' : 'T-shirt';
  }

  get isTryOnReady(): boolean {
    return !!this.personFile && !!this.garmentFile;
  }

  get displayStatusLabel(): string {
    switch (this.tryOnStatus) {
      case 'queued':
        return 'Queued';
      case 'processing':
        return 'Processing';
      case 'completed':
        return 'Done';
      case 'failed':
        return 'Failed';
      default:
        return 'Idle';
    }
  }

  onSelectType(type: ProductType) {
    if (this.selectedType === type) {
      return;
    }
    this.selectedType = type;
  }

  onGenerate() {
    this.runPreprocess();
  }

  onClear() {
    this.prompt = '';
    this.hasPreview = false;
  }

  onGarmentFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files && input.files[0];
    this.garmentFile = file ?? null;
    this.preprocessedGarmentUrl = null;

    if (this.garmentPreviewUrl) {
      URL.revokeObjectURL(this.garmentPreviewUrl);
    }

    this.garmentPreviewUrl = file ? URL.createObjectURL(file) : null;
  }

  onPersonFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files && input.files[0];
    this.personFile = file ?? null;

    if (this.personPreviewUrl) {
      URL.revokeObjectURL(this.personPreviewUrl);
    }

    this.personPreviewUrl = file ? URL.createObjectURL(file) : null;
  }

  onPreprocessGarment(): void {
    this.runPreprocess();
  }

  private runPreprocess(): void {
    const trimmedPrompt = this.prompt.trim();
    if (!trimmedPrompt || this.isPreprocessing || this.isGenerating) {
      return;
    }

    this.isPreprocessing = true;
    this.isGenerating = true;
    this.tryOnError = null;
    this.preprocessedGarmentUrl = null;
    this.hasPreview = false;

    this.tryOnService
      .preprocessGarment(trimmedPrompt, this.selectedType, this.garmentFile)
      .subscribe({
        next: (res: ApiResponse<{ preprocessedImageUrl: string }>) => {
          this.isPreprocessing = false;
          this.isGenerating = false;

          if (!res.success || !res.data) {
            this.toast.error(res.message || 'Failed to generate design.');
            return;
          }

          this.preprocessedGarmentUrl = res.data.preprocessedImageUrl;
          this.hasPreview = !!this.preprocessedGarmentUrl;
          this.toast.success('Design generated successfully.');
        },
        error: () => {
          this.isPreprocessing = false;
          this.isGenerating = false;
          this.toast.error('Failed to generate design.');
        },
      });
  }

  onStartTryOn(): void {
    if (!this.personFile || !this.garmentFile || this.isStartingTryOn || this.isPolling) {
      return;
    }

    this.isStartingTryOn = true;
    this.tryOnError = null;
    this.tryOnImageUrl = null;
    this.tryOnStatus = 'processing';

    this.tryOnService.startTryOn(this.personFile, this.garmentFile).subscribe({
      next: (res: ApiResponse<{ jobId: string; statusUrl?: string }>) => {
        this.isStartingTryOn = false;

        if (!res.success || !res.data || !res.data.jobId) {
          this.tryOnStatus = 'failed';
          this.tryOnError = res.message || 'Failed to start try-on.';
          this.toast.error(this.tryOnError);
          return;
        }

        this.tryOnJobId = res.data.jobId;
        this.tryOnStatus = 'processing';
        this.beginPolling();
      },
      error: () => {
        this.isStartingTryOn = false;
        this.tryOnStatus = 'failed';
        this.tryOnError = 'Failed to start try-on.';
        this.toast.error(this.tryOnError);
      },
    });
  }

  onCancelTryOn(): void {
    if (!this.isPolling) {
      return;
    }

    this.stopPolling();
    this.tryOnStatus = 'idle';
    this.tryOnJobId = null;
    this.tryOnError = null;
    this.toast.info('Try-on cancelled.');
  }

  private beginPolling(): void {
    if (!this.tryOnJobId) {
      return;
    }

    this.stopPolling();
    this.isPolling = true;
    this.pollingStartTime = Date.now();
    this.pollOnce();
  }

  private pollOnce(): void {
    if (!this.tryOnJobId || !this.pollingStartTime) {
      this.stopPolling();
      return;
    }

    const elapsedSeconds = (Date.now() - this.pollingStartTime) / 1000;
    if (elapsedSeconds > 120) {
      this.stopPolling();
      this.tryOnStatus = 'failed';
      this.tryOnError = 'Try-on is taking longer than expected. Please try again.';
      this.toast.error(this.tryOnError);
      return;
    }

    this.tryOnService.getTryOnStatus(this.tryOnJobId).subscribe({
      next: (res: ApiResponse<TryOnJobStatus>) => {
        if (!res.success || !res.data) {
          this.stopPolling();
          this.tryOnStatus = 'failed';
          this.tryOnError = res.message || 'Failed to get try-on status.';
          this.toast.error(this.tryOnError);
          return;
        }

        const data = res.data;
        const status = (data.status || '').toLowerCase();

        if (status === 'completed') {
          this.stopPolling();
          this.tryOnStatus = 'completed';

          if (data.imageUrl) {
            this.tryOnImageUrl = data.imageUrl;
          } else if (data.imageBase64) {
            this.tryOnImageUrl = `data:image/png;base64,${data.imageBase64}`;
          }

          if (!this.tryOnImageUrl) {
            this.tryOnError = 'The try-on finished but no image was returned.';
            this.toast.error(this.tryOnError);
          } else {
            this.toast.success('Your virtual try-on is ready.');
          }

          return;
        }

        if (status === 'failed') {
          this.stopPolling();
          this.tryOnStatus = 'failed';
          this.tryOnError = data.error || data.message || res.message || 'Try-on failed.';
          this.toast.error(this.tryOnError);
          return;
        }

        if (status === 'queued') {
          this.tryOnStatus = 'queued';
        } else {
          this.tryOnStatus = 'processing';
        }

        const delayMs = elapsedSeconds < 10 ? 2000 : 5000;
        this.pollTimeoutHandle = setTimeout(() => this.pollOnce(), delayMs);
      },
      error: () => {
        this.stopPolling();
        this.tryOnStatus = 'failed';
        this.tryOnError = 'Failed to get try-on status.';
        this.toast.error(this.tryOnError);
      },
    });
  }

  private stopPolling(): void {
    this.isPolling = false;
    if (this.pollTimeoutHandle) {
      clearTimeout(this.pollTimeoutHandle);
      this.pollTimeoutHandle = null;
    }
    this.pollingStartTime = null;
  }
}
