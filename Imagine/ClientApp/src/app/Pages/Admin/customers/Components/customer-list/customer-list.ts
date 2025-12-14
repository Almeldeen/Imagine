import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CustomerItem } from '../customer-item/customer-item';
import { ICustomer } from '../../Core/Interface/ICustomer';
import { AdminCustomersService } from '../../Core/Service/admin-customers.service';
import { ToastService } from '../../../../../core/toast.service';
import { CustomerDetailsModal } from '../customer-details/customer-details';
import { CustomerEditModal } from '../customer-edit/customer-edit';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-customer-list',
  imports: [CommonModule, CustomerItem],
  templateUrl: './customer-list.html',
  styleUrls: ['./customer-list.css'],
})
export class CustomerList {
  private readonly modal = inject(NgbModal);
  private readonly service = inject(AdminCustomersService);
  private readonly toast = inject(ToastService);

  @Input() customers: ICustomer[] = [];
  @Input() viewMode: 'grid' | 'list' = 'grid';
  @Output() refresh = new EventEmitter<void>();

  onViewCustomer(customer: ICustomer): void {
    this.service.getById(customer.id).subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.toast.error(res.message || 'Failed to load customer details.');
          return;
        }

        const ref = this.modal.open(CustomerDetailsModal, { size: 'lg', backdrop: 'static' });
        ref.componentInstance.customer = res.data;
      },
      error: () => {
        this.toast.error('Failed to load customer details.');
      },
    });
  }

  onEditCustomer(customer: ICustomer): void {
    this.service.getById(customer.id).subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.toast.error(res.message || 'Failed to load customer for edit.');
          return;
        }

        const ref = this.modal.open(CustomerEditModal, { size: 'lg', backdrop: 'static' });
        ref.componentInstance.customer = res.data;

        ref.result.then(
          (result) => {
            if (result) {
              this.refresh.emit();
            }
          },
          () => {}
        );
      },
      error: () => {
        this.toast.error('Failed to load customer for edit.');
      },
    });
  }

  onResetPassword(customer: ICustomer): void {
    Swal.fire({
      icon: 'warning',
      title: 'Reset password?',
      text: `This will generate a new password for ${customer.fullName}.`,
      showCancelButton: true,
      confirmButtonText: 'Yes, reset',
      cancelButtonText: 'Cancel',
    }).then((result) => {
      if (!result.isConfirmed) {
        return;
      }

      this.service.resetPassword(customer.id).subscribe({
        next: (res) => {
          if (!res.success) {
            this.toast.error(res.message || 'Failed to reset password.');
            return;
          }

          this.toast.success(
            "Password reset successfully. The new password has been sent to the customer’s email."
          );
        },
        error: () => {
          this.toast.error('Failed to reset password.');
        },
      });
    });
  }
}
