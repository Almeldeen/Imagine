import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ICustomerDetails } from '../../Core/Interface/ICustomer';
import { AdminCustomersService } from '../../Core/Service/admin-customers.service';
import { ToastService } from '../../../../../core/toast.service';

@Component({
  selector: 'app-customer-edit',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-edit.html',
  styleUrl: './customer-edit.css',
})
export class CustomerEditModal {
  @Input() customer!: ICustomerDetails;

  fullName = '';
  email = '';
  phoneNumber = '';
  isActive = true;
  roles: string[] = [];
  profileImageFile: File | null = null;

  saving = false;

  constructor(
    public activeModal: NgbActiveModal,
    private readonly service: AdminCustomersService,
    private readonly toast: ToastService,
  ) {}

  ngOnInit(): void {
    if (!this.customer) {
      return;
    }

    this.fullName = this.customer.fullName;
    this.email = this.customer.email;
    this.phoneNumber = this.customer.phoneNumber ?? '';
    this.isActive = this.customer.isActive;
    this.roles = [...(this.customer.roles ?? [])];
  }

  onRoleToggle(role: string, event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.checked) {
      if (!this.roles.includes(role)) {
        this.roles.push(role);
      }
    } else {
      this.roles = this.roles.filter(r => r !== role);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      this.profileImageFile = null;
      return;
    }

    const file = input.files[0];
    if (!file.type.startsWith('image/')) {
      this.toast.error('Please choose a valid image file.');
      input.value = '';
      this.profileImageFile = null;
      return;
    }

    this.profileImageFile = file;
  }

  onCancel(): void {
    this.activeModal.dismiss('cancel');
  }

  onSave(event: Event): void {
    event.preventDefault();

    if (!this.customer) {
      return;
    }

    if (!this.fullName.trim() || !this.email.trim()) {
      this.toast.error('Full name and email are required.');
      return;
    }

    const form = new FormData();
    form.append('FullName', this.fullName.trim());
    form.append('Email', this.email.trim());
    form.append('PhoneNumber', this.phoneNumber.trim());
    form.append('IsActive', String(this.isActive));

    const distinctRoles = this.roles.length ? this.roles : ['Client'];
    for (const role of distinctRoles) {
      form.append('Roles', role);
    }

    if (this.profileImageFile) {
      form.append('ProfileImageFile', this.profileImageFile);
    }

    this.saving = true;

    this.service.update(this.customer.id, form).subscribe({
      next: (res) => {
        this.saving = false;

        if (!res.success) {
          this.toast.error(res.message || 'Failed to update customer.');
          return;
        }

        this.toast.success('Customer updated successfully.');
        this.activeModal.close(true);
      },
      error: () => {
        this.saving = false;
        this.toast.error('Failed to update customer.');
      },
    });
  }
}
