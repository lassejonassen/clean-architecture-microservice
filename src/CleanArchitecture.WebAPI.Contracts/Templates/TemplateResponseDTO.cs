namespace CleanArchitecture.WebAPI.Contracts.Templates;

public sealed record TemplateResponseDTO
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
}
