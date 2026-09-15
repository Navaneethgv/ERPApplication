using System;

namespace ERP.Domain.Common;

/// <summary>
/// Centralized Time and TimeZone Provider ensuring Indian Standard Time (IST, UTC+05:30)
/// is used consistently across database persistence, business logic, APIs, and client views.
/// </summary>
public static class TimeHelper
{
    private static readonly TimeZoneInfo _istZone = ResolveIstTimeZone();

    private static TimeZoneInfo ResolveIstTimeZone()
    {
        // 1. Try standard Windows time zone ID
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        }
        catch { }

        // 2. Try IANA time zone ID (Linux / container / macOS)
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
        }
        catch { }

        // 3. Robust fallback to UTC+05:30 custom time zone
        return TimeZoneInfo.CreateCustomTimeZone("IST", TimeSpan.FromHours(5.5), "India Standard Time", "IST");
    }

    /// <summary>
    /// Indian Standard Time (UTC+05:30) TimeZoneInfo instance
    /// </summary>
    public static TimeZoneInfo IstZone => _istZone;

    /// <summary>
    /// Gets the current date and time in Indian Standard Time (IST).
    /// </summary>
    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _istZone);

    /// <summary>
    /// Gets today's date at 00:00:00 in Indian Standard Time (IST).
    /// </summary>
    public static DateTime Today => Now.Date;

    /// <summary>
    /// Converts a DateTime to IST cleanly without double conversion.
    /// </summary>
    public static DateTime ToIst(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, _istZone);
        }
        return dateTime;
    }

    /// <summary>
    /// Formats a DateTime to a standard ISO-like string for Excel and API storage (yyyy-MM-dd HH:mm:ss)
    /// </summary>
    public static string FormatStorage(DateTime dt)
    {
        return dt.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

