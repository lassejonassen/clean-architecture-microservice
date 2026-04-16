namespace CleanArchitecture.Application.Templates.DTOs;

public sealed record TemplateDTO
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}
