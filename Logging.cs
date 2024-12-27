using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using MqttBrokerWithDashboard.Extensions;
using System.Linq;

namespace MqttBrokerWithDashboard.Logging;

/// <summary>
/// Class used to log application lifecycle so it can keep track of host uptime and downtime
/// Only tracks lifecycle events (starting, shutting down) all other logging is configured on startup
/// </summary>
public static class AppLifecycleLogger
{
    public enum LifecycleEvent
    {
        STARTING,
        SHUTTING_DOWN
    }

    public record AppLifecycleLog(LifecycleEvent Event, DateTime Timestamp);

    private static readonly string filePath = "Logs/AppLifecycleLog.json";

    public static async Task LogEventAsync(LifecycleEvent eventType)
    {
        var record = new AppLifecycleLog(eventType, DateTime.Now);
        string json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = false });
        await File.AppendAllTextAsync(filePath, json + Environment.NewLine);
    }

    public static IHost UseLifecycleLogger(this IHost host)
    {
        var lifetime = host.Services.GetService<IHostApplicationLifetime>();

        lifetime.ApplicationStarted.Register(async () =>
        {
            await LogEventAsync(LifecycleEvent.STARTING);
        });

        lifetime.ApplicationStopping.Register(async () =>
        {
            await LogEventAsync(LifecycleEvent.SHUTTING_DOWN);
        });

        return host;
    }

    #region Reading
    private static object readLock = new object();
    private static IEnumerable<AppLifecycleLog> _readLogs;

    public static IEnumerable<AppLifecycleLog> Logs
    {
        get
        {
            lock (readLock)
            {
                _readLogs ??= ReadLogs().ToArray();
                return _readLogs;
            }
        }
    }

    private static IEnumerable<AppLifecycleLog> ReadLogs()
    {
        var logs = new List<AppLifecycleLog>();
        if (File.Exists(filePath))
        {
            string content = IOExtensions.ReadAllTextShared(filePath);
            string[] jsonLogs = content.Split(Environment.NewLine).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            foreach (var json in jsonLogs)
            {
                try
                {
                    var log = JsonSerializer.Deserialize<AppLifecycleLog>(json);
                    if (log != null)
                    {
                        logs.Add(log);
                    }
                }
                catch (JsonException) { }
            }
        }
        return logs;
    }
    #endregion
}

