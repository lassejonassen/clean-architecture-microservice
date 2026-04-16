using CleanArchitecture.Domain.Templates.Errors;
using CleanArchitecture.Domain.Templates.Events;

namespace CleanArchitecture.Domain.Templates.Entities;

public sealed class Template : DomainEntity
{
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 500;

    private Template() { }
    private Template(DateTime utcNow) : base(utcNow) { }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public static Result<Template> Create(string name, string? description, DateTime utcNow)
    {

        var validationResult = ValidateInvariants(name, description);
        if (validationResult.IsFailure)
        {
            return Result.Failure<Template>(validationResult.Error);
        }

        var template = new Template(utcNow)
        {
            Name = name,
            Description = description
        };

        template.RaiseDomainEvent(new TemplateCreatedDomainEvent(template.Id));

        return Result.Success(template);
    }

    public Result Update(string name, string? description)
    {
        var validationResult = ValidateInvariants(name, description);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        Name = name;
        Description = description;

        return Result.Success();
    }

    private static Result ValidateInvariants(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(TemplateErrors.NameIsNullOrWhitespace);

        if (name.Length > NameMaxLength)
            return Result.Failure(TemplateErrors.NameTooLong);

        if (description is not null && description.Length > DescriptionMaxLength)
            return Result.Failure(TemplateErrors.DescriptionTooLong);

        return Result.Success();
    }
}
