using System.ComponentModel.DataAnnotations;

namespace EngAce.Api.DTO.Shared;

// ===== SHARED DTOs =====
// DTOs được sử dụng chung giữa nhiều modules

/// <summary>
/// Base response DTO cho API responses
/// </summary>
public class BaseResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Paginated response DTO
/// </summary>
public class PaginatedResponseDto<T>
{
    public List<T> Items { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
}

/// <summary>
/// Base pagination request DTO
/// </summary>
public class PaginationRequestDto
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

/// <summary>
/// Date range filter DTO
/// </summary>
public class DateRangeFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    
    public bool IsValid => FromDate == null || ToDate == null || FromDate <= ToDate;
}

/// <summary>
/// File upload DTO
/// </summary>
public class FileUploadDto
{
    [Required]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public string ContentType { get; set; } = string.Empty;
    
    [Required]
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    
    public long FileSize => FileContent.Length;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Generic filter DTO
/// </summary>
public class FilterDto
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty; // equals, contains, startsWith, etc.
    public object? Value { get; set; }
}