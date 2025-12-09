export interface ICustomer {
  id: string;
  fullName: string;
  email: string;
  phoneNumber?: string | null;
  roles: string[];
  isActive: boolean;
  profileImageUrl?: string | null;
  createdAt: string;
}

export interface ICustomerDetails extends ICustomer {
  firstName: string;
  lastName: string;
  address?: string | null;
  ordersCount: number;
  updatedAt: string;
}

export interface ICustomerListResult {
  items: ICustomer[];
  totalAll: number;
  totalActive: number;
  totalInactive: number;
  totalPremium: number;
}

export interface IImportCustomersResult {
  imported: number;
  skipped: number;
}
