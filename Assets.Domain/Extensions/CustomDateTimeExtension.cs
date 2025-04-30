namespace Assets.Domain.Extensions;

public static class CustomDateTimeExtension
{
    public static DateTime ToUtcFromJakarta(this DateTime date)
    {
        return date.AddHours(-7);
    }

    public static DateTime FromUtcToJakarta(this DateTime date)
    {
        return date.AddHours(7);
    }
}