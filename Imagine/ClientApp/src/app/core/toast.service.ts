import { Injectable } from '@angular/core';
import Swal, { SweetAlertIcon } from 'sweetalert2';

@Injectable({ providedIn: 'root' })
export class ToastService {
  private show(icon: SweetAlertIcon, message: string, title?: string): void {
    Swal.fire({
      toast: true,
      position: 'top-end',
      icon,
      title: title ?? message,
      text: title ? message : undefined,
      showConfirmButton: false,
      timer: 3000,
      timerProgressBar: true,
      customClass: {
        popup: 'imagine-toast-popup',
      },
    });
  }

  success(message: string, title = 'Success'): void {
    this.show('success', message, title);
  }

  error(message: string, title = 'Something went wrong'): void {
    this.show('error', message, title);
  }

  info(message: string, title = 'Notice'): void {
    this.show('info', message, title);
  }
}
