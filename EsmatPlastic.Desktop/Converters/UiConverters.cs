using System.Globalization;
using System.Windows.Data;
using EsmatPlastic.Desktop.Models.Users;
using EsmatPlastic.Desktop.Services.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Converters;

public static class Loc
{
    public static LocalizationService Current =>
        App.ServiceProvider.GetRequiredService<LocalizationService>();
}

public class TranslateConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        if (value is null)
            return string.Empty;

        var str = value.ToString();
        if (string.IsNullOrWhiteSpace(str))
            return string.Empty;

        return Loc.Current.Translate(str);
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) =>
        throw new NotSupportedException();
}

public class BoolToStatusConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        var isActive = value is true;
        return isActive
            ? Loc.Current["Active"]
            : Loc.Current["Inactive"];
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) =>
        throw new NotSupportedException();
}

public class RoleToTextConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        var roleStr = value switch
        {
            UserRole.Admin or "Admin" => "AdminRole",
            UserRole.Warehouse or "Warehouse" => "WarehouseRole",
            UserRole.Accountant or "Accountant" => "AccountantRole",
            _ => value?.ToString() ?? string.Empty
        };

        return Loc.Current[roleStr];
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) =>
        throw new NotSupportedException();
}

public class StockTypeToTextConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        var type = value?.ToString() ?? string.Empty;

        return type.Equals("In", StringComparison.OrdinalIgnoreCase)
            ? Loc.Current["Incoming"]
            : Loc.Current["Outgoing"];
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) =>
        throw new NotSupportedException();
}
