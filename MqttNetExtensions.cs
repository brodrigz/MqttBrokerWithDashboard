using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Diagnostics;
using Serilog.Core;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MQTTnet.Protocol;
using MQTTnet.Packets;
using System.Collections.Generic;

namespace MqttBrokerWithDashboard
{
    public class MqttNetLogger : IMqttNetLogger
    {
        private ILogger _logger;
        public MqttNetLogger(ILogger logger)
        {
            this._logger = logger;
        }

        public bool IsEnabled { get; set; } = true;

        public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
        {
            if (!IsEnabled) return;

            // Format the message
            var formattedMessage = string.Format(message, parameters);

            // Log based on the log level
            switch (logLevel)
            {
                case MqttNetLogLevel.Verbose:
                    _logger.LogTrace(formattedMessage);
                    break;
                case MqttNetLogLevel.Info:
                    _logger.LogInformation(formattedMessage);
                    break;
                case MqttNetLogLevel.Warning:
                    _logger.LogWarning(formattedMessage);
                    break;
                case MqttNetLogLevel.Error:
                    _logger.LogError(exception, formattedMessage);
                    break;
            }
        }
    }

    public static class MqttNetExtensions
    {
        public static IMqttNetLogger GetMqttNetLogger(this ILogger logger) => new MqttNetLogger(logger);

        /// <summary>
        /// Creates a MqttClient and connects to the specified broker
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns>Created client</returns>
        public static async Task<IMqttClient> CreateAndConnectMqttClient(string clientId, string host, int port, ILogger logger = null)
        {
            var factory = new MqttFactory(logger?.GetMqttNetLogger() ?? new MqttNetNullLogger());

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(host, port)
                .WithClientId(clientId)
                .Build();

            var client = factory.CreateMqttClient();

            await client.ConnectAsync(options);

            return client;
        }

        public static async Task SubscribeAsync(this IMqttClient client, string topic, MqttQualityOfServiceLevel qos = default)
        {
            var filters = new List<MqttTopicFilter> { new MqttTopicFilter { Topic = topic, QualityOfServiceLevel = qos } };

            var subscribeOptions = new MqttClientSubscribeOptions
            {
                TopicFilters = filters
            };

            await client.SubscribeAsync(subscribeOptions);
        }
    }
}
