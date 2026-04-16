using CleanArchitecture.Domain.Templates.Entities;

namespace CleanArchitecture.Domain.Templates.Repositories;

public interface ITemplateRepository : IBaseRepository<Template>
{
    Task<IReadOnlyCollection<Template>> GetAllAsync(CancellationToken cancellationToken);
    Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
