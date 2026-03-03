namespace TaskApi.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string Status { get; set; } = "todo"; // todo/doing/done
    public int? AssigneeUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? AssigneeUser { get; set; }
    public List<TaskLog> Logs { get; set; } = new();
}