using System;

namespace Application.Features.TryOn.DTOs
{
    public class PreprocessResultDto
    {
        public string PreprocessedImageUrl { get; set; } = string.Empty;
        public int? CustomizationJobId { get; set; }
    }

    public class DesignGeneratedDto
    {
        public int CustomizationJobId { get; set; }
        public string DesignImageUrl { get; set; } = string.Empty;
    }

    public class ApplyDesignResultDto
    {
        public int CustomizationJobId { get; set; }
        public string FinalProductImageUrl { get; set; } = string.Empty;
    }

    public class TryOnJobCreatedDto
    {
        public string JobId { get; set; } = string.Empty;
        public string? StatusUrl { get; set; }
    }

    public class TryOnJobStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? ImageBase64 { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
        public string? ErrorCode { get; set; }
        public string? Provider { get; set; }
    }

    public class GenerateGarmentResultDto
    {
        public int CustomizationJobId { get; set; }
        public string DeApiRequestId { get; set; } = string.Empty;
        public string? GeneratedGarmentUrl { get; set; }
    }
}
