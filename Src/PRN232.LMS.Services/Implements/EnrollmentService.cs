using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Implements;

public class EnrollmentService(
    IGenericRepository<Enrollment> enrollmentRepository,
    IGenericRepository<Student> studentRepository,
    IGenericRepository<Course> courseRepository) : IEnrollmentService
{
    private static readonly HashSet<string> AllowedExpands = ["student", "course"];

    private static readonly IReadOnlyDictionary<string, LambdaExpression> SortMappings =
        new Dictionary<string, LambdaExpression>(StringComparer.OrdinalIgnoreCase)
        {
            ["enrollmentid"] = (Expression<Func<Enrollment, int>>)(x => x.EnrollmentId),
            ["studentid"] = (Expression<Func<Enrollment, int>>)(x => x.StudentId),
            ["courseid"] = (Expression<Func<Enrollment, int>>)(x => x.CourseId),
            ["enrolldate"] = (Expression<Func<Enrollment, DateTime>>)(x => x.EnrollDate),
            ["status"] = (Expression<Func<Enrollment, string>>)(x => x.Status),
            ["studentname"] = (Expression<Func<Enrollment, string?>>)(x => x.Student != null ? x.Student.FullName : null),
            ["coursename"] = (Expression<Func<Enrollment, string?>>)(x => x.Course != null ? x.Course.CourseName : null)
        };

    public async Task<ServiceResult<PagedResult<EnrollmentBusinessModel>>> GetListAsync(ListQueryModel query, CancellationToken cancellationToken = default)
    {
        var expandItems = QueryParser.ParseToSet(query.Expand);
        var invalidExpandItems = expandItems.Except(AllowedExpands, StringComparer.OrdinalIgnoreCase).ToArray();

        if (invalidExpandItems.Length > 0)
        {
            return ServiceResult<PagedResult<EnrollmentBusinessModel>>.Invalid("Invalid expand fields.", invalidExpandItems);
        }

        var enrollmentsQuery = enrollmentRepository.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            enrollmentsQuery = enrollmentsQuery.Where(x =>
                EF.Functions.Like(x.Status, pattern) ||
                (x.Student != null && (
                    EF.Functions.Like(x.Student.FullName, pattern) ||
                    EF.Functions.Like(x.Student.Email, pattern))) ||
                (x.Course != null && EF.Functions.Like(x.Course.CourseName, pattern)));
        }

        var (sortedQuery, invalidSortFields) = enrollmentsQuery.ApplySort(query.Sort, SortMappings, "enrollmentid");
        if (invalidSortFields.Count > 0)
        {
            return ServiceResult<PagedResult<EnrollmentBusinessModel>>.Invalid("Invalid sort fields.", invalidSortFields);
        }

        var totalItems = await sortedQuery.CountAsync(cancellationToken);
        var materializedQuery = ApplyIncludes(sortedQuery, expandItems);

        var items = await materializedQuery
            .Skip((query.NormalizedPage - 1) * query.NormalizedSize)
            .Take(query.NormalizedSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<EnrollmentBusinessModel>>.Ok(new PagedResult<EnrollmentBusinessModel>
        {
            Items = items.Select(entity => MapToBusiness(
                entity,
                includeStudent: expandItems.Contains("student"),
                includeCourse: expandItems.Contains("course"))).ToList(),
            Page = query.NormalizedPage,
            PageSize = query.NormalizedSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.NormalizedSize)
        });
    }

    public async Task<ServiceResult<EnrollmentBusinessModel>> GetByIdAsync(int id, string? expand, CancellationToken cancellationToken = default)
    {
        var expandItems = ResolveDetailExpands(expand);
        var invalidExpandItems = expandItems.Except(AllowedExpands, StringComparer.OrdinalIgnoreCase).ToArray();

        if (invalidExpandItems.Length > 0)
        {
            return ServiceResult<EnrollmentBusinessModel>.Invalid("Invalid expand fields.", invalidExpandItems);
        }

        var entity = await ApplyIncludes(enrollmentRepository.Query().AsNoTracking(), expandItems)
            .FirstOrDefaultAsync(x => x.EnrollmentId == id, cancellationToken);

        if (entity is null)
        {
            return ServiceResult<EnrollmentBusinessModel>.NotFound($"Enrollment with id {id} was not found.");
        }

        return ServiceResult<EnrollmentBusinessModel>.Ok(MapToBusiness(
            entity,
            includeStudent: expandItems.Contains("student"),
            includeCourse: expandItems.Contains("course")));
    }

    public async Task<ServiceResult<EnrollmentBusinessModel>> CreateAsync(EnrollmentBusinessModel model, CancellationToken cancellationToken = default)
    {
        var validationErrors = await ValidateEnrollmentAsync(model, null, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<EnrollmentBusinessModel>.Invalid("Enrollment data is invalid.", validationErrors);
        }

        var entity = new Enrollment
        {
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollDate = model.EnrollDate,
            Status = model.Status.Trim()
        };

        await enrollmentRepository.AddAsync(entity, cancellationToken);
        await enrollmentRepository.SaveChangesAsync(cancellationToken);

        entity = await ApplyIncludes(
                enrollmentRepository.Query().AsNoTracking(),
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "student", "course" })
            .FirstAsync(x => x.EnrollmentId == entity.EnrollmentId, cancellationToken);

        return ServiceResult<EnrollmentBusinessModel>.Ok(
            MapToBusiness(entity, includeStudent: true, includeCourse: true),
            "Enrollment created successfully.");
    }

    public async Task<ServiceResult<EnrollmentBusinessModel>> UpdateAsync(int id, EnrollmentBusinessModel model, CancellationToken cancellationToken = default)
    {
        var validationErrors = await ValidateEnrollmentAsync(model, id, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<EnrollmentBusinessModel>.Invalid("Enrollment data is invalid.", validationErrors);
        }

        var entity = await enrollmentRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<EnrollmentBusinessModel>.NotFound($"Enrollment with id {id} was not found.");
        }

        entity.StudentId = model.StudentId;
        entity.CourseId = model.CourseId;
        entity.EnrollDate = model.EnrollDate;
        entity.Status = model.Status.Trim();

        enrollmentRepository.Update(entity);
        await enrollmentRepository.SaveChangesAsync(cancellationToken);

        var updatedEntity = await ApplyIncludes(
                enrollmentRepository.Query().AsNoTracking(),
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "student", "course" })
            .FirstAsync(x => x.EnrollmentId == id, cancellationToken);

        return ServiceResult<EnrollmentBusinessModel>.Ok(
            MapToBusiness(updatedEntity, includeStudent: true, includeCourse: true),
            "Enrollment updated successfully.");
    }

    public async Task<ServiceResult<EnrollmentBusinessModel>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await ApplyIncludes(
                enrollmentRepository.Query().AsNoTracking(),
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "student", "course" })
            .FirstOrDefaultAsync(x => x.EnrollmentId == id, cancellationToken);

        if (entity is null)
        {
            return ServiceResult<EnrollmentBusinessModel>.NotFound($"Enrollment with id {id} was not found.");
        }

        var deletedModel = MapToBusiness(entity, includeStudent: true, includeCourse: true);
        var trackedEntity = await enrollmentRepository.GetByIdAsync(id, cancellationToken);

        if (trackedEntity is null)
        {
            return ServiceResult<EnrollmentBusinessModel>.NotFound($"Enrollment with id {id} was not found.");
        }

        enrollmentRepository.Remove(trackedEntity);
        await enrollmentRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<EnrollmentBusinessModel>.Ok(deletedModel, "Enrollment deleted successfully.");
    }

    private async Task<List<string>> ValidateEnrollmentAsync(EnrollmentBusinessModel model, int? currentId, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(model.Status))
        {
            errors.Add("Status is required.");
        }

        var studentExists = await studentRepository.AnyAsync(x => x.StudentId == model.StudentId, cancellationToken);
        if (!studentExists)
        {
            errors.Add("StudentId does not exist.");
        }

        var courseExists = await courseRepository.AnyAsync(x => x.CourseId == model.CourseId, cancellationToken);
        if (!courseExists)
        {
            errors.Add("CourseId does not exist.");
        }

        var duplicatePair = await enrollmentRepository.Query()
            .AsNoTracking()
            .AnyAsync(x =>
                x.StudentId == model.StudentId &&
                x.CourseId == model.CourseId &&
                (!currentId.HasValue || x.EnrollmentId != currentId.Value), cancellationToken);

        if (duplicatePair)
        {
            errors.Add("The student is already enrolled in the selected course.");
        }

        return errors;
    }

    private static IQueryable<Enrollment> ApplyIncludes(IQueryable<Enrollment> query, IReadOnlySet<string> expandItems)
    {
        if (expandItems.Contains("student"))
        {
            query = query.Include(x => x.Student);
        }

        if (expandItems.Contains("course"))
        {
            query = query.Include(x => x.Course);
        }

        return query;
    }

    private static HashSet<string> ResolveDetailExpands(string? expand)
    {
        if (string.IsNullOrWhiteSpace(expand))
        {
            return [.. AllowedExpands];
        }

        return QueryParser.ParseToSet(expand);
    }

    private static EnrollmentBusinessModel MapToBusiness(
        Enrollment entity,
        bool includeStudent = false,
        bool includeCourse = false)
        => new()
        {
            EnrollmentId = entity.EnrollmentId,
            StudentId = entity.StudentId,
            CourseId = entity.CourseId,
            EnrollDate = entity.EnrollDate,
            Status = entity.Status,
            Student = includeStudent && entity.Student is not null
                ? new StudentBusinessModel
                {
                    StudentId = entity.Student.StudentId,
                    FullName = entity.Student.FullName,
                    Email = entity.Student.Email,
                    DateOfBirth = entity.Student.DateOfBirth
                }
                : null,
            Course = includeCourse && entity.Course is not null
                ? new CourseBusinessModel
                {
                    CourseId = entity.Course.CourseId,
                    CourseName = entity.Course.CourseName,
                    SemesterId = entity.Course.SemesterId,
                    SubjectId = entity.Course.SubjectId
                }
                : null
        };
}
