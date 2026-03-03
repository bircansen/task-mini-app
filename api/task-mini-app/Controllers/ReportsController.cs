using Microsoft.AspNetCore.Mvc;
using TaskApi.Common;
using TaskApi.Dtos;
using TaskApi.Services;

namespace TaskApi.Controllers;

[ApiController]
[Route("reports")]
public class ReportsController : ControllerBase
{
    private readonly TaskService _service;

    public ReportsController(TaskService service)
    {
        _service = service;
    }

    [HttpGet("open-tasks-by-user")]
    public async Task<ActionResult<ApiResponse<List<UserOpenTaskCountDto>>>> GetOpenTasksByUser()
    {
        var data = await _service.GetOpenTaskCountByUser();
        return Ok(ApiResponse<List<UserOpenTaskCountDto>>.Ok(data));
    }
}