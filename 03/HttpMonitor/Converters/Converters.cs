using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace HttpMonitor.Converters;

public class StatusCodeColorConverter : IValueConverter
{
    public static readonly StatusCodeColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int code)
        {
            return code switch
            {
                >= 200 and < 300 => new SolidColorBrush(Color.Parse("#4ADE80")),
                >= 300 and < 400 => new SolidColorBrush(Color.Parse("#FACC15")),
                >= 400 => new SolidColorBrush(Color.Parse("#F87171")),
                0 => new SolidColorBrush(Color.Parse("#94A3B8")),
                _ => new SolidColorBrush(Color.Parse("#94A3B8"))
            };
        }
        return new SolidColorBrush(Color.Parse("#94A3B8"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class DirectionColorConverter : IValueConverter
{
    public static readonly DirectionColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "IN" => new SolidColorBrush(Color.Parse("#818CF8")),
            "OUT" => new SolidColorBrush(Color.Parse("#34D399")),
            "SYS" => new SolidColorBrush(Color.Parse("#FB923C")),
            _ => new SolidColorBrush(Color.Parse("#94A3B8"))
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class MethodColorConverter : IValueConverter
{
    public static readonly MethodColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "GET" => new SolidColorBrush(Color.Parse("#60A5FA")),
            "POST" => new SolidColorBrush(Color.Parse("#A78BFA")),
            "SYS" => new SolidColorBrush(Color.Parse("#FB923C")),
            _ => new SolidColorBrush(Color.Parse("#94A3B8"))
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToStringConverter : IValueConverter
{
    public string TrueValue { get; set; } = "";
    public string FalseValue { get; set; } = "";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? TrueValue : FalseValue;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToColorConverter : IValueConverter
{
    public static readonly BoolToColorConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            var colorStr = b ? "#4ADE80" : "#F87171";
            return new SolidColorBrush(Color.Parse(colorStr));
        }
        return new SolidColorBrush(Color.Parse("#94A3B8"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
