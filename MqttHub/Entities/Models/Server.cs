// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MqttHub.Entities.Models;

public class Server
{
    public string Address { get; set; }
    public int Port { get; set; }
    public string ClientId { get; set; }

    /// <summary>
    /// If [clientId] is not set, then default value is unique Guid
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    /// <param name="clientId"></param>
    public Server(string address, int port, string clientId = "")
    {
        Address = address;
        Port = port;
        ClientId = string.IsNullOrEmpty(clientId) ? Guid.NewGuid().ToString() : clientId;
    }
}
