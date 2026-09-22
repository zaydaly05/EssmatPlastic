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
        return value switch
        {
            UserRole.Admin or "Admin" => Loc.Current["AdminRole"],
            UserRole.Warehouse or "Warehouse" => Loc.Current["WarehouseRole"],
            UserRole.Accountant or "Accountant" => Loc.Current["AccountantRole"],
            _ => value?.ToString() ?? string.Empty
        };
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

public class UnitFormatConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        var unitType = parameter as string;
        var isArabic = Loc.Current.IsArabic;
        if (value is null)
            return string.Empty;

        if (unitType == "Piece")
        {
            return isArabic ? $"{value:N0} قطعة" : $"{value:N0} pcs";
        }

        if (unitType == "Variant")
        {
            return isArabic ? $"{value} صنف" : $"{value} variants";
        }

        return value.ToString() ?? string.Empty;
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture) =>
        throw new NotSupportedException();
}
