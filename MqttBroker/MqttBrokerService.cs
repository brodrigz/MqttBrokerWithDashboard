using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MqttBrokerWithDashboard.MqttBroker
{
    public class MqttBrokerService
    {
        /// <summary>
        /// Id that the sever will use when publishing messages directly.
        /// </summary>
        private const string ServerId = "SERVER";

        #region Private

        private readonly object _thisLock = new();

        private List<MqttMessage> _messages = new();

        private readonly ILogger Logger;

        private Dictionary<string, List<MqttMessage>> _messagesByTopic = new();

        private List<MqttClient> _connectedClients = new();

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
                SenderClientId = ServerId
            };
        }

        #endregion Private

        public MqttServer Server { get; set; }

        public IReadOnlyList<MqttMessage> Messages
        {
            get
            {
                lock (_thisLock)
                {
                    return _messages?.AsReadOnly();
                }
            }
        }

        public IReadOnlyDictionary<string, List<MqttMessage>> MessagesByTopic
        {
            get
            {
                lock (_thisLock)
                {
                    return _messagesByTopic as IReadOnlyDictionary<string, List<MqttMessage>>;
                }
            }
        }

        public IReadOnlyList<MqttClient> ConnectedClients
        {
            get
            {
                lock (_thisLock)
                {
                    return _connectedClients?.AsReadOnly();
                }
            }
        }

        public MqttBrokerService(ILogger<MqttBrokerService> log)
        {
            Logger = log;
        }

        #region Events

        public event Action<ClientConnectedEventArgs> OnClientConnected;

        public event Action<ClientDisconnectedEventArgs> OnClientDisconnected;

        public event Action<InterceptingPublishEventArgs> OnMessageReceived;

        public void BindServer(MqttServer mqttServer)
        {
            Server = mqttServer;
            Server.ClientConnectedAsync += HandleClientConnectedAsync;
            Server.ClientDisconnectedAsync += HandleClientDisconnectedAsync;
            Server.InterceptingPublishAsync += HandleApplicationMessageReceivedAsync;
        }

        private Task HandleClientConnectedAsync(ClientConnectedEventArgs e)
        {
            lock (_thisLock)
            {
                _connectedClients.Add(new MqttClient
                {
                    TimeOfConnection = DateTime.Now,
                    ClientId = e.ClientId,
                    AllowSend = true,
                    AllowReceive = true,
                });
            }

            Logger.LogInformation($"Client connected: {e.ClientId}");

            OnClientConnected?.Invoke(e);
            return Task.CompletedTask;
        }

        private Task HandleClientDisconnectedAsync(ClientDisconnectedEventArgs e)
        {
            lock (_thisLock)
            {
                var client = _connectedClients.Find(x => x.ClientId == e.ClientId);
                if (client == null)
                {
                    Logger.LogError($"Unkownd client disconnected: {e.ClientId}");
                    return Task.CompletedTask;
                }

                _connectedClients.Remove(client);
            }

            Logger.LogInformation($"Client disconnected: {e.ClientId}");

            OnClientDisconnected?.Invoke(e);
            return Task.CompletedTask;
        }

        private Task HandleApplicationMessageReceivedAsync(InterceptingPublishEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = e.ApplicationMessage.ConvertPayloadToString();

            lock (_thisLock)
            {
                var client = _connectedClients.Find(x => x.ClientId == e.ClientId);
                var message = new MqttMessage
                {
                    Timestamp = DateTime.Now,
                    Client = client,
                    Topic = topic,
                    Payload = payload,
                    Original = e.ApplicationMessage,
                };

                _messages.Insert(0, message);

                if (_messagesByTopic.ContainsKey(topic))
                    _messagesByTopic[topic].Insert(0, message);
                else
                    _messagesByTopic[topic] = new List<MqttMessage> { message };
            }

            Logger.LogInformation($"OnMessageReceived: {topic} {payload}");

            OnMessageReceived?.Invoke(e);
            return Task.CompletedTask;
        }

        #endregion Events

        public async Task Publish(InjectedMqttApplicationMessage message, CancellationToken token = default) => await Server?.InjectApplicationMessage(message, token);

        public async Task Publish(string topic, byte[] payload, bool retain = false, MqttQualityOfServiceLevel qos = default, CancellationToken token = default) => await Publish(BuildMessage(topic, payload, retain, qos), token);

        public async Task Publish(string topic, string payload, bool retain = false, MqttQualityOfServiceLevel qos = default, CancellationToken token = default) => await Publish(BuildMessage(topic, payload, retain, qos), token);
    }
}