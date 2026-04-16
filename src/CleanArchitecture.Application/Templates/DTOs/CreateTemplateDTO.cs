namespace CleanArchitecture.Application.Templates.DTOs;

public sealed record CreateTemplateDTO
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}
