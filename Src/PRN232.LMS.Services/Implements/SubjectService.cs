using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Implements;

public class SubjectService(
    IGenericRepository<Subject> subjectRepository) : ISubjectService
{
    private static readonly HashSet<string> AllowedExpands = ["courses"];

    private static readonly IReadOnlyDictionary<string, LambdaExpression> SortMappings =
        new Dictionary<string, LambdaExpression>(StringComparer.OrdinalIgnoreCase)
        {
            ["subjectid"] = (Expression<Func<Subject, int>>)(x => x.SubjectId),
            ["subjectcode"] = (Expression<Func<Subject, string>>)(x => x.SubjectCode),
            ["subjectname"] = (Expression<Func<Subject, string>>)(x => x.SubjectName),
            ["credit"] = (Expression<Func<Subject, int>>)(x => x.Credit)
        };

    public async Task<ServiceResult<PagedResult<SubjectBusinessModel>>> GetListAsync(ListQueryModel query, CancellationToken cancellationToken = default)
    {
        var expandItems = QueryParser.ParseToSet(query.Expand);
        var invalidExpandItems = expandItems.Except(AllowedExpands, StringComparer.OrdinalIgnoreCase).ToArray();

        if (invalidExpandItems.Length > 0)
        {
            return ServiceResult<PagedResult<SubjectBusinessModel>>.Invalid("Invalid expand fields.", invalidExpandItems);
        }

        var subjectsQuery = subjectRepository.Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            subjectsQuery = subjectsQuery.Where(x =>
                EF.Functions.Like(x.SubjectCode, pattern) ||
                EF.Functions.Like(x.SubjectName, pattern));
        }

        var (sortedQuery, invalidSortFields) = subjectsQuery.ApplySort(query.Sort, SortMappings, "subjectid");
        if (invalidSortFields.Count > 0)
        {
            return ServiceResult<PagedResult<SubjectBusinessModel>>.Invalid("Invalid sort fields.", invalidSortFields);
        }

        var totalItems = await sortedQuery.CountAsync(cancellationToken);
        var materializedQuery = ApplyIncludes(sortedQuery, expandItems);

        var items = await materializedQuery
            .Skip((query.NormalizedPage - 1) * query.NormalizedSize)
            .Take(query.NormalizedSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<SubjectBusinessModel>>.Ok(new PagedResult<SubjectBusinessModel>
        {
            Items = items.Select(entity => MapToBusiness(entity, includeCourses: expandItems.Contains("courses"))).ToList(),
            Page = query.NormalizedPage,
            PageSize = query.NormalizedSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)query.NormalizedSize)
        });
    }

    public async Task<ServiceResult<SubjectBusinessModel>> GetByIdAsync(int id, string? expand, CancellationToken cancellationToken = default)
    {
        var expandItems = ResolveDetailExpands(expand);
        var invalidExpandItems = expandItems.Except(AllowedExpands, StringComparer.OrdinalIgnoreCase).ToArray();

        if (invalidExpandItems.Length > 0)
        {
            return ServiceResult<SubjectBusinessModel>.Invalid("Invalid expand fields.", invalidExpandItems);
        }

        var entity = await ApplyIncludes(subjectRepository.Query().AsNoTracking(), expandItems)
            .FirstOrDefaultAsync(x => x.SubjectId == id, cancellationToken);

        if (entity is null)
        {
            return ServiceResult<SubjectBusinessModel>.NotFound($"Subject with id {id} was not found.");
        }

        return ServiceResult<SubjectBusinessModel>.Ok(
            MapToBusiness(entity, includeCourses: expandItems.Contains("courses")));
    }

    public async Task<ServiceResult<SubjectBusinessModel>> CreateAsync(SubjectBusinessModel model, CancellationToken cancellationToken = default)
    {
        var validationErrors = await ValidateSubjectAsync(model, null, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<SubjectBusinessModel>.Invalid("Subject data is invalid.", validationErrors);
        }

        var entity = new Subject
        {
            SubjectCode = model.SubjectCode.Trim(),
            SubjectName = model.SubjectName.Trim(),
            Credit = model.Credit
        };

        await subjectRepository.AddAsync(entity, cancellationToken);
        await subjectRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<SubjectBusinessModel>.Ok(MapToBusiness(entity), "Subject created successfully.");
    }

    public async Task<ServiceResult<SubjectBusinessModel>> UpdateAsync(int id, SubjectBusinessModel model, CancellationToken cancellationToken = default)
    {
        var validationErrors = await ValidateSubjectAsync(model, id, cancellationToken);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<SubjectBusinessModel>.Invalid("Subject data is invalid.", validationErrors);
        }

        var entity = await subjectRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<SubjectBusinessModel>.NotFound($"Subject with id {id} was not found.");
        }

        entity.SubjectCode = model.SubjectCode.Trim();
        entity.SubjectName = model.SubjectName.Trim();
        entity.Credit = model.Credit;

        subjectRepository.Update(entity);
        await subjectRepository.SaveChangesAsync(cancellationToken);

        return ServiceResult<SubjectBusinessModel>.Ok(MapToBusiness(entity), "Subject updated successfully.");
    }

    public async Task<ServiceResult<SubjectBusinessModel>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await subjectRepository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<SubjectBusinessModel>.NotFound($"Subject with id {id} was not found.");
        }

        var deletedModel = MapToBusiness(entity);
        subjectRepository.Remove(entity);

        try
        {
            await subjectRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ServiceResult<SubjectBusinessModel>.Invalid("Subject cannot be deleted because it is referenced by existing courses.");
        }

        return ServiceResult<SubjectBusinessModel>.Ok(deletedModel, "Subject deleted successfully.");
    }

    private async Task<List<string>> ValidateSubjectAsync(SubjectBusinessModel model, int? currentId, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(model.SubjectCode))
        {
            errors.Add("SubjectCode is required.");
        }

        if (string.IsNullOrWhiteSpace(model.SubjectName))
        {
            errors.Add("SubjectName is required.");
        }

        if (model.Credit < 1 || model.Credit > 10)
        {
            errors.Add("Credit must be between 1 and 10.");
        }

        var normalizedCode = model.SubjectCode.Trim().ToLowerInvariant();
        var codeExists = await subjectRepository.Query()
            .AsNoTracking()
            .AnyAsync(x => x.SubjectCode.ToLower() == normalizedCode && (!currentId.HasValue || x.SubjectId != currentId.Value), cancellationToken);

        if (codeExists)
        {
            errors.Add("SubjectCode already exists.");
        }

        return errors;
    }

    private static IQueryable<Subject> ApplyIncludes(IQueryable<Subject> query, IReadOnlySet<string> expandItems)
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

    private static SubjectBusinessModel MapToBusiness(Subject entity, bool includeCourses = false)
        => new()
        {
            SubjectId = entity.SubjectId,
            SubjectCode = entity.SubjectCode,
            SubjectName = entity.SubjectName,
            Credit = entity.Credit,
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
