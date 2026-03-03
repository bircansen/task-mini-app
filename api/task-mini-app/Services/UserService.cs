using Microsoft.EntityFrameworkCore;
using TaskApi.Common;
using TaskApi.Data;
using TaskApi.Dtos;

namespace TaskApi.Services;

public class UserService
{
    private readonly AppDbContext _db;
    public UserService(AppDbContext db) => _db = db;

    public async Task<ApiResponse<List<UserDto>>> GetUsers()
    {
        var users = await _db.Users.AsNoTracking()
            .OrderBy(u => u.Id)
            .Select(u => new UserDto(u.Id, u.FullName, u.Email))
            .ToListAsync();

        return ApiResponse<List<UserDto>>.Ok(users);
    }
}