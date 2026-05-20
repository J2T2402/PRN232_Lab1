using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Implements;

public class StudentService(
    IGenericRepository<Student> studentRepository) : IStudentService
{
    private static readonly HashSet<string> AllowedExpands = ["enrollments"];

    private static readonly IReadOnlyDictionary<string, LambdaExpression> SortMappings =
        new Dictionary<string, LambdaExpression>(StringComparer.OrdinalIgnoreCase)
        {
            ["studentid"] = (Expression<Func<Student, int>>)(x => x.StudentId),
            ["fullname"] = (Expression<Func<Student, string>>)(x => x.FullName),
            ["email"] = (Expression<Func<Student, string>>)(x => x.Email),
            ["dateofbirth"] = (Expression<Func<Student, DateTime>>)(x => x.DateOfBirth)
        };

    public async Task<ServiceResult<PagedResult<StudentBusinessModel>>> GetListAsync(ListQueryModel query, CancellationToken cancellationToken = default)
    {
        var expandItems = QueryParser.ParseToSet(query.Expand);
        var invalidExpandItems = expandItems.Except(AllowedExpands, StringComparer.OrdinalIgnoreCase).ToArray();

        if (invalidExpandItems.Length > 0)
        {
            return ServiceResult<PagedResult<StudentBusinessModel>>.Invalid("Invalid expand fields.", invalidExpandItems);
        }

        var studentsQuery = studentRepository.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            studentsQuery = studentsQuery.Where(x =>
                EF.Functions.Like(x.FullName, pattern) ||
                EF.Functions.Like(x.Email, pattern));
        }

        var (sortedQuery, invalidSortFields) = studentsQuery.ApplySort(query.Sort, SortMappings, "studentid");
        if (invalidSortFields.Count > 0)
        {
            return ServiceResult<PagedResult<StudentBusinessModel>>.Invalid("Invalid sort fields.", invalidSortFields);
        }

        var totalItems = await sortedQuery.CountAsync(cancellationToken);
        var materializedQuery = ApplyIncludes(sortedQuery, expandItems);

        var items = await materializedQuery
            .Skip((query.NormalizedPage - 1) * query.NormalizedSize)
            .Take(query.NormalizedSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<StudentBusinessModel>>.Ok(new PagedResult<StudentBusinessModel>
        {
            Items = items.Select(entity => MapToBusiness(entity, includeEnrollments: expandItems.Contains("enrollments"))).ToList(),
            Page = query.NormalizedPage,
            PageSize = query.NormalizedSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.NormalizedSize)
        });
    }

    public async Task<ServiceResult<StudentBusinessModel>> GetByIdAsync(int id, string? expand, CancellationToken cancellationToken = default)
    {
        var expandItems = ResolveDetailExpands(expand);
        var invalidExpandItems = expandItems.Except(AllowedExpands, StringComparer.OrdinalIgnoreCase).ToArray();

        if (invalidExpandItems.Length > 0)
        {
            return ServiceResult<StudentBusinessModel>.Invalid("Invalid expand fields.", invalidExpandItems);
        }

        var entity = await ApplyIncludes(studentRepository.Query().AsNoTracking(), expandItems)
            .FirstOrDefaultAsync(x => x.StudentId == id, cancellationToken);

        if (entity is null)
        {
            return ServiceResult<StudentBusinessModel>.NotFound($"Student with id {id} was not found.");
        }

        return ServiceResult<StudentBusinessModel>.Ok(
            MapToBusiness(entity, includeEnrollments: expandItems.Contains("enrollments")));
    }

    public async Task<ServiceResult<StudentBusinessModel>> CreateAsync(StudentBusinessModel model, CancellationToken cancellationToken = default)
    {
        var validationErrors = await ValidateStudentAsync(model, null, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<StudentBusinessModel>.Invalid("Student data is invalid.", validationErrors);
        }

        var entity = new Student
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            DateOfBirth = model.DateOfBirth
        };

        await studentRepository.AddAsync(entity, cancellationToken);
        await studentRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<StudentBusinessModel>.Ok(MapToBusiness(entity), "Student created successfully.");
    }

    public async Task<ServiceResult<StudentBusinessModel>> UpdateAsync(int id, StudentBusinessModel model, CancellationToken cancellationToken = default)
    {
        var validationErrors = await ValidateStudentAsync(model, id, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<StudentBusinessModel>.Invalid("Student data is invalid.", validationErrors);
        }

        var entity = await studentRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<StudentBusinessModel>.NotFound($"Student with id {id} was not found.");
        }

        entity.FullName = model.FullName.Trim();
        entity.Email = model.Email.Trim();
        entity.DateOfBirth = model.DateOfBirth;

        studentRepository.Update(entity);
        await studentRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<StudentBusinessModel>.Ok(MapToBusiness(entity), "Student updated successfully.");
    }

    public async Task<ServiceResult<StudentBusinessModel>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await studentRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<StudentBusinessModel>.NotFound($"Student with id {id} was not found.");
        }

        var deletedModel = MapToBusiness(entity);
        studentRepository.Remove(entity);
        await studentRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<StudentBusinessModel>.Ok(deletedModel, "Student deleted successfully.");
    }

    private async Task<List<string>> ValidateStudentAsync(StudentBusinessModel model, int? currentId, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(model.FullName))
        {
            errors.Add("FullName is required.");
        }

        if (string.IsNullOrWhiteSpace(model.Email))
        {
            errors.Add("Email is required.");
        }

        if (model.DateOfBirth >= DateTime.UtcNow.Date)
        {
            errors.Add("DateOfBirth must be in the past.");
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        var emailExists = await studentRepository.Query()
            .AsNoTracking()
            .AnyAsync(x => x.Email.ToLower() == normalizedEmail && (!currentId.HasValue || x.StudentId != currentId.Value), cancellationToken);

        if (emailExists)
        {
            errors.Add("Email already exists.");
        }

        return errors;
    }

    private static IQueryable<Student> ApplyIncludes(IQueryable<Student> query, IReadOnlySet<string> expandItems)
    {
        if (expandItems.Contains("enrollments"))
        {
            query = query
                .Include(x => x.Enrollments)
                .ThenInclude(x => x.Course)
                .AsSplitQuery();
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

    private static StudentBusinessModel MapToBusiness(Student entity, bool includeEnrollments = false)
        => new()
        {
            StudentId = entity.StudentId,
            FullName = entity.FullName,
            Email = entity.Email,
            DateOfBirth = entity.DateOfBirth,
            Enrollments = includeEnrollments
                ? entity.Enrollments
                    .OrderBy(x => x.EnrollmentId)
                    .Select(enrollment => new EnrollmentBusinessModel
                    {
                        EnrollmentId = enrollment.EnrollmentId,
                        StudentId = enrollment.StudentId,
                        CourseId = enrollment.CourseId,
                        EnrollDate = enrollment.EnrollDate,
                        Status = enrollment.Status,
                        Course = enrollment.Course is null
                            ? null
                            : new CourseBusinessModel
                            {
                                CourseId = enrollment.Course.CourseId,
                                CourseName = enrollment.Course.CourseName,
                                SemesterId = enrollment.Course.SemesterId,
                                SubjectId = enrollment.Course.SubjectId
                            }
                    })
                    .ToList()
                : null
        };
}
