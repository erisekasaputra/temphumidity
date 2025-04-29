using System.Text; 
using MQTTnet;

namespace MqttHub.Applications.Clients;

internal class ClientKerberosAuthenticationHandler : IMqttEnhancedAuthenticationHandler
{
    public async Task HandleEnhancedAuthenticationAsync(MqttEnhancedAuthenticationEventArgs eventArgs)
    {
        if (eventArgs.AuthenticationMethod != "GS2-KRB5")
        {
            throw new InvalidOperationException("Wrong authentication method");
        }

        var sendOptions = new SendMqttEnhancedAuthenticationDataOptions
        {
            Data = "initial context token"u8.ToArray()
        };

        await eventArgs.SendAsync(sendOptions);

        var response = await eventArgs.ReceiveAsync(CancellationToken.None);

        Console.WriteLine($"Received AUTH data from server: {Encoding.UTF8.GetString(response.AuthenticationData)}");

        // No further data is required, but we have to fulfil the exchange.
        sendOptions = new SendMqttEnhancedAuthenticationDataOptions
        {
            Data = []
        };

        await eventArgs.SendAsync(sendOptions, CancellationToken.None);
    }
}
