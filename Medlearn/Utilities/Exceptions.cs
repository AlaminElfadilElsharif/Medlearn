// Utilities/Exceptions.cs
using BCrypt.Net;
using System.Globalization;

namespace Medlearn.Utilities
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }
}

// Utilities/PasswordHasher.cs
namespace BlazorApp1.Utilities
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }

    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
    // Extension method for string truncation
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }
    }

    // Extension method for week number calculation
    public static class DateTimeExtensions
    {
        public static int GetWeekOfYear(this DateTime date)
        {
            var ci = CultureInfo.CurrentCulture;
            var calendar = ci.Calendar;
            var calendarWeekRule = ci.DateTimeFormat.CalendarWeekRule;
            var firstDayOfWeek = ci.DateTimeFormat.FirstDayOfWeek;
            return calendar.GetWeekOfYear(date, calendarWeekRule, firstDayOfWeek);
        }
    }
}
