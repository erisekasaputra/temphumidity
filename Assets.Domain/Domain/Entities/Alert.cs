using Assets.Domain.Domain.Enums;

namespace Assets.Domain.Domain.Entities;

public class Alert
{
    public static Alert Normal() => new (AlertType.Normal, string.Empty); 
    public static Alert UCLExceeded(string message) => new (AlertType.UCLExceeded, message); 
    public static Alert LCLExceeded(string message) => new (AlertType.LCLExceeded, message); 
    public static Alert UCLApproachingWarning(string message) => new (AlertType.UCLApproachingWarning, message); 
    public static Alert LCLApproachingWarning(string message) => new (AlertType.LCLApproachingWarning, message); 

    public AlertType Type { get; set; }
    public string Message { get; set; } = string.Empty;

    public Alert()
    {
        
    }

    private Alert(AlertType type, string message)
    {
        Type = type;
        Message = message;
    }
}