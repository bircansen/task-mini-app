namespace TaskApi.Dtos;

public record UserOpenTaskCountDto(
    int UserId,
    string FullName,
    int OpenTaskCount
);