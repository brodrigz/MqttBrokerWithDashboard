using Microsoft.AspNetCore.Components;
using MqttBrokerWithDashboard.MqttBroker;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MqttBrokerWithDashboard.Components.Panels
{
    public partial class MessagesPanel : ComponentBase, IDisposable
    {
        [Inject] private MqttBrokerService _mqtt { get; set; }

        private string _searchString = "";

        private bool _collapseByTopic = true;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _mqtt.OnMessageReceived += OnMessageReceived;
        }

        public void Dispose()
        {
            _mqtt.OnMessageReceived -= OnMessageReceived;
        }

        private void OnMessageReceived(InterceptingPublishEventArgs e) =>
            InvokeAsync(StateHasChanged);

        private IEnumerable<MqttMessage> GetItems()
        {
            if (_collapseByTopic)
                return _mqtt.MessagesByTopic.Values.Select(x => x[0]);
            return _mqtt.Messages;
        }

        private bool FilterFunc(MqttMessage message)
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;
            if (message.Client != null && message.Client.ClientId.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (message.Topic != null && message.Topic.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (message.Payload != null && message.Payload.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        private string GetClientId(MqttMessage message) => message.Client?.ClientId ?? "SERVER";
    }
}