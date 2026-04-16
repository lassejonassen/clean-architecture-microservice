namespace CleanArchitecture.WebAPI.Contracts.Templates;

public sealed record TemplateCreateRequestDTO
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}