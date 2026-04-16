using CleanArchitecture.SharedKernel;

namespace CleanArchitecture.Infrastructure;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}