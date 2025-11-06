using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Farmacia.Data.Contexts;

internal static class DateOnlyConverters
{
    internal sealed class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
    {
        public DateOnlyConverter() : base(date => date.ToDateTime(TimeOnly.MinValue), date => DateOnly.FromDateTime(date))
        {
        }
    }

    internal sealed class TimeOnlyConverter : ValueConverter<TimeOnly, TimeSpan>
    {
        public TimeOnlyConverter() : base(time => time.ToTimeSpan(), span => TimeOnly.FromTimeSpan(span))
        {
        }
    }

    internal sealed class DateOnlyComparer : ValueComparer<DateOnly>
    {
        public static DateOnlyComparer Instance { get; } = new();

        private DateOnlyComparer() : base((a, b) => a.DayNumber == b.DayNumber, date => date.GetHashCode())
        {
        }
    }

    internal sealed class TimeOnlyComparer : ValueComparer<TimeOnly>
    {
        public static TimeOnlyComparer Instance { get; } = new();

        private TimeOnlyComparer() : base((a, b) => a.Ticks == b.Ticks, time => time.GetHashCode())
        {
        }
    }
}
