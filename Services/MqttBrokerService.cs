using MqttBrokerWithDashboard.Extensions;
using MqttBrokerWithDashboard.Models;
using MqttBrokerWithDashboard.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MqttClientModel = MqttBrokerWithDashboard.Models.MqttClientModel;
using MqttSubscriptionModel = MqttBrokerWithDashboard.Models.MqttSubscriptionModel;

namespace MqttBrokerWithDashboard.Services
{
    public class MqttBrokerService
    {
        private ConcurrentDictionary<Guid, MqttMessageModel> _messageCache = new();
        private ConcurrentDictionary<Guid, MqttSubscriptionModel> _subsCache = new();
        private ConcurrentDictionary<string, MqttClientModel> _clientCache = new();
        private readonly MqttServiceOptions options;
        private readonly ILogger Logger;

        private InjectedMqttApplicationMessage BuildMessage(string topic, string payload, bool retain = false, MqttQualityOfServiceLevel qos = default) => BuildMessage(topic, Encoding.UTF8.GetBytes(payload), retain, qos);

        private InjectedMqttApplicationMessage BuildMessage(string topic, byte[] payload, bool retain = false, MqttQualityOfServiceLevel qos = default)
        {
            var msg = new MqttApplicationMessageBuilder()
              .WithTopic(topic)
              .WithPayload(payload)
              .WithQualityOfServiceLevel(qos)
              .WithRetainFlag(retain)
              .Build();

            return new InjectedMqttApplicationMessage(msg)
            {
                SenderClientId = options.ServerId
            };
        }

        public MqttBrokerService(ILogger<MqttBrokerService> log, IOptions<MqttServiceOptions> serviceOptions)
        {
            Logger = log;
            this.options = serviceOptions.Value;
        }

        public MqttServer Server { get; set; }
        public IEnumerable<MqttMessageModel> Messages => _messageCache.Select(x => x.Value);
        public IEnumerable<MqttSubscriptionModel> Subscriptions => _subsCache.Select(x => x.Value);
        public IDictionary<string, IEnumerable<MqttMessageModel>> MessagesByTopic => Messages.GroupBy(x => x.Topic).ToDictionary(k => k.Key, v => v.AsEnumerable());
        public IEnumerable<MqttClientModel> ConnectedClients => _clientCache.Values.Where(x => x.Connected);
        public IEnumerable<MqttClientModel> DisconnectedClients => _clientCache.Values.Where(x => !x.Connected);

        #region Events

        public event Action<ClientConnectedEventArgs> OnClientConnected;
        public event Action<ClientDisconnectedEventArgs> OnClientDisconnected;
        public event Action<InterceptingPublishEventArgs> OnMessageReceived;
        public event Action<ClientSubscribedTopicEventArgs> OnClientSubscribed;
        public event Action<ClientUnsubscribedTopicEventArgs> OnClientUnsubscribed;

        public void BindServer(MqttServer mqttServer)
        {
            Server = mqttServer;
            Server.ClientConnectedAsync += HandleClientConnectedAsync;
            Server.ClientDisconnectedAsync += HandleClientDisconnectedAsync;
            Server.ClientSubscribedTopicAsync += HandleClientSubscribedTopicAsync;
            Server.ClientUnsubscribedTopicAsync += HandleClientUnsubscribedTopicAsync;
            Server.InterceptingPublishAsync += HandleApplicationMessageReceivedAsync;
        }

        private void CacheSubscription(ClientSubscribedTopicEventArgs e)
        {
            var subscription = new Models.MqttSubscriptionModel()
            {
                TimeStamp = DateTime.Now,
                ClientId = e.ClientId,
                Topic = e.TopicFilter.Topic,
                QoS = e.TopicFilter.QualityOfServiceLevel
            };
            if (_subsCache.Count >= options.MaxRecordedSubscriptions)
            {
                _subsCache.TryEvictOldest(x => x.TimeStamp);
            }
            _subsCache.TryAdd(Guid.NewGuid(), subscription);
        }

        private void CacheSubscription(ClientUnsubscribedTopicEventArgs e)
        {
            var subscription = new Models.MqttSubscriptionModel()
            {
                TimeStamp = DateTime.Now,
                ClientId = e.ClientId,
                Topic = e.TopicFilter,
                Unsubscribe = true
            };
            if (_subsCache.Count >= options.MaxRecordedSubscriptions)
            {
                _subsCache.TryEvictOldest(x => x.TimeStamp);
            }
            _subsCache.TryAdd(Guid.NewGuid(), subscription);
        }

        private void AddOrUpdateClient(ClientConnectedEventArgs e)
        {
            var client = _clientCache.GetOrAdd(e.ClientId, _ => new MqttClientModel
            {
                ClientId = e.ClientId,
                AllowReceive = true,
                AllowSend = true
            });
            client.RecordConnect();
        }

        private void AddOrUpdateClient(ClientDisconnectedEventArgs e)
        {
            if (_clientCache.TryGetValue(e.ClientId, out var client))
            {
                client.RecordDisconnect();
            }
            else { Logger.LogWarning("Unmanaged client has disconnected {id}", e.ClientId); }
            if (_clientCache.Count >= options.MaxRecordedDisconnections)
            {
                _clientCache.TryEvictOldest(x => x.TimeOfDisconnection.Value, x => !x.Connected);
            }
        }

        private Task HandleClientConnectedAsync(ClientConnectedEventArgs e)
        {
            Logger.LogInformation($"Client connected: {e.ClientId}");
            AddOrUpdateClient(e);
            OnClientConnected?.Invoke(e);
            return Task.CompletedTask;
        }

        private Task HandleClientDisconnectedAsync(ClientDisconnectedEventArgs e)
        {
            Logger.LogInformation($"Client disconnected: {e.ClientId}");
            AddOrUpdateClient(e);
            OnClientDisconnected?.Invoke(e);
            return Task.CompletedTask;
        }

        private async Task HandleClientSubscribedTopicAsync(ClientSubscribedTopicEventArgs e)
        {
            Logger.LogInformation($"Client {e.ClientId} subscribed to {e.TopicFilter.Topic}");
            if (_clientCache.TryGetValue(e.ClientId, out var client))
            {
                client.RecordSubscribe(e.TopicFilter.Topic);
            }
            CacheSubscription(e);
            OnClientSubscribed?.Invoke(e);
            await Task.CompletedTask;
        }

        private async Task HandleClientUnsubscribedTopicAsync(ClientUnsubscribedTopicEventArgs e)
        {
            Logger.LogInformation($"Client {e.ClientId} unsubscribed from {e.TopicFilter}");
            if (_clientCache.TryGetValue(e.ClientId, out var client))
            {
                client.RecordUnsubscribe(e.TopicFilter);
            }
            CacheSubscription(e);
            OnClientUnsubscribed?.Invoke(e);
            await Task.CompletedTask;
        }

        private Task HandleApplicationMessageReceivedAsync(InterceptingPublishEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = e.ApplicationMessage.ConvertPayloadToString();

            var msg = new MqttMessageModel()
            {
                ClientId = e.ClientId,
                Topic = topic,
                Payload = payload,
                Timestamp = DateTime.Now,
                QoS = e.ApplicationMessage.QualityOfServiceLevel
            };

            if (_messageCache.Count >= options.MaxRecordedMessages)
            {
                _messageCache.TryEvictOldest(x => x.Timestamp);
            }

            _messageCache.TryAdd(Guid.NewGuid(), msg);
            Logger.LogInformation($"OnMessageReceived: {topic} {payload}");

            if (_clientCache.TryGetValue(e.ClientId, out var client))
            {
                client.RecordPublish();
            }

            OnMessageReceived?.Invoke(e);
            return Task.CompletedTask;
        }

        #endregion Events

        public async Task Publish(InjectedMqttApplicationMessage message, CancellationToken token = default) => await Server?.InjectApplicationMessage(message, token);

        public async Task Publish(string topic, byte[] payload, bool retain = false, MqttQualityOfServiceLevel qos = default, CancellationToken token = default) => await Publish(BuildMessage(topic, payload, retain, qos), token);

        public async Task Publish(string topic, string payload, bool retain = false, MqttQualityOfServiceLevel qos = default, CancellationToken token = default) => await Publish(BuildMessage(topic, payload, retain, qos), token);
    }
}