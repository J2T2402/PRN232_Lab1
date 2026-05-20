using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Interfaces;

public interface IEnrollmentService
{
    Task<ServiceResult<PagedResult<EnrollmentBusinessModel>>> GetListAsync(ListQueryModel query, CancellationToken cancellationToken = default);
    Task<ServiceResult<EnrollmentBusinessModel>> GetByIdAsync(int id, string? expand, CancellationToken cancellationToken = default);
    Task<ServiceResult<EnrollmentBusinessModel>> CreateAsync(EnrollmentBusinessModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult<EnrollmentBusinessModel>> UpdateAsync(int id, EnrollmentBusinessModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult<EnrollmentBusinessModel>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
