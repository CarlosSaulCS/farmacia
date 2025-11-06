using System;
using System.Globalization;
using System.Windows.Data;

namespace Farmacia.UI.Wpf.Converters;

public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool flag)
        {
            return !flag;
        }

        return System.Windows.Data.Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool flag)
        {
            return !flag;
        }

        return System.Windows.Data.Binding.DoNothing;
    }
}
