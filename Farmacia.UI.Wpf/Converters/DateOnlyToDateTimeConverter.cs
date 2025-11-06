using System;
using System.Globalization;
using System.Windows.Data;

namespace Farmacia.UI.Wpf.Converters;

public class DateOnlyToDateTimeConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is DateOnly date
            ? date.ToDateTime(TimeOnly.MinValue)
            : null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime);
        }

    return System.Windows.Data.Binding.DoNothing;
    }
}
