using Microsoft.EntityFrameworkCore;
using TaskApi.Common;
using TaskApi.Data;
using TaskApi.Dtos;
using TaskApi.Models;

namespace TaskApi.Services;

public class TaskService
{
    private readonly AppDbContext _db;

    public TaskService(AppDbContext db) => _db = db;

    private static readonly HashSet<string> AllowedStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "todo", "doing", "done" };

    private static bool IsValidStatus(string s) => AllowedStatuses.Contains(s);
    private static string NormalizeStatus(string s) => s.Trim().ToLowerInvariant();

    // GET /tasks?status=&assignee=&q=
    public async Task<ApiResponse<List<TaskDto>>> GetTasks(string? status, int? assignee, string? q)
    {
        var query = _db.Tasks.AsNoTracking()
            .Include(t => t.AssigneeUser)
            .AsQueryable();

        // status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = NormalizeStatus(status);
            if (!IsValidStatus(normalizedStatus))
                return ApiResponse<List<TaskDto>>.Fail("VALIDATION_ERROR", "status must be todo/doing/done");

            query = query.Where(t => t.Status == normalizedStatus);
        }

        // assignee filter
        if (assignee.HasValue)
            query = query.Where(t => t.AssigneeUserId == assignee.Value);

        // q search (title OR description) - supports partial match
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();

            // LIKE: %term%
            var like = $"%{term}%";

            query = query.Where(t =>
                EF.Functions.Like(t.Title, like) ||
                (t.Description != null && EF.Functions.Like(t.Description, like))
            );
        }

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TaskDto(
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.AssigneeUserId,
                t.AssigneeUser != null ? t.AssigneeUser.FullName : null,
                t.DueDate,
                t.CreatedAt
            ))
            .ToListAsync();

        return ApiResponse<List<TaskDto>>.Ok(items);
    }

    // POST /tasks
    public async Task<ApiResponse<TaskDto>> Create(TaskCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return ApiResponse<TaskDto>.Fail("VALIDATION_ERROR", "title is required");

        var normalizedStatus = NormalizeStatus(dto.Status);
        if (!IsValidStatus(normalizedStatus))
            return ApiResponse<TaskDto>.Fail("VALIDATION_ERROR", "status must be todo/doing/done");

        if (dto.AssigneeUserId.HasValue)
        {
            var userExists = await _db.Users.AnyAsync(u => u.Id == dto.AssigneeUserId.Value);
            if (!userExists)
                return ApiResponse<TaskDto>.Fail("VALIDATION_ERROR", "assignee_user_id invalid");
        }

        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description,
            Status = normalizedStatus,
            AssigneeUserId = dto.AssigneeUserId,
            DueDate = dto.DueDate
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        _db.TaskLogs.Add(new TaskLog
        {
            TaskId = task.Id,
            Action = "created",
            OldValue = null,
            NewValue = $"status={task.Status}"
        });
        await _db.SaveChangesAsync();

        var assigneeName = task.AssigneeUserId.HasValue
            ? await _db.Users.Where(u => u.Id == task.AssigneeUserId.Value).Select(u => u.FullName).FirstOrDefaultAsync()
            : null;

        return ApiResponse<TaskDto>.Ok(new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.AssigneeUserId,
            assigneeName,
            task.DueDate,
            task.CreatedAt
        ));
    }

    // PUT /tasks/{id}
    public async Task<ApiResponse<TaskDto>> Update(int id, TaskUpdateDto dto)
    {
        var task = await _db.Tasks.Include(t => t.AssigneeUser).FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return ApiResponse<TaskDto>.Fail("NOT_FOUND", "task not found");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return ApiResponse<TaskDto>.Fail("VALIDATION_ERROR", "title is required");

        var normalizedStatus = NormalizeStatus(dto.Status);
        if (!IsValidStatus(normalizedStatus))
            return ApiResponse<TaskDto>.Fail("VALIDATION_ERROR", "status must be todo/doing/done");

        if (dto.AssigneeUserId.HasValue)
        {
            var userExists = await _db.Users.AnyAsync(u => u.Id == dto.AssigneeUserId.Value);
            if (!userExists)
                return ApiResponse<TaskDto>.Fail("VALIDATION_ERROR", "assignee_user_id invalid");
        }

        var oldStatus = task.Status;
        var oldSnapshot = $"title={task.Title};desc={task.Description};assignee={task.AssigneeUserId};due={task.DueDate:O};status={task.Status}";

        task.Title = dto.Title.Trim();
        task.Description = dto.Description;
        task.Status = normalizedStatus;
        task.AssigneeUserId = dto.AssigneeUserId;
        task.DueDate = dto.DueDate;

        await _db.SaveChangesAsync();

        // Status değiştiyse özellikle status_changed log
        if (!string.Equals(oldStatus, task.Status, StringComparison.OrdinalIgnoreCase))
        {
            _db.TaskLogs.Add(new TaskLog
            {
                TaskId = task.Id,
                Action = "status_changed",
                OldValue = oldStatus,
                NewValue = task.Status
            });
        }
        else
        {
            _db.TaskLogs.Add(new TaskLog
            {
                TaskId = task.Id,
                Action = "updated",
                OldValue = oldSnapshot,
                NewValue = $"title={task.Title};desc={task.Description};assignee={task.AssigneeUserId};due={task.DueDate:O};status={task.Status}"
            });
        }

        await _db.SaveChangesAsync();

        var assigneeName = task.AssigneeUserId.HasValue
            ? await _db.Users.Where(u => u.Id == task.AssigneeUserId.Value).Select(u => u.FullName).FirstOrDefaultAsync()
            : null;

        return ApiResponse<TaskDto>.Ok(new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.AssigneeUserId,
            assigneeName,
            task.DueDate,
            task.CreatedAt
        ));
    }

    // PATCH /tasks/{id}/status
    public async Task<ApiResponse<TaskDto>> UpdateStatus(int id, TaskStatusUpdateDto dto)
    {
        var task = await _db.Tasks.Include(t => t.AssigneeUser).FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return ApiResponse<TaskDto>.Fail("NOT_FOUND", "task not found");

        var newStatus = NormalizeStatus(dto.Status);
        if (!IsValidStatus(newStatus))
            return ApiResponse<TaskDto>.Fail("VALIDATION_ERROR", "status must be todo/doing/done");

        var oldStatus = task.Status;

        // idempotent: aynı status ise 200 dön, log yazma
        if (string.Equals(oldStatus, newStatus, StringComparison.OrdinalIgnoreCase))
        {
            var assigneeSame = task.AssigneeUser != null ? task.AssigneeUser.FullName : null;
            return ApiResponse<TaskDto>.Ok(new TaskDto(
                task.Id,
                task.Title,
                task.Description,
                task.Status,
                task.AssigneeUserId,
                assigneeSame,
                task.DueDate,
                task.CreatedAt
            ));
        }

        task.Status = newStatus;
        await _db.SaveChangesAsync();

        _db.TaskLogs.Add(new TaskLog
        {
            TaskId = task.Id,
            Action = "status_changed",
            OldValue = oldStatus,
            NewValue = newStatus
        });
        await _db.SaveChangesAsync();

        var assigneeName = task.AssigneeUser != null ? task.AssigneeUser.FullName : null;

        return ApiResponse<TaskDto>.Ok(new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.AssigneeUserId,
            assigneeName,
            task.DueDate,
            task.CreatedAt
        ));
    }

    //BONUS: KULLANICI BAZLI AÇIK GÖREV SAYISI
    public async Task<List<UserOpenTaskCountDto>> GetOpenTaskCountByUser()
    {
        // 1) DB'den sadece open task sayıları
        var openCounts = await _db.Tasks
            .AsNoTracking()
            .Where(t =>
                t.AssigneeUserId != null &&
                (t.Status == "todo" || t.Status == "doing") 
            )
            .GroupBy(t => t.AssigneeUserId!.Value)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync();

        var countMap = openCounts.ToDictionary(x => x.UserId, x => x.Count);

        // 2) Users'ı çek
        var users = await _db.Users
            .AsNoTracking()
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync();

        // 3) Join
        var result = users
            .Select(u => new UserOpenTaskCountDto(
                u.Id,
                u.FullName,
                countMap.TryGetValue(u.Id, out var c) ? c : 0
            ))
            .OrderByDescending(x => x.OpenTaskCount)
            .ToList();

        return result;
    }
}