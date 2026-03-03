namespace TaskApi.Dtos;

public record TaskDto(
    int Id,
    string Title,
    string? Description,
    string Status,
    int? AssigneeUserId,
    string? AssigneeFullName,
    DateTime? DueDate,
    DateTime CreatedAt
);

public record TaskCreateDto(
    string Title,
    string? Description,
    string Status,
    int? AssigneeUserId,
    DateTime? DueDate
);

public record TaskUpdateDto(
    string Title,
    string? Description,
    string Status,
    int? AssigneeUserId,
    DateTime? DueDate
);

public record TaskStatusUpdateDto(string Status);