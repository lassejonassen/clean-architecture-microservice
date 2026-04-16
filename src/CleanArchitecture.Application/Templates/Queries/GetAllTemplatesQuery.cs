using CleanArchitecture.Application.Templates.DTOs;
using CleanArchitecture.Domain.Templates.Repositories;
using CleanArchitecture.SharedKernel.Messaging;

namespace CleanArchitecture.Application.Templates.Queries;

public sealed record GetAllTemplatesQuery : IRequest<IReadOnlyCollection<TemplateDTO>>;

public sealed class GetAllTemplatesQueryHandler(ITemplateRepository templateRepository) 
    : IRequestHandler<GetAllTemplatesQuery, IReadOnlyCollection<TemplateDTO>>
{
    public async Task<IReadOnlyCollection<TemplateDTO>> Handle(GetAllTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = await templateRepository.GetAllAsync(cancellationToken);
        return [.. templates.Select(wg => new TemplateDTO
        {
            Id = wg.Id,
            Name = wg.Name,
            Description = wg.Description,
        })];
    }
}

