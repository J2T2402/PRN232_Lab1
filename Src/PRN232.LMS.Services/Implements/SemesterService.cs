using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Implements;

public class SemesterService(IGenericRepository<Semester> semesterRepository) : ISemesterService
{
    private static readonly HashSet<string> AllowedExpands = ["courses"];

    private static readonly IReadOnlyDictionary<string, LambdaExpression> SortMappings =
        new Dictionary<string, LambdaExpression>(StringComparer.OrdinalIgnoreCase)
        {
            ["semesterid"] = (Expression<Func<Semester, int>>)(x => x.SemesterId),
            ["semestername"] = (Expression<Func<Semester, string>>)(x => x.SemesterName),
            ["startdate"] = (Expression<Func<Semester, DateTime>>)(x => x.StartDate),
            ["enddate"] = (Expression<Func<Semester, DateTime>>)(x => x.EndDate)
        };

    public async Task<ServiceResult<PagedResult<SemesterBusinessModel>>> GetListAsync(ListQueryModel query, CancellationToken cancellationToken = default)
    {
        var expandItems = QueryParser.ParseToSet(query.Expand);
        var invalidExpandItems = expandItems.Except(AllowedExpands, StringComparer.OrdinalIgnoreCase).ToArray();

        if (invalidExpandItems.Length > 0)
        {
            return ServiceResult<PagedResult<SemesterBusinessModel>>.Invalid("Invalid expand fields.", invalidExpandItems);
        }

        var semestersQuery = semesterRepository.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            semestersQuery = semestersQuery.Where(x => EF.Functions.Like(x.SemesterName, pattern));
        }

        var (sortedQuery, invalidSortFields) = semestersQuery.ApplySort(query.Sort, SortMappings, "semesterid");
        if (invalidSortFields.Count > 0)
        {
            return ServiceResult<PagedResult<SemesterBusinessModel>>.Invalid("Invalid sort fields.", invalidSortFields);
        }

        var totalItems = await sortedQuery.CountAsync(cancellationToken);
        var materializedQuery = ApplyIncludes(sortedQuery, expandItems);

        var items = await materializedQuery
            .Skip((query.NormalizedPage - 1) * query.NormalizedSize)
            .Take(query.NormalizedSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<SemesterBusinessModel>>.Ok(new PagedResult<SemesterBusinessModel>
        {
            Items = items.Select(entity => MapToBusiness(entity, includeCourses: expandItems.Contains("courses"))).ToList(),
            Page = query.NormalizedPage,
            PageSize = query.NormalizedSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.NormalizedSize)
        });
    }

    public async Task<ServiceResult<SemesterBusinessModel>> GetByIdAsync(int id, string? expand, CancellationToken cancellationToken = default)
    {
        var expandItems = ResolveDetailExpands(expand);
        var invalidExpandItems = expandItems.Except(AllowedExpands, StringComparer.OrdinalIgnoreCase).ToArray();

        if (invalidExpandItems.Length > 0)
        {
            return ServiceResult<SemesterBusinessModel>.Invalid("Invalid expand fields.", invalidExpandItems);
        }

        var entity = await ApplyIncludes(semesterRepository.Query().AsNoTracking(), expandItems)
            .FirstOrDefaultAsync(x => x.SemesterId == id, cancellationToken);

        if (entity is null)
        {
            return ServiceResult<SemesterBusinessModel>.NotFound($"Semester with id {id} was not found.");
        }

        return ServiceResult<SemesterBusinessModel>.Ok(
            MapToBusiness(entity, includeCourses: expandItems.Contains("courses")));
    }

    public async Task<ServiceResult<SemesterBusinessModel>> CreateAsync(SemesterBusinessModel model, CancellationToken cancellationToken = default)
    {
        var validation = ValidateSemester(model);
        if (validation.Count > 0)
        {
            return ServiceResult<SemesterBusinessModel>.Invalid("Semester data is invalid.", validation);
        }

        var entity = new Semester
        {
            SemesterName = model.SemesterName.Trim(),
            StartDate = model.StartDate,
            EndDate = model.EndDate
        };

        await semesterRepository.AddAsync(entity, cancellationToken);
        await semesterRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<SemesterBusinessModel>.Ok(MapToBusiness(entity), "Semester created successfully.");
    }

    public async Task<ServiceResult<SemesterBusinessModel>> UpdateAsync(int id, SemesterBusinessModel model, CancellationToken cancellationToken = default)
    {
        var validation = ValidateSemester(model);
        if (validation.Count > 0)
        {
            return ServiceResult<SemesterBusinessModel>.Invalid("Semester data is invalid.", validation);
        }

        var entity = await semesterRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<SemesterBusinessModel>.NotFound($"Semester with id {id} was not found.");
        }

        entity.SemesterName = model.SemesterName.Trim();
        entity.StartDate = model.StartDate;
        entity.EndDate = model.EndDate;

        semesterRepository.Update(entity);
        await semesterRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<SemesterBusinessModel>.Ok(MapToBusiness(entity), "Semester updated successfully.");
    }

    public async Task<ServiceResult<SemesterBusinessModel>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await semesterRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<SemesterBusinessModel>.NotFound($"Semester with id {id} was not found.");
        }

        var deletedModel = MapToBusiness(entity);
        semesterRepository.Remove(entity);

        try
        {
            await semesterRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ServiceResult<SemesterBusinessModel>.Invalid("Semester cannot be deleted because it is referenced by existing courses.");
        }

        return ServiceResult<SemesterBusinessModel>.Ok(deletedModel, "Semester deleted successfully.");
    }

    private static IQueryable<Semester> ApplyIncludes(IQueryable<Semester> query, IReadOnlySet<string> expandItems)
    {
        if (expandItems.Contains("courses"))
        {
            query = query.Include(x => x.Courses).AsSplitQuery();
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

    private static List<string> ValidateSemester(SemesterBusinessModel model)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(model.SemesterName))
        {
            errors.Add("SemesterName is required.");
        }

        if (model.StartDate >= model.EndDate)
        {
            errors.Add("StartDate must be earlier than EndDate.");
        }

        return errors;
    }

    private static SemesterBusinessModel MapToBusiness(Semester entity, bool includeCourses = false)
        => new()
        {
            SemesterId = entity.SemesterId,
            SemesterName = entity.SemesterName,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Courses = includeCourses
                ? entity.Courses
                    .OrderBy(x => x.CourseId)
                    .Select(course => new CourseBusinessModel
                    {
                        CourseId = course.CourseId,
                        CourseName = course.CourseName,
                        SemesterId = course.SemesterId,
                        SubjectId = course.SubjectId
                    })
                    .ToList()
                : null
        };
}
