using Microsoft.AspNetCore.Components;
using MqttBrokerWithDashboard.MqttBroker;
using MQTTnet.Server;

namespace MqttBrokerWithDashboard.Components
{
    public partial class DashboardPanels : ComponentBase
    {
        [Inject] private MqttBrokerService _mqtt { get; set; }

        private int _numberOfUnseenMessages = 0;

        private bool _isMessagesPanelExpanded;

        private bool IsMessagesPanelExpanded
        {
            get => _isMessagesPanelExpanded;

            set
            {
                if (value)
                    _numberOfUnseenMessages = 0;
                _isMessagesPanelExpanded = value;
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _mqtt.OnClientConnected += OnClientConnected;
            _mqtt.OnClientDisconnected += OnClientDisconnected;
            _mqtt.OnMessageReceived += OnMessageReceived;
        }

        public void Dispose()
        {
            _mqtt.OnClientConnected -= OnClientConnected;
            _mqtt.OnClientDisconnected -= OnClientDisconnected;
            _mqtt.OnMessageReceived -= OnMessageReceived;
        }

        private void OnClientConnected(ClientConnectedEventArgs e) =>
            InvokeAsync(StateHasChanged);

        private void OnClientDisconnected(ClientDisconnectedEventArgs e) =>
            InvokeAsync(StateHasChanged);

        private void OnMessageReceived(InterceptingPublishEventArgs e)
        {
            if (!_isMessagesPanelExpanded)
                _numberOfUnseenMessages++;
            InvokeAsync(StateHasChanged);
        }
    }
}