namespace Application.Common.Models
{
    public class BaseResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int? CurrentPage { get; set; }
        public int? PageSize { get; set; }
        public int? TotalItems { get; set; }
        public int? TotalPages => TotalItems.HasValue && PageSize.HasValue && PageSize.Value > 0
            ? (int)Math.Ceiling((double)TotalItems.Value / PageSize.Value)
            : null;

        public static BaseResponse<T> SuccessResponse(T data, string message = "Operation completed successfully")
        {
            return new BaseResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static BaseResponse<T> SuccessResponse(T data, int currentPage, int pageSize, int totalItems, string message = "Operation completed successfully")
        {
            return new BaseResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        public static BaseResponse<T> FailureResponse(string message)
        {
            return new BaseResponse<T>
            {
                Success = false,
                Message = message,
                Data = default
            };
        }
    }
}
