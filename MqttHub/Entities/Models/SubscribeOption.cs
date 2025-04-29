// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MQTTnet.Protocol;

namespace MqttHub.Entities.Models;  
public class SubscribeOption
{
    public string Group { get; set; }
    public string Topic { get; set; }
    public bool NoLocal { get; set; }
    public bool RetainAsPublished { get; set; }
    public MqttQualityOfServiceLevel QoS { get; set; }
    public SubscribeOption(string group, string topic, bool noLocal, MqttQualityOfServiceLevel qos, bool retainAsPublished)
    {
        Group = group;
        Topic = topic;
        NoLocal = noLocal;
        QoS = qos;
        RetainAsPublished = retainAsPublished;
    }
}
