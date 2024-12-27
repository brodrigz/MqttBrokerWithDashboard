using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;

namespace MqttBrokerWithDashboard.Models
{
    public class MqttClientModel
    {
        public string ClientId { get; set; }

        public DateTime TimeOfConnection { get; private set; }
        public DateTime? TimeOfDisconnection { get; private set; }
        public TimeSpan MaxDowntime { get; private set; } = TimeSpan.Zero;
        public bool Connected { get; private set; }

        private int _publishCount;
        public int PublishCount => _publishCount;

        private int _disconnectCount;
        public int DisconnectCount => _disconnectCount;

        public decimal AvgPublishPerMinute { get; private set; }
        public IEnumerable<string> SubscribedTopics => _subs.Select(x => x.Key);

        [JsonIgnore]
        public bool AllowSend { get; set; }
        [JsonIgnore]
        public bool AllowReceive { get; set; }

        // Use a thread-safe collection for subscriptions
        private readonly ConcurrentDictionary<string, byte> _subs = new();
        private readonly ConcurrentQueue<DateTime> _pubs = new();


        public void RecordSubscribe(string topic)
        {
            _subs[topic] = 0x1;
        }

        public void RecordUnsubscribe(string topic)
        {
            _subs.TryRemove(topic, out _);
        }

        public void RecordConnect()
        {
            Connected = true;
            TimeOfConnection = DateTime.Now;
            // Calculate MaxDowntime
            if (TimeOfDisconnection > TimeOfConnection) return; //Safeguard
            if (TimeOfDisconnection != null)
            {
                TimeSpan downtime = TimeOfConnection - TimeOfDisconnection.Value;
                if (downtime > MaxDowntime)
                {
                    MaxDowntime = downtime;
                }
            }
        }

        public void RecordDisconnect()
        {
            Connected = false;
            TimeOfDisconnection = DateTime.Now;
            Interlocked.Increment(ref _disconnectCount);
        }

        public void RecordPublish()
        {
            Interlocked.Increment(ref _publishCount);
            // Calculate avg pub per minute
            var cutoff = DateTime.Now.AddMinutes(-1);
            while (_pubs.TryPeek(out var time) && time < cutoff)
            {
                _pubs.TryDequeue(out _);
            }
            AvgPublishPerMinute = _pubs.Count;
            _pubs.Enqueue(DateTime.Now);
        }
    }
}
