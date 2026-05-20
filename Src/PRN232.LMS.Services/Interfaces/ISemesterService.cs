using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Interfaces;

public interface ISemesterService
{
    Task<ServiceResult<PagedResult<SemesterBusinessModel>>> GetListAsync(ListQueryModel query, CancellationToken cancellationToken = default);
    Task<ServiceResult<SemesterBusinessModel>> GetByIdAsync(int id, string? expand, CancellationToken cancellationToken = default);
    Task<ServiceResult<SemesterBusinessModel>> CreateAsync(SemesterBusinessModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult<SemesterBusinessModel>> UpdateAsync(int id, SemesterBusinessModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult<SemesterBusinessModel>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
