using ClosedXML.Excel;
using System.Globalization;


namespace ERP.Infrastructure.Repositories;

public static class ExcelHelper
{
    public static string GetString(IXLCell cell, string defaultValue = "")
    {
        if (cell.IsEmpty()) return defaultValue;
        var val = cell.GetString()?.Trim();
        return string.IsNullOrEmpty(val) ? defaultValue : val;
    }

    public static int GetInt(IXLCell cell, int defaultValue = 0)
    {
        if (cell.IsEmpty()) return defaultValue;
        if (cell.TryGetValue(out int intVal)) return intVal;
        if (int.TryParse(cell.GetString(), out int parsed)) return parsed;
        return defaultValue;
    }

    public static int? GetNullableInt(IXLCell cell)
    {
        if (cell.IsEmpty()) return null;
        if (cell.TryGetValue(out int intVal)) return intVal;
        if (int.TryParse(cell.GetString(), out int parsed)) return parsed;
        return null;
    }

    public static decimal GetDecimal(IXLCell cell, decimal defaultValue = 0m)
    {
        if (cell.IsEmpty()) return defaultValue;
        if (cell.TryGetValue(out decimal decVal)) return decVal;
        if (decimal.TryParse(cell.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsed)) return parsed;
        return defaultValue;
    }

    public static DateTime GetDateTime(IXLCell cell, DateTime? defaultValue = null)
    {
        if (cell.IsEmpty()) return defaultValue ?? TimeHelper.Now;
        if (cell.TryGetValue(out DateTime dateVal)) return TimeHelper.ToIst(dateVal);
        if (DateTime.TryParse(cell.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed)) return TimeHelper.ToIst(parsed);
        if (DateTime.TryParse(cell.GetString(), out DateTime parsedLocal)) return TimeHelper.ToIst(parsedLocal);
        return defaultValue ?? TimeHelper.Now;
    }

    public static DateTime? GetNullableDateTime(IXLCell cell)
    {
        if (cell.IsEmpty()) return null;
        if (cell.TryGetValue(out DateTime dateVal)) return TimeHelper.ToIst(dateVal);
        if (DateTime.TryParse(cell.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed)) return TimeHelper.ToIst(parsed);
        if (DateTime.TryParse(cell.GetString(), out DateTime parsedLocal)) return TimeHelper.ToIst(parsedLocal);
        return null;
    }
}


