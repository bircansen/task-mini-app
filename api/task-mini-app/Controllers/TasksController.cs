using Microsoft.AspNetCore.Mvc;
using TaskApi.Common;
using TaskApi.Dtos;
using TaskApi.Services;

namespace TaskApi.Controllers;

[ApiController]
[Route("tasks")]
public class TasksController : ControllerBase
{
    private readonly TaskService _service;

    public TasksController(TaskService service) => _service = service;

    // GET /tasks?status=&assignee=&q=&page=&pageSize=
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TaskDto>>>> Get(
       [FromQuery] string? status,
       [FromQuery] int? assignee,
       [FromQuery] string? q
   )
    {
        var res = await _service.GetTasks(status, assignee, q);

        if (!res.Success)
            return BadRequest(res);

        return Ok(res);
    }

    // POST /tasks
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TaskDto>>> Create([FromBody] TaskCreateDto dto)
    {
        var res = await _service.Create(dto);

        if (!res.Success)
            return BadRequest(res);

        // İstersen 201 de dönebilirsin:
        // return CreatedAtAction(nameof(Get), new { id = res.Data!.Id }, res);
        return Ok(res);
    }

    // PUT /tasks/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> Update([FromRoute] int id, [FromBody] TaskUpdateDto dto)
    {
        var res = await _service.Update(id, dto);

        if (!res.Success)
        {
            if (res.Error?.Code == "NOT_FOUND") return NotFound(res);
            return BadRequest(res);
        }

        return Ok(res);
    }

    // PATCH /tasks/{id}/status
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> UpdateStatus([FromRoute] int id, [FromBody] TaskStatusUpdateDto dto)
    {
        var res = await _service.UpdateStatus(id, dto);

        if (!res.Success)
        {
            if (res.Error?.Code == "NOT_FOUND") return NotFound(res);
            return BadRequest(res);
        }

        return Ok(res);
    }
}