namespace MqttBrokerWithDashboard.Options
{
    public class MqttServiceOptions
    {
        public int MaxRecordedDisconnections { get; set; }
        public int MaxRecordedSubscriptions { get; set; }
        public int MaxRecordedMessages { get; set; }
        public string ServerId { get; set; }
    }
}
