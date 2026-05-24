using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Diplom
{
    public class BoolToYesNoConverter : IValueConverter
    {
        public static readonly BoolToYesNoConverter Instance = new BoolToYesNoConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Активна" : "Неактивна";
            }
            return "Неизвестно";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NotNullToBoolConverter : IValueConverter
    {
        public static readonly NotNullToBoolConverter Instance = new NotNullToBoolConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            if (value is string str)
                return !string.IsNullOrWhiteSpace(str);

            return true;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBoolConverter : IValueConverter
    {
        public static readonly InverseBoolConverter Instance = new InverseBoolConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}