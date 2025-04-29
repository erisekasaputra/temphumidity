namespace StreamGate.Worker.Configuration;

public class MqttOption 
{
    public const string Section = "MQTT";
    public string BrokerAddress { get; set; } = string.Empty;
    public int Port { get; set; } 
    public string ClientId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsUsingTls { get; set; }
    public string TlsCertificatePath { get; set; } = string.Empty;
}