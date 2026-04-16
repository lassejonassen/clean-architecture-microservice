using CleanArchitecture.Application.Templates.DTOs;
using CleanArchitecture.Domain.Templates.Errors;
using CleanArchitecture.Domain.Templates.Repositories;
using CleanArchitecture.SharedKernel;
using CleanArchitecture.SharedKernel.Messaging;

namespace CleanArchitecture.Application.Templates.Queries;

public sealed record GetTemplateByIdQuery(Guid Id) : IRequest<Result<TemplateDTO>>;

public sealed class GetTemplateByIdQueryHandler(ITemplateRepository templateRepository) 
    : IRequestHandler<GetTemplateByIdQuery, Result<TemplateDTO>>
{
    public async Task<Result<TemplateDTO>> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await templateRepository.GetByIdAsync(request.Id, cancellationToken);
        if (template is null)
            return Result.Failure<TemplateDTO>(TemplateErrors.NotFound);

        var templateDTO = new TemplateDTO
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
        };

        return Result.Success(templateDTO);
    }
}
