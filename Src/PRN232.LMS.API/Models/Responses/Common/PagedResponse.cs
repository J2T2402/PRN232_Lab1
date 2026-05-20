namespace PRN232.LMS.API.Models.Responses.Common;

public class PagedResponse<T> : ApiResponse<T>
{
    public PaginationMetadata? Pagination { get; set; }
}
