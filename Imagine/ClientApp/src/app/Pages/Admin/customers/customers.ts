import { Component, OnDestroy, OnInit, ViewChild, ElementRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomerHeader } from './Components/customer-header/customer-header';
import { CustomerList } from './Components/customer-list/customer-list';
import { CustomerEmptyState } from './Components/customer-empty-state/customer-empty-state';
import { AdminCustomersService, CustomerFilterParams } from './Core/Service/admin-customers.service';
import { ICustomer } from './Core/Interface/ICustomer';
import { ToastService } from '../../../core/toast.service';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-customers',
  imports: [CommonModule, CustomerHeader, CustomerList, CustomerEmptyState],
  templateUrl: './customers.html',
  styleUrls: ['./customers.css'],
})
export class Customers implements OnInit, OnDestroy {
  private readonly service = inject(AdminCustomersService);
  private readonly toast = inject(ToastService);

  customers: ICustomer[] = [];
  loading = false;
  error = '';

  // Filters / view state
  hasCustomers = false;
  selectedStatus: 'all' | 'active' | 'inactive' | 'premium' = 'all';
  viewMode: 'grid' | 'list' = 'grid';
  currentSort: 'name' | 'date' | 'status' = 'date';
  currentSearch = '';

  // Filter counts
  allCount = 0;
  activeCount = 0;
  inactiveCount = 0;
  premiumCount = 0;

  // Pagination metadata (backend-driven)
  currentPage = 1;
  pageSize = 20;
  totalItems = 0;

  private readonly search$ = new Subject<string>();
  private readonly destroy$ = new Subject<void>();

  @ViewChild('importFile') importFileInput?: ElementRef<HTMLInputElement>;

  ngOnInit(): void {
    this.search$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe((term) => {
        this.currentSearch = term;
        this.currentPage = 1;
        this.loadCustomers();
      });

    this.loadCustomers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadCustomers(): void {
    this.loading = true;
    this.error = '';

    const params: CustomerFilterParams = {
      search: this.currentSearch || undefined,
      role: 'Client',
      status: this.selectedStatus,
      sort: this.currentSort,
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
    };

    this.service.getAll(params).subscribe({
      next: (res) => {
        this.loading = false;

        if (!res.success || !res.data) {
          this.customers = [];
          this.hasCustomers = false;
          this.error = res.message || 'Failed to load customers.';
          return;
        }

        const payload = res.data;
        this.customers = payload.items ?? [];
        this.hasCustomers = this.customers.length > 0;

        this.allCount = payload.totalAll ?? 0;
        this.activeCount = payload.totalActive ?? 0;
        this.inactiveCount = payload.totalInactive ?? 0;
        this.premiumCount = payload.totalPremium ?? 0;

        this.currentPage = res.currentPage ?? this.currentPage;
        this.pageSize = res.pageSize ?? this.pageSize;
        this.totalItems = res.totalItems ?? this.customers.length;
      },
      error: () => {
        this.loading = false;
        this.customers = [];
        this.hasCustomers = false;
        this.error = 'Failed to load customers.';
        this.toast.error('Unable to load customers. Please try again.');
      },
    });
  }

  onRefresh(): void {
    this.loadCustomers();
  }

  onFilterChange(filter: string) {
    let mapped: 'all' | 'active' | 'inactive' | 'premium' = 'all';
    switch (filter) {
      case 'active':
        mapped = 'active';
        break;
      case 'inactive':
        mapped = 'inactive';
        break;
      case 'premium':
        mapped = 'premium';
        break;
      case 'all':
      default:
        mapped = 'all';
        break;
    }

    if (this.selectedStatus === mapped) {
      return;
    }

    this.selectedStatus = mapped;
    this.currentPage = 1;
    this.loadCustomers();
  }

  onViewChange(view: string) {
    if (this.viewMode === view) {
      return;
    }
    this.viewMode = (view as any) ?? 'grid';
  }

  onSortChange(sort: string) {
    const mapped = (sort as 'name' | 'date' | 'status') ?? 'date';

    if (this.currentSort === mapped) {
      return;
    }

    this.currentSort = mapped;
    this.currentPage = 1;
    this.loadCustomers();
  }

  onSearchChange(search: string) {
    this.search$.next(search);
  }

  onExport(): void {
    const params: CustomerFilterParams = {
      search: this.currentSearch || undefined,
      role: 'Client',
      status: this.selectedStatus,
      sort: this.currentSort,
    };

    this.service.export(params).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `customers-${new Date().toISOString().slice(0, 10)}.csv`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.toast.error('Failed to export customers.');
      },
    });
  }

  onImportClick(): void {
    this.importFileInput?.nativeElement.click();
  }

  onImportFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      return;
    }

    const file = input.files[0];

    this.service.import(file).subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.toast.error(res.message || 'Failed to import customers.');
          return;
        }

        this.toast.success(
          `Imported ${res.data.imported} customers, skipped ${res.data.skipped}.`
        );
        this.loadCustomers();
      },
      error: () => {
        this.toast.error('Failed to import customers.');
      },
    });

    input.value = '';
  }
}
