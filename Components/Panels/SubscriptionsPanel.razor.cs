using Microsoft.AspNetCore.Components;
using MqttBrokerWithDashboard.Services;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using MqttBrokerWithDashboard.Models;

namespace MqttBrokerWithDashboard.Components.Panels
{
    public partial class SubscriptionsPanel : ComponentBase, IDisposable
    {
        [Inject] private MqttBrokerService _mqtt { get; set; }

        private string _searchString = "";

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _mqtt.OnClientSubscribed += OnSubscribeEvent;
            _mqtt.OnClientUnsubscribed += OnUnSubscribeEvent;
        }

        public void Dispose()
        {
            _mqtt.OnClientUnsubscribed -= OnUnSubscribeEvent;
            _mqtt.OnClientSubscribed -= OnSubscribeEvent;
        }

        private void OnSubscribeEvent(ClientSubscribedTopicEventArgs e) =>
            InvokeAsync(StateHasChanged);

        private void OnUnSubscribeEvent(ClientUnsubscribedTopicEventArgs e) =>
            InvokeAsync(StateHasChanged);

        private IEnumerable<Models.MqttSubscriptionModel> GetItems()
        {
            return _mqtt.Subscriptions;
        }

        private bool FilterFunc(Models.MqttSubscriptionModel subscription)
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;
            if (subscription.ClientId.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (subscription.Topic != null && subscription.Topic.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        //private string GetClientId(MqttMessage message) => message?.ClientId ?? "SERVER";
    }
}