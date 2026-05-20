using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Implements;

public class CourseService(
    IGenericRepository<Course> courseRepository,
    IGenericRepository<Semester> semesterRepository,
    IGenericRepository<Subject> subjectRepository) : ICourseService
{
    private static readonly HashSet<string> AllowedExpands = ["semester", "subject", "enrollments"];

    private static readonly IReadOnlyDictionary<string, LambdaExpression> SortMappings =
        new Dictionary<string, LambdaExpression>(StringComparer.OrdinalIgnoreCase)
        {
            ["courseid"] = (Expression<Func<Course, int>>)(x => x.CourseId),
            ["coursename"] = (Expression<Func<Course, string>>)(x => x.CourseName),
            ["semesterid"] = (Expression<Func<Course, int>>)(x => x.SemesterId),
            ["subjectid"] = (Expression<Func<Course, int>>)(x => x.SubjectId),
            ["semestername"] = (Expression<Func<Course, string?>>)(x => x.Semester != null ? x.Semester.SemesterName : null),
            ["subjectcode"] = (Expression<Func<Course, string?>>)(x => x.Subject != null ? x.Subject.SubjectCode : null),
            ["subjectname"] = (Expression<Func<Course, string?>>)(x => x.Subject != null ? x.Subject.SubjectName : null)
        };

    public async Task<ServiceResult<PagedResult<CourseBusinessModel>>> GetListAsync(ListQueryModel query, CancellationToken cancellationToken = default)
    {
        var expandItems = QueryParser.ParseToSet(query.Expand);
        var invalidExpandItems = expandItems.Except(AllowedExpands, StringComparer.OrdinalIgnoreCase).ToArray();

        if (invalidExpandItems.Length > 0)
        {
            return ServiceResult<PagedResult<CourseBusinessModel>>.Invalid("Invalid expand fields.", invalidExpandItems);
        }

        var coursesQuery = courseRepository.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            coursesQuery = coursesQuery.Where(x =>
                EF.Functions.Like(x.CourseName, pattern) ||
                (x.Semester != null && EF.Functions.Like(x.Semester.SemesterName, pattern)) ||
                (x.Subject != null && (
                    EF.Functions.Like(x.Subject.SubjectCode, pattern) ||
                    EF.Functions.Like(x.Subject.SubjectName, pattern))));
        }

        var (sortedQuery, invalidSortFields) = coursesQuery.ApplySort(query.Sort, SortMappings, "courseid");
        if (invalidSortFields.Count > 0)
        {
            return ServiceResult<PagedResult<CourseBusinessModel>>.Invalid("Invalid sort fields.", invalidSortFields);
        }

        var totalItems = await sortedQuery.CountAsync(cancellationToken);
        var materializedQuery = ApplyIncludes(sortedQuery, expandItems);

        var items = await materializedQuery
            .Skip((query.NormalizedPage - 1) * query.NormalizedSize)
            .Take(query.NormalizedSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<CourseBusinessModel>>.Ok(new PagedResult<CourseBusinessModel>
        {
            Items = items.Select(entity => MapToBusiness(
                entity,
                includeSemester: expandItems.Contains("semester"),
                includeSubject: expandItems.Contains("subject"),
                includeEnrollments: expandItems.Contains("enrollments"))).ToList(),
            Page = query.NormalizedPage,
            PageSize = query.NormalizedSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.NormalizedSize)
        });
    }

    public async Task<ServiceResult<CourseBusinessModel>> GetByIdAsync(int id, string? expand, CancellationToken cancellationToken = default)
    {
        var expandItems = ResolveDetailExpands(expand);
        var invalidExpandItems = expandItems.Except(AllowedExpands, StringComparer.OrdinalIgnoreCase).ToArray();

        if (invalidExpandItems.Length > 0)
        {
            return ServiceResult<CourseBusinessModel>.Invalid("Invalid expand fields.", invalidExpandItems);
        }

        var entity = await ApplyIncludes(courseRepository.Query().AsNoTracking(), expandItems)
            .FirstOrDefaultAsync(x => x.CourseId == id, cancellationToken);

        if (entity is null)
        {
            return ServiceResult<CourseBusinessModel>.NotFound($"Course with id {id} was not found.");
        }

        return ServiceResult<CourseBusinessModel>.Ok(MapToBusiness(
            entity,
            includeSemester: expandItems.Contains("semester"),
            includeSubject: expandItems.Contains("subject"),
            includeEnrollments: expandItems.Contains("enrollments")));
    }

    public async Task<ServiceResult<CourseBusinessModel>> CreateAsync(CourseBusinessModel model, CancellationToken cancellationToken = default)
    {
        var validationErrors = await ValidateCourseAsync(model, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<CourseBusinessModel>.Invalid("Course data is invalid.", validationErrors);
        }

        var entity = new Course
        {
            CourseName = model.CourseName.Trim(),
            SemesterId = model.SemesterId,
            SubjectId = model.SubjectId
        };

        await courseRepository.AddAsync(entity, cancellationToken);
        await courseRepository.SaveChangesAsync(cancellationToken);

        entity = await ApplyIncludes(
                courseRepository.Query().AsNoTracking(),
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "semester", "subject" })
            .FirstAsync(x => x.CourseId == entity.CourseId, cancellationToken);

        return ServiceResult<CourseBusinessModel>.Ok(
            MapToBusiness(entity, includeSemester: true, includeSubject: true),
            "Course created successfully.");
    }

    public async Task<ServiceResult<CourseBusinessModel>> UpdateAsync(int id, CourseBusinessModel model, CancellationToken cancellationToken = default)
    {
        var validationErrors = await ValidateCourseAsync(model, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<CourseBusinessModel>.Invalid("Course data is invalid.", validationErrors);
        }

        var entity = await courseRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<CourseBusinessModel>.NotFound($"Course with id {id} was not found.");
        }

        entity.CourseName = model.CourseName.Trim();
        entity.SemesterId = model.SemesterId;
        entity.SubjectId = model.SubjectId;

        courseRepository.Update(entity);
        await courseRepository.SaveChangesAsync(cancellationToken);

        var updatedEntity = await ApplyIncludes(
                courseRepository.Query().AsNoTracking(),
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "semester", "subject" })
            .FirstAsync(x => x.CourseId == id, cancellationToken);

        return ServiceResult<CourseBusinessModel>.Ok(
            MapToBusiness(updatedEntity, includeSemester: true, includeSubject: true),
            "Course updated successfully.");
    }

    public async Task<ServiceResult<CourseBusinessModel>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await ApplyIncludes(
                courseRepository.Query().AsNoTracking(),
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "semester", "subject" })
            .FirstOrDefaultAsync(x => x.CourseId == id, cancellationToken);

        if (entity is null)
        {
            return ServiceResult<CourseBusinessModel>.NotFound($"Course with id {id} was not found.");
        }

        var deletedModel = MapToBusiness(entity, includeSemester: true, includeSubject: true);
        var trackedEntity = await courseRepository.GetByIdAsync(id, cancellationToken);

        if (trackedEntity is null)
        {
            return ServiceResult<CourseBusinessModel>.NotFound($"Course with id {id} was not found.");
        }

        courseRepository.Remove(trackedEntity);
        await courseRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<CourseBusinessModel>.Ok(deletedModel, "Course deleted successfully.");
    }

    private async Task<List<string>> ValidateCourseAsync(CourseBusinessModel model, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(model.CourseName))
        {
            errors.Add("CourseName is required.");
        }

        var semesterExists = await semesterRepository.AnyAsync(x => x.SemesterId == model.SemesterId, cancellationToken);
        if (!semesterExists)
        {
            errors.Add("SemesterId does not exist.");
        }

        var subjectExists = await subjectRepository.AnyAsync(x => x.SubjectId == model.SubjectId, cancellationToken);
        if (!subjectExists)
        {
            errors.Add("SubjectId does not exist.");
        }

        return errors;
    }

    private static IQueryable<Course> ApplyIncludes(IQueryable<Course> query, IReadOnlySet<string> expandItems)
    {
        if (expandItems.Contains("semester"))
        {
            query = query.Include(x => x.Semester);
        }

        if (expandItems.Contains("subject"))
        {
            query = query.Include(x => x.Subject);
        }

        if (expandItems.Contains("enrollments"))
        {
            query = query
                .Include(x => x.Enrollments)
                .ThenInclude(x => x.Student)
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

    private static CourseBusinessModel MapToBusiness(
        Course entity,
        bool includeSemester = false,
        bool includeSubject = false,
        bool includeEnrollments = false)
        => new()
        {
            CourseId = entity.CourseId,
            CourseName = entity.CourseName,
            SemesterId = entity.SemesterId,
            SubjectId = entity.SubjectId,
            Semester = includeSemester && entity.Semester is not null
                ? new SemesterBusinessModel
                {
                    SemesterId = entity.Semester.SemesterId,
                    SemesterName = entity.Semester.SemesterName,
                    StartDate = entity.Semester.StartDate,
                    EndDate = entity.Semester.EndDate
                }
                : null,
            Subject = includeSubject && entity.Subject is not null
                ? new SubjectBusinessModel
                {
                    SubjectId = entity.Subject.SubjectId,
                    SubjectCode = entity.Subject.SubjectCode,
                    SubjectName = entity.Subject.SubjectName,
                    Credit = entity.Subject.Credit
                }
                : null,
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
                        Student = enrollment.Student is null
                            ? null
                            : new StudentBusinessModel
                            {
                                StudentId = enrollment.Student.StudentId,
                                FullName = enrollment.Student.FullName,
                                Email = enrollment.Student.Email,
                                DateOfBirth = enrollment.Student.DateOfBirth
                            }
                    })
                    .ToList()
                : null
        };
}
