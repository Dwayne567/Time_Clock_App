namespace Timeclock_WebApplication.Utils
{
    // Utility class for date-related operations
    public static class DateUtils
    {
        // Get the start of the week for a given date
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
