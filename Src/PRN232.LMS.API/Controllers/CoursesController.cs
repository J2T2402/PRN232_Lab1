using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Helpers;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Requests.Common;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.API.Models.Responses.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.BusinessModels;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<IEnumerable<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetListAsync([FromQuery] BaseQueryRequest query, CancellationToken cancellationToken)
    {
        if (!FieldSelectionHelper.TryValidateFields<CourseResponse>(query.Fields, out var invalidFields))
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid fields requested.", invalidFields));
        }

        var result = await courseService.GetListAsync(ToListQuery(query), cancellationToken);
        if (!result.Success)
        {
            return ToErrorResult(result);
        }

        var items = result.Data!.Items.Select(ResponseMapper.ToCourseResponse).ToList();
        var shapedItems = FieldSelectionHelper.ShapeCollection(items, query.Fields);

        return Ok(ToPagedResponse(shapedItems, result.Data, result.Message));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByIdAsync(int id, [FromQuery] string? expand, CancellationToken cancellationToken)
    {
        var result = await courseService.GetByIdAsync(id, expand, cancellationToken);
        if (!result.Success)
        {
            return ToErrorResult(result);
        }

        return Ok(ApiResponse<CourseResponse>.Ok(ResponseMapper.ToCourseResponse(result.Data!), result.Message));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var result = await courseService.CreateAsync(new CourseBusinessModel
        {
            CourseName = request.CourseName,
            SemesterId = request.SemesterId,
            SubjectId = request.SubjectId
        }, cancellationToken);

        if (!result.Success)
        {
            return ToErrorResult(result);
        }

        var response = ResponseMapper.ToCourseResponse(result.Data!);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CourseResponse>.Ok(response, result.Message));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        var result = await courseService.UpdateAsync(id, new CourseBusinessModel
        {
            CourseName = request.CourseName,
            SemesterId = request.SemesterId,
            SubjectId = request.SubjectId
        }, cancellationToken);

        if (!result.Success)
        {
            return ToErrorResult(result);
        }

        return Ok(ApiResponse<CourseResponse>.Ok(ResponseMapper.ToCourseResponse(result.Data!), result.Message));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var result = await courseService.DeleteAsync(id, cancellationToken);
        if (!result.Success)
        {
            return ToErrorResult(result);
        }

        return Ok(ApiResponse<CourseResponse>.Ok(ResponseMapper.ToCourseResponse(result.Data!), result.Message));
    }

    private static ListQueryModel ToListQuery(BaseQueryRequest query)
        => new()
        {
            Search = query.Search,
            Sort = query.Sort,
            Page = query.Page,
            Size = query.Size,
            Expand = query.Expand
        };

    private IActionResult ToErrorResult<T>(ServiceResult<T> result)
        => result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(ApiResponse<object>.Fail(result.Message, result.Errors)),
            ServiceErrorType.Validation => BadRequest(ApiResponse<object>.Fail(result.Message, result.Errors)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail(result.Message, result.Errors))
        };

    private static PagedResponse<IEnumerable<object>> ToPagedResponse(IReadOnlyList<object> items, PagedResult<CourseBusinessModel> pagedResult, string message)
        => new()
        {
            Success = true,
            Message = message,
            Data = items,
            Errors = null,
            Pagination = new PaginationMetadata
            {
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                TotalItems = pagedResult.TotalItems,
                TotalPages = pagedResult.TotalPages
            }
        };
}
