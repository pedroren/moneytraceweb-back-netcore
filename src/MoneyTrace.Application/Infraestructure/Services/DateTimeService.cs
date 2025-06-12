using MoneyTrace.Application.Domain;

namespace MoneyTrace.Application.Infraestructure.Services;

/// <summary>
/// Date handling logic
/// </summary>
public class DateTimeService
{
    public DateTime Now => DateTime.Now;

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime Today => DateTime.Today;

    public DateTimeOffset OffsetNow => DateTimeOffset.Now;

    public DateTimeOffset OffsetUtcNow => DateTimeOffset.UtcNow;
}

public static class DateTimeExtensions{
    public static DateTime NextDueDate(this DateTime date, Frequency frequency)
    {
        return frequency switch
        {
            Frequency.Weekly => date.AddDays(7),
            Frequency.BiWeekly => date.AddDays(14),
            Frequency.Monthly => date.AddMonths(1),
            Frequency.BiMonthly => date.AddMonths(2),
            Frequency.Yearly => date.AddYears(1),
            _ => throw new ArgumentOutOfRangeException(nameof(frequency), frequency, null)
        };
    }
}