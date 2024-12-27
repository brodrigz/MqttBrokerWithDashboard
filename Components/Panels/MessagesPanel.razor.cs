using Microsoft.AspNetCore.Components;
using MqttBrokerWithDashboard.Services;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using MqttBrokerWithDashboard.Models;

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

        private IEnumerable<MqttMessageModel> GetItems()
        {
            if (_collapseByTopic)
                return _mqtt.MessagesByTopic.Values.Select(g => g.OrderByDescending(x=>x.Timestamp).FirstOrDefault());
            return _mqtt.Messages;
        }

        private bool FilterFunc(MqttMessageModel message)
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;
            if (message.ClientId.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (message.Topic != null && message.Topic.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (message.Payload != null && message.Payload.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        private string GetClientId(MqttMessageModel message) => message?.ClientId ?? "SERVER";
    }
}