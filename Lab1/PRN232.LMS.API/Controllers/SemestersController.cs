using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Infrastructure;
using PRN232.LMS.API.Models;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/semesters")]
public sealed class SemestersController : ControllerBase
{
    private static readonly HashSet<string> AllowedSort = new(StringComparer.OrdinalIgnoreCase)
    {
        "semesterId", "semesterName", "startDate", "endDate"
    };

    private static readonly HashSet<string> AllowedExpand = new(StringComparer.OrdinalIgnoreCase)
    {
        "courses"
    };

    private readonly ISemesterService _semesters;

    public SemestersController(ISemesterService semesters)
    {
        _semesters = semesters;
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

        var result = await _semesters.GetListAsync(query);
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
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> GetById(int id)
    {
        var semester = await _semesters.GetByIdAsync(id);
        if (semester is null)
        {
            return NotFound(ApiResponse<object>.Fail("NotFound", $"Semester {id} not found"));
        }

        return Ok(ApiResponse<SemesterResponse>.Ok(semester.ToResponse()));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> Create([FromBody] SemesterUpsertRequest request)
    {
        var result = await _semesters.CreateAsync(new SemesterUpsertModel(request.SemesterName, request.StartDate, request.EndDate));
        if (!result.Success)
        {
            return BadRequest(ApiResponse<object>.Fail(result.Error!.Code, result.Error.Message));
        }

        var response = result.Data!.ToResponse();
        return CreatedAtAction(nameof(GetById), new { id = response.SemesterId }, ApiResponse<SemesterResponse>.Ok(response));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> Update(int id, [FromBody] SemesterUpsertRequest request)
    {
        var result = await _semesters.UpdateAsync(id, new SemesterUpsertModel(request.SemesterName, request.StartDate, request.EndDate));
        if (!result.Success)
        {
            return result.Error!.Code == "NotFound"
                ? NotFound(ApiResponse<object>.Fail(result.Error.Code, result.Error.Message))
                : BadRequest(ApiResponse<object>.Fail(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<SemesterResponse>.Ok(result.Data!.ToResponse()));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var result = await _semesters.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(ApiResponse<object>.Fail(result.Error!.Code, result.Error.Message));
        }

        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }
}
