using Microsoft.AspNetCore.Components;
using MqttBrokerWithDashboard.MqttBroker;
using MQTTnet.Server;
using System;

namespace MqttBrokerWithDashboard.Components.Panels
{
    public partial class ClientsPanel : ComponentBase, IDisposable
    {
        [Inject] private MqttBrokerService _mqtt { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _mqtt.OnClientConnected += OnClientConnected;
            _mqtt.OnClientDisconnected += OnClientDisconnected;
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
    }
}