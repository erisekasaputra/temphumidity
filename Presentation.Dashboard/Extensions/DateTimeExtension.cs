namespace Presentation.Dashboard.Extensions;

public static class CustomDateTimeExtension
{
    public static DateTime ToUtcFromJakarta(this DateTime date)
    { 
        return date.AddHours(-7);
    }
}