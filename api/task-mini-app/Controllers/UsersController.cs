using Microsoft.AspNetCore.Mvc;
using TaskApi.Common;
using TaskApi.Dtos;
using TaskApi.Services;

namespace TaskApi.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly UserService _service;
    public UsersController(UserService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> Get()
    {
        var res = await _service.GetUsers();
        return Ok(res);
    }
}
