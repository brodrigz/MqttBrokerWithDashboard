using Microsoft.AspNetCore.Components;
using MqttBrokerWithDashboard.Services;
using MQTTnet.Server;

namespace MqttBrokerWithDashboard.Components
{
    public partial class DashboardPanels : ComponentBase
    {
        [Inject] private MqttBrokerService _mqtt { get; set; }

        private int _numberOfUnseenMessages = 0;
        private int _numberOfUnseenSubscriptions = 0;

        private bool _isMessagesPanelExpanded;
        private bool _isSubscriptionsPanelExpanded;

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

        private bool IsSubscriptionsPanelExpanded
        {
            get => _isSubscriptionsPanelExpanded;

            set
            {
                if (value)
                    _numberOfUnseenSubscriptions = 0;
                _isSubscriptionsPanelExpanded = value;
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _mqtt.OnClientConnected += OnClientConnected;
            _mqtt.OnClientDisconnected += OnClientDisconnected;
            _mqtt.OnClientSubscribed += OnClientSubscribe;
            _mqtt.OnClientUnsubscribed += OnClientUnsubscribe;
            _mqtt.OnMessageReceived += OnMessageReceived;
        }

        public void Dispose()
        {
            _mqtt.OnClientConnected -= OnClientConnected;
            _mqtt.OnClientDisconnected -= OnClientDisconnected;
            _mqtt.OnClientSubscribed -= OnClientSubscribe;
            _mqtt.OnClientUnsubscribed -= OnClientUnsubscribe;
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

        private void OnClientSubscribe(ClientSubscribedTopicEventArgs e)
        {
            OnSubscriptionEvent();
        }

        private void OnClientUnsubscribe(ClientUnsubscribedTopicEventArgs e)
        {
            OnSubscriptionEvent();
        }

        private void OnSubscriptionEvent()
        {
            if (!_isSubscriptionsPanelExpanded)
                _numberOfUnseenSubscriptions++;
            InvokeAsync(StateHasChanged);
        }
    }
}