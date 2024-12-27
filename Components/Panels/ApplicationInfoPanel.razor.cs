using MqttBrokerWithDashboard.Logging;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Hosting.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static MqttBrokerWithDashboard.Logging.AppLifecycleLogger;

namespace MqttBrokerWithDashboard.Components.Panels
{
    public partial class ApplicationInfoPanel : ComponentBase, IAsyncDisposable
    {
        private Timer _timer;

        public IEnumerable<AppLifecycleLog> Logs;
        public DateTime AppStarted;
        public TimeSpan Uptime => Extensions.CommonExtensions.GetUptime();

        protected override void OnInitialized()
        {
            Logs = AppLifecycleLogger.Logs.ToArray();
            AppStarted = Process.GetCurrentProcess().StartTime;

            _timer = new Timer(1000);
            _timer.Elapsed += async (sender, args) => await InvokeAsync(StateHasChangedAsync);
            _timer.AutoReset = true;
            _timer.Enabled = true;

            base.OnInitialized();
        }

        private Task StateHasChangedAsync()
        {
            StateHasChanged();
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }

            await Task.CompletedTask;
        }
    }
}
