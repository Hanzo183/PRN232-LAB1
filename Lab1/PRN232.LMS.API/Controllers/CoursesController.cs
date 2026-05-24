using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Infrastructure;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/courses")]
public sealed class CoursesController : ControllerBase
{
    private static readonly HashSet<string> AllowedSort = new(StringComparer.OrdinalIgnoreCase)
    {
        "courseId", "courseName", "semesterId"
    };

    private static readonly HashSet<string> AllowedExpand = new(StringComparer.OrdinalIgnoreCase)
    {
        "semester", "enrollments"
    };

    private readonly ICourseService _courses;

    public CoursesController(ICourseService courses)
    {
        _courses = courses;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedData<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedData<object>>>> GetList([FromQuery] ListQueryParameters queryParams)
    {
        var (success, error, query) = QueryParsing.ToListQuery(queryParams, AllowedSort, AllowedExpand);
        if (!success || query is null)
        {
            return BadRequest(ApiResponse<object>.Fail("Validation", error ?? "Invalid query"));
        }

        var result = await _courses.GetListAsync(query);
        var items = result.Items.Select(c => c.ToResponse()).ToList();

        var fields = QueryParsing.ParseFields(queryParams.Fields);
        var shaped = FieldSelection.Shape(items, fields);
        if (!shaped.Success)
        {
            return BadRequest(ApiResponse<object>.Fail("Validation", shaped.Error ?? "Invalid fields"));
        }

        var data = new PagedData<object>(
            Items: shaped.Items,
            Pagination: new Pagination(
                result.Pagination.Page,
                result.Pagination.PageSize,
                result.Pagination.TotalItems,
                result.Pagination.TotalPages));

        return Ok(ApiResponse<PagedData<object>>.Ok(data));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> GetById(int id)
    {
        var course = await _courses.GetByIdAsync(id);
        if (course is null)
        {
            return NotFound(ApiResponse<object>.Fail("NotFound", $"Course {id} not found"));
        }

        return Ok(ApiResponse<CourseResponse>.Ok(course.ToResponse()));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> Create([FromBody] CourseUpsertRequest request)
    {
        var result = await _courses.CreateAsync(new CourseUpsertModel(request.CourseName, request.SemesterId));
        if (!result.Success)
        {
            return BadRequest(ApiResponse<object>.Fail(result.Error!.Code, result.Error.Message));
        }

        var response = result.Data!.ToResponse();
        return CreatedAtAction(nameof(GetById), new { id = response.CourseId }, ApiResponse<CourseResponse>.Ok(response));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> Update(int id, [FromBody] CourseUpsertRequest request)
    {
        var result = await _courses.UpdateAsync(id, new CourseUpsertModel(request.CourseName, request.SemesterId));
        if (!result.Success)
        {
            return result.Error!.Code == "NotFound"
                ? NotFound(ApiResponse<object>.Fail(result.Error.Code, result.Error.Message))
                : BadRequest(ApiResponse<object>.Fail(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<CourseResponse>.Ok(result.Data!.ToResponse()));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var result = await _courses.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(ApiResponse<object>.Fail(result.Error!.Code, result.Error.Message));
        }

        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }
}
