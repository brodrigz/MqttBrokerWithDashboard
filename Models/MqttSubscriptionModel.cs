using MQTTnet.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;

namespace MqttBrokerWithDashboard.Models
{
    public record MqttSubscriptionModel
    {
        public DateTime TimeStamp { get; set; }
        public string ClientId { get; set; }
        public string Topic { get; set; }
        public MqttQualityOfServiceLevel? QoS { get; set; }
        public bool Unsubscribe { get; set; }
        public string Action => Unsubscribe ? "Unsubscribe" : "Subscribe";
    }
}
