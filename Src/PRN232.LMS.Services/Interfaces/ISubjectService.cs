using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Interfaces;

public interface ISubjectService
{
    Task<ServiceResult<PagedResult<SubjectBusinessModel>>> GetListAsync(ListQueryModel query, CancellationToken cancellationToken = default);
    Task<ServiceResult<SubjectBusinessModel>> GetByIdAsync(int id, string? expand, CancellationToken cancellationToken = default);
    Task<ServiceResult<SubjectBusinessModel>> CreateAsync(SubjectBusinessModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult<SubjectBusinessModel>> UpdateAsync(int id, SubjectBusinessModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult<SubjectBusinessModel>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
