import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TryOnService, TryOnJobStatus } from '../../../core/tryon.service';
import { CustomProductService, SaveCustomizationRequest, SavedCustomProduct } from '../../../core/custom-product.service';
import { ToastService } from '../../../core/toast.service';
import { ApiResponse } from '../../../core/IApiResponse';
import { CartService } from '../../../core/cart.service';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from '../../../../environments/environment';

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
  private readonly customProductService = inject(CustomProductService);
  private readonly cartService = inject(CartService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

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

  customizationJobId: number | null = null;

  tryOnJobId: string | null = null;
  tryOnStatus: TryOnUiStatus = 'idle';
  tryOnImageUrl: string | null = null;
  tryOnError: string | null = null;

  isPreprocessing = false;
  isStartingTryOn = false;
  isPolling = false;
  isSavingCustomization = false;
  private pollingStartTime: number | null = null;
  private pollTimeoutHandle: any = null;
  savedCustomProductId: number | null = null;

  // Optional context when coming from a specific product details page
  originProductId: number | null = null;
  originColorName: string | null = null;
  originColorHex: string | null = null;
  originSize: string | null = null;
  originBaseImageUrl: string | null = null;

  private resolveImageUrl(url: string | null | undefined): string | null {
    if (!url) {
      return null;
    }

    if (/^https?:\/\//i.test(url)) {
      return url;
    }

    if (url.startsWith('/uploads/')) {
      return `${environment.apiUrl}${url}`;
    }

    return url;
  }

  get selectedLabel(): string {
    return this.selectedType === 'hoodie' ? 'Hoodie' : 'T-shirt';
  }

  get isTryOnReady(): boolean {
    return !!this.personFile && this.customizationJobId !== null;
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

  ngOnInit(): void {
    const fromState = (history.state as any)?.studioContext;
    let ctx: any = fromState;

    if (!ctx) {
      try {
        const raw = sessionStorage.getItem('studioContext');
        ctx = raw ? JSON.parse(raw) : null;
      } catch {
        ctx = null;
      }
    }

    if (!ctx) {
      const qp = this.route.snapshot.queryParamMap;
      ctx = {
        productId: qp.get('productId'),
        colorName: qp.get('colorName'),
        colorHex: qp.get('colorHex'),
        size: qp.get('size'),
        baseImageUrl: qp.get('baseImageUrl'),
      };
    }

    const productIdVal = ctx?.productId;
    const parsedProductId = Number(productIdVal);
    if (!Number.isNaN(parsedProductId) && parsedProductId > 0) {
      this.originProductId = parsedProductId;
    }

    this.originColorName = ctx?.colorName ?? null;
    this.originColorHex = ctx?.colorHex ?? null;
    this.originSize = ctx?.size ?? null;

    const baseImageUrl = ctx?.baseImageUrl;
    if (baseImageUrl) {
      this.originBaseImageUrl = this.resolveImageUrl(baseImageUrl);
      if (!this.garmentPreviewUrl) {
        this.garmentPreviewUrl = this.originBaseImageUrl;
      }
    }
  }

  applyPresetPrompt(preset: 'comic' | 'wolf' | 'monogram'): void {
  switch (preset) {

    case 'comic':
      this.prompt =
        'spider-man illustration, centered, transparent background, print-ready PNG, ' +
        'no clothing, no model, no mockup, no background';
      break;

    case 'wolf':
      this.prompt =
        'wolf illustration, centered, transparent background, print-ready PNG, ' +
        'no clothing, no model, no mockup, no background';
      break;

    case 'monogram':
      this.prompt =
        "IMAGINE text logo, centered, transparent background, print-ready PNG, " +
        'no clothing, no model, no mockup, no background';
      break;
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

    const colorConstraint = this.originColorName
      ? ` Keep the garment color ${this.originColorName} and do not change the base garment color or fabric shade.`
      : '';

    const effectivePrompt = `${trimmedPrompt}${colorConstraint}`.trim();
    this.isPreprocessing = true;
    this.isGenerating = true;
    this.tryOnError = null;
    this.preprocessedGarmentUrl = null;
    this.customizationJobId = null;
    this.hasPreview = false;

    const startPreprocessWithFile = (file: File | null) => {
      this.tryOnService
        .preprocessGarment(effectivePrompt, this.selectedType, file, this.originBaseImageUrl ?? undefined)
        .subscribe({
          next: (res: ApiResponse<{ preprocessedImageUrl: string; customizationJobId?: number }>) => {
            this.isPreprocessing = false;
            this.isGenerating = false;

            if (!res.success || !res.data) {
              this.toast.error(res.message || 'Failed to generate design.');
              return;
            }

            this.preprocessedGarmentUrl = res.data.preprocessedImageUrl;
            if (typeof res.data.customizationJobId === 'number') {
              this.customizationJobId = res.data.customizationJobId;
            }
            this.hasPreview = !!this.preprocessedGarmentUrl;
            this.toast.success('Design generated successfully.');
          },
          error: () => {
            this.isPreprocessing = false;
            this.isGenerating = false;
            this.toast.error('Failed to generate design.');
          },
        });
    };

    if (!this.garmentFile && this.originBaseImageUrl) {
      fetch(this.originBaseImageUrl)
        .then((response) => {
          if (!response.ok) {
            throw new Error('Failed to load base garment image.');
          }
          return response.blob();
        })
        .then((blob) => {
          const fileName = (this.originColorName || 'product-mockup') + '.png';
          this.garmentFile = new File([blob], fileName, { type: blob.type || 'image/png' });
          startPreprocessWithFile(this.garmentFile);
        })
        .catch(() => {
          startPreprocessWithFile(this.garmentFile);
        });
      return;
    }

    startPreprocessWithFile(this.garmentFile);
  }

  onStartTryOn(): void {
    if (!this.personFile || this.customizationJobId === null || this.isStartingTryOn || this.isPolling) {
      return;
    }

    this.isStartingTryOn = true;
    this.tryOnError = null;
    this.tryOnImageUrl = null;
    this.tryOnStatus = 'processing';

    this.tryOnService.startTryOn(this.personFile, this.customizationJobId).subscribe({
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

  onSaveCustomization(): void {
    if (this.customizationJobId === null) {
      this.toast.error('Generate a design first before saving.');
      return;
    }

    if (this.isSavingCustomization) {
      return;
    }

    this.isSavingCustomization = true;

    const payload: SaveCustomizationRequest = {
      customizationJobId: this.customizationJobId,
      productId: this.originProductId ?? undefined,
      colorName: this.originColorName ?? undefined,
      colorHex: this.originColorHex ?? undefined,
      size: this.originSize ?? undefined,
    };

    this.customProductService.saveFromCustomization(payload).subscribe({
      next: (res: ApiResponse<SavedCustomProduct>) => {
        this.isSavingCustomization = false;

        if (!res.success || !res.data) {
          this.toast.error(res.message || 'Failed to save customization.');
          return;
        }

        this.savedCustomProductId = res.data.id;
        this.toast.success('Customization saved to your custom products.');
      },
      error: () => {
        this.isSavingCustomization = false;
        this.toast.error('Failed to save customization.');
      },
    });
  }

  onAddDesignToCartAndCheckout(): void {
    if (this.customizationJobId === null) {
      this.toast.error('Generate a design first before ordering.');
      return;
    }

    if (this.isSavingCustomization) {
      return;
    }

    this.isSavingCustomization = true;

    const payload: SaveCustomizationRequest = {
      customizationJobId: this.customizationJobId,
      productId: this.originProductId ?? undefined,
      colorName: this.originColorName ?? undefined,
      colorHex: this.originColorHex ?? undefined,
      size: this.originSize ?? undefined,
    };

    this.customProductService.saveFromCustomization(payload).subscribe({
      next: (res: ApiResponse<SavedCustomProduct>) => {
        if (!res.success || !res.data) {
          this.isSavingCustomization = false;
          this.toast.error(res.message || 'Failed to save customization before ordering.');
          return;
        }

        const customProductId = res.data.id;
        this.savedCustomProductId = customProductId;

        this.cartService.addCustomProductToCart(customProductId, 1, this.originSize ?? undefined).subscribe({
          next: (cartRes) => {
            this.isSavingCustomization = false;

            if (!cartRes.success) {
              this.toast.error(cartRes.message || 'Failed to add design to cart.');
              return;
            }

            this.toast.success('Your custom design was added to the cart. You can now complete your order.');
            this.router.navigate(['/Checkout']);
          },
          error: () => {
            this.isSavingCustomization = false;
            this.toast.error('Failed to add design to cart.');
          },
        });
      },
      error: () => {
        this.isSavingCustomization = false;
        this.toast.error('Failed to save customization before ordering.');
      },
    });
  }
}
