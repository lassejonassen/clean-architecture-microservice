using CleanArchitecture.Domain.Templates.Entities;

namespace CleanArchitecture.Domain.Templates.Errors;

public static class TemplateErrors
{
    private const string Prefix = nameof(Template);

    public static readonly Error NameIsNullOrWhitespace
        = new($"{Prefix}.NameIsNullOrWhitespace", "The name provided is null or whitespace.", ErrorType.Failure);

    public static readonly Error NameTooLong
        = new($"{Prefix}.NameTooLong", "The name provided is too long.", ErrorType.Failure);

    public static readonly Error DescriptionTooLong
        = new($"{Prefix}.DescriptionTooLong", "The description provided is too long.", ErrorType.Failure);

    public static readonly Error NotFound
        = new($"{Prefix}.NotFound", "The template with the specified ID was not found.", ErrorType.Failure);
}
