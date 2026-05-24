using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Infrastructure;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/students")]
public sealed class StudentsController : ControllerBase
{
    private static readonly HashSet<string> AllowedSort = new(StringComparer.OrdinalIgnoreCase)
    {
        "studentId", "fullName", "email", "dateOfBirth"
    };

    private static readonly HashSet<string> AllowedExpand = new(StringComparer.OrdinalIgnoreCase)
    {
        "enrollments"
    };

    private readonly IStudentService _students;

    public StudentsController(IStudentService students)
    {
        _students = students;
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

        var result = await _students.GetListAsync(query);
        var items = result.Items.Select(s => s.ToResponse()).ToList();

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
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> GetById(int id)
    {
        var student = await _students.GetByIdAsync(id);
        if (student is null)
        {
            return NotFound(ApiResponse<object>.Fail("NotFound", $"Student {id} not found"));
        }

        return Ok(ApiResponse<StudentResponse>.Ok(student.ToResponse()));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> Create([FromBody] StudentUpsertRequest request)
    {
        var result = await _students.CreateAsync(new StudentUpsertModel(request.FullName, request.Email, request.DateOfBirth));
        if (!result.Success)
        {
            return BadRequest(ApiResponse<object>.Fail(result.Error!.Code, result.Error.Message));
        }

        var response = result.Data!.ToResponse();
        return CreatedAtAction(nameof(GetById), new { id = response.StudentId }, ApiResponse<StudentResponse>.Ok(response));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> Update(int id, [FromBody] StudentUpsertRequest request)
    {
        var result = await _students.UpdateAsync(id, new StudentUpsertModel(request.FullName, request.Email, request.DateOfBirth));
        if (!result.Success)
        {
            return result.Error!.Code == "NotFound"
                ? NotFound(ApiResponse<object>.Fail(result.Error.Code, result.Error.Message))
                : BadRequest(ApiResponse<object>.Fail(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<StudentResponse>.Ok(result.Data!.ToResponse()));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var result = await _students.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(ApiResponse<object>.Fail(result.Error!.Code, result.Error.Message));
        }

        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }
}
