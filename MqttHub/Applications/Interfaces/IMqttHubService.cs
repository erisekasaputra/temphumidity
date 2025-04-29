// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Security.Authentication;
using MqttHub.Applications.Services;
using MqttHub.Entities.Models;
using MQTTnet;

namespace MqttHub.Applications.Interfaces;

public interface IMqttHubService
{
    event Func<object, Task>? OnLog;
    event Func<string, string, Task>? OnMessageReceived;   
    void Dispose();
    MqttHubService UseTcpServer(Server server);
    MqttHubService UseWebSocket(Server server);
    Task ConnectAsync();
    MqttHubService UseMQTTv5();
    MqttHubService UseTLS(ICollection<Certificate> certificates, SslProtocols protocol = SslProtocols.Tls12);
    MqttHubService UseServerCA(string certificate);
    MqttHubService Build();
    MqttHubService UseAWS();
    Task DisconnectAsync(MqttClientDisconnectOptionsReason reason = MqttClientDisconnectOptionsReason.NormalDisconnection, string reasonString = "");
    Task PublishAsync(PublishOption topic);
    Task SubscribeAsync(SubscribeOption topic);
    Task PingAsync(); 
}
