using Microsoft.AspNetCore.Components;
using MqttBrokerWithDashboard.Services;
using System.Threading.Tasks;
using MQTTnet.Protocol;

namespace MqttBrokerWithDashboard.Components.Panels
{
    public partial class PublishPanel : ComponentBase
    {
        [Inject] private MqttBrokerService _mqtt { get; set; }

        private string _topic = "MyTopic";

        private string _payload = "MyPayload";

        private bool _retained;

        private MqttQualityOfServiceLevel _qos;

        private bool IsPublishDisabled => string.IsNullOrWhiteSpace(_topic) || string.IsNullOrWhiteSpace(_payload);

        private async Task Publish() => await _mqtt.Publish(_topic, _payload, _retained, _qos);
    }
}