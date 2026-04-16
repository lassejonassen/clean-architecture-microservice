namespace CleanArchitecture.WebAPI.Contracts.Templates;

public sealed record TemplateListResponseDTO
{
    public required IEnumerable<TemplateResponseDTO> Data { get; init; }
}
