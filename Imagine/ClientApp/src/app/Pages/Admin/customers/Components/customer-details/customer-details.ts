import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { ICustomerDetails } from '../../Core/Interface/ICustomer';

@Component({
  selector: 'app-customer-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './customer-details.html',
  styleUrl: './customer-details.css',
})
export class CustomerDetailsModal {
  @Input() customer!: ICustomerDetails;

  constructor(public activeModal: NgbActiveModal) {}

  close(): void {
    this.activeModal.dismiss('close');
  }
}
