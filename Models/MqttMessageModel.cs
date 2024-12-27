using MQTTnet;
using MQTTnet.Protocol;
using System;

namespace MqttBrokerWithDashboard.Models
{
    public record MqttMessageModel
    {
        public DateTime Timestamp { get; set; }
        public string ClientId { get; set; }
        public string Topic { get; set; }
        public string Payload { get; set; }
        public MqttQualityOfServiceLevel QoS { get; set; }
    }
}