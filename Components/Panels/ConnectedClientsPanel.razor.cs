using Microsoft.AspNetCore.Components;
using MqttBrokerWithDashboard.Services;
using MqttBrokerWithDashboard.Models;
using MQTTnet.Server;
using System;

namespace MqttBrokerWithDashboard.Components.Panels
{
    public partial class ConnectedClientsPanel : ComponentBase, IDisposable
    {
        [Inject] private MqttBrokerService _mqtt { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _mqtt.OnClientConnected += OnClientConnected;
            _mqtt.OnClientDisconnected += OnClientDisconnected;
            _mqtt.OnMessageReceived += OnMessageReceived;
            _mqtt.OnClientSubscribed += OnClientSubscribed;
            _mqtt.OnClientUnsubscribed += OnClientUnsubscribed;
        }

        public void Dispose()
        {
            _mqtt.OnClientConnected -= OnClientConnected;
            _mqtt.OnClientDisconnected -= OnClientDisconnected;
        }

        private void OnClientConnected(ClientConnectedEventArgs e) =>
            InvokeAsync(StateHasChanged);

        private void OnClientDisconnected(ClientDisconnectedEventArgs e) =>
            InvokeAsync(StateHasChanged);

        private void OnMessageReceived(InterceptingPublishEventArgs e) =>
            InvokeAsync(StateHasChanged);

        private void OnClientSubscribed(ClientSubscribedTopicEventArgs e) =>
             InvokeAsync(StateHasChanged);

        private void OnClientUnsubscribed(ClientUnsubscribedTopicEventArgs e) =>
             InvokeAsync(StateHasChanged);
    }
}