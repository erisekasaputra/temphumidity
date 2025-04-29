// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MqttHub.Entities.Models;

public class Certificate
{
    public string Path { get; set; }
    public string Password { get; set; }

    public Certificate(string path, string password)
    {
        Path = path;
        Password = password;
    }
}
