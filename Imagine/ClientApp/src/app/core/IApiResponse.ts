export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  currentPage?: number;
  pageSize?: number;
  totalItems?: number;
  totalPages?: number;
}
