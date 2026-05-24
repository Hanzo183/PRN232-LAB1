using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Infrastructure;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/enrollments")]
public sealed class EnrollmentsController : ControllerBase
{
    private static readonly HashSet<string> AllowedSort = new(StringComparer.OrdinalIgnoreCase)
    {
        "enrollmentId", "enrollDate", "status", "studentId", "courseId"
    };

    private static readonly HashSet<string> AllowedExpand = new(StringComparer.OrdinalIgnoreCase)
    {
        "student", "course"
    };

    private readonly IEnrollmentService _enrollments;

    public EnrollmentsController(IEnrollmentService enrollments)
    {
        _enrollments = enrollments;
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

        var result = await _enrollments.GetListAsync(query);
        var items = result.Items.Select(e => e.ToResponse()).ToList();

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
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> GetById(int id)
    {
        var enrollment = await _enrollments.GetByIdAsync(id);
        if (enrollment is null)
        {
            return NotFound(ApiResponse<object>.Fail("NotFound", $"Enrollment {id} not found"));
        }

        return Ok(ApiResponse<EnrollmentResponse>.Ok(enrollment.ToResponse()));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> Create([FromBody] EnrollmentUpsertRequest request)
    {
        var result = await _enrollments.CreateAsync(new EnrollmentUpsertModel(request.StudentId, request.CourseId, request.EnrollDate, request.Status));
        if (!result.Success)
        {
            return BadRequest(ApiResponse<object>.Fail(result.Error!.Code, result.Error.Message));
        }

        var response = result.Data!.ToResponse();
        return CreatedAtAction(nameof(GetById), new { id = response.EnrollmentId }, ApiResponse<EnrollmentResponse>.Ok(response));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> Update(int id, [FromBody] EnrollmentUpsertRequest request)
    {
        var result = await _enrollments.UpdateAsync(id, new EnrollmentUpsertModel(request.StudentId, request.CourseId, request.EnrollDate, request.Status));
        if (!result.Success)
        {
            return result.Error!.Code == "NotFound"
                ? NotFound(ApiResponse<object>.Fail(result.Error.Code, result.Error.Message))
                : BadRequest(ApiResponse<object>.Fail(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<EnrollmentResponse>.Ok(result.Data!.ToResponse()));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var result = await _enrollments.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(ApiResponse<object>.Fail(result.Error!.Code, result.Error.Message));
        }

        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }
}
