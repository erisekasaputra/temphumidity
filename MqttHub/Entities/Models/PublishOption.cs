using MqttHub.Applications.Services;
using MQTTnet.Protocol;

namespace MqttHub.Entities.Models; 

public class PublishOption
{
    public string Topic { get; set; }
    public MqttQualityOfServiceLevel QoS { get; set; }
    public string Payload { get; set; }
    public PayloadContentType PayloadContentType { get; set; }
    public bool RetainFlag { get; set; }
    public PublishOption(string topic, MqttQualityOfServiceLevel qos, string payload, PayloadContentType payloadContentType, bool retainFlag = true)
    {
        Topic = topic;
        QoS = qos;
        Payload = payload;
        PayloadContentType = payloadContentType;
        RetainFlag = retainFlag;    
    }
}
