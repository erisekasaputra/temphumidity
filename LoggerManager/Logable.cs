namespace LoggerManager;

public class Logable
{
    public event Func<object, Task>? OnLog;

    // Static method to invoke the event
    public async Task LCall<T>(T message)
    {
        if (OnLog == null)
        {
            Console.WriteLine($"No logger callback registered. Log message: {message}");
            return;
        }

        if (message == null)
        {
            return;
        }

        try
        {
            await OnLog.Invoke(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Logging error: {ex.Message}");
        }
    }
}
