using Microsoft.AspNetCore.Components;
using MqttBrokerWithDashboard.MqttBroker;

namespace MqttBrokerWithDashboard.Components.Panels
{
    public partial class PublishPanel : ComponentBase
    {
        [Inject] private MqttBrokerService _mqtt { get; set; }

        private string _topic = "MyTopic";

        private string _payload = "MyPayload";

        private bool _retained;

        private bool IsPublishDisabled => string.IsNullOrWhiteSpace(_topic) || string.IsNullOrWhiteSpace(_payload);

        private void Publish() => _mqtt.Publish(_topic, _payload, _retained);
    }
}