using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Interfaces;

public interface IStudentService
{
    Task<ServiceResult<PagedResult<StudentBusinessModel>>> GetListAsync(ListQueryModel query, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentBusinessModel>> GetByIdAsync(int id, string? expand, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentBusinessModel>> CreateAsync(StudentBusinessModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentBusinessModel>> UpdateAsync(int id, StudentBusinessModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult<StudentBusinessModel>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
