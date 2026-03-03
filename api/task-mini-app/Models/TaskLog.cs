namespace TaskApi.Models;

public class TaskLog
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string Action { get; set; } = default!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TaskItem Task { get; set; } = default!;
}