using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Interfaces;

public interface ICourseService
{
    Task<ServiceResult<PagedResult<CourseBusinessModel>>> GetListAsync(ListQueryModel query, CancellationToken cancellationToken = default);
    Task<ServiceResult<CourseBusinessModel>> GetByIdAsync(int id, string? expand, CancellationToken cancellationToken = default);
    Task<ServiceResult<CourseBusinessModel>> CreateAsync(CourseBusinessModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult<CourseBusinessModel>> UpdateAsync(int id, CourseBusinessModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult<CourseBusinessModel>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
