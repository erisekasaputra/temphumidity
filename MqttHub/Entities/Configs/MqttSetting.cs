namespace MqttHub.Entities.Configs;

public class MqttSetting
{
    public string BrokerAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; 
    public bool Tls { get; set; }
    public string TlsCertificatePath { get; set; } = string.Empty;
}