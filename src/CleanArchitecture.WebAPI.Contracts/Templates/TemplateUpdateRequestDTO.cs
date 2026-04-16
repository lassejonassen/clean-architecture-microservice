namespace CleanArchitecture.WebAPI.Contracts.Templates;

public sealed record TemplateUpdateRequestDTO
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}
