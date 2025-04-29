namespace Presentation.Dashboard.SeedWorks;

public static class DateTimeConverter
{
    public static string ToJakarta(this DateTime dateTime, string format)   
    {
        TimeZoneInfo jakartaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        DateTime jakartaTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, jakartaTimeZone);

        return jakartaTime.ToString(format);
    } 

    public static DateTime CeilSeconds(this DateTime dateTime)
    {
        return dateTime.Second > 0 && dateTime.Millisecond > 0 ? dateTime.AddSeconds(1).AddMilliseconds(-dateTime.Millisecond) : dateTime;
    }
}