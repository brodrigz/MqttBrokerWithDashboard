using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MqttBrokerWithDashboard.Extensions
{
    public static class CommonExtensions
    {
        public static TimeSpan GetUptime()
        {
            using (var process = Process.GetCurrentProcess())
            {
                return DateTime.Now - process.StartTime;
            }
        }

        public static bool TryEvictOldest<TKey, TValue>(this IDictionary<TKey, TValue> src, Func<TValue, DateTime> selectDateExpr)
        {
            if (src == null || src.Count == 0)
            {
                return false;
            }

            TKey oldestKey = default;
            DateTime oldestDate = DateTime.MaxValue;
            bool found = false;

            foreach (var kvp in src)
            {
                DateTime currentDate = selectDateExpr(kvp.Value);
                if (currentDate < oldestDate)
                {
                    oldestDate = currentDate;
                    oldestKey = kvp.Key;
                    found = true;
                }
            }

            if (!found)
            {
                return false;
            }

            return src.Remove(oldestKey);
        }

        public static bool TryEvictOldest<TKey, TValue>(this IDictionary<TKey, TValue> src, Func<TValue, DateTime> selectDateExpr, Func<TValue, bool> filter)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));
            if (selectDateExpr == null)
                throw new ArgumentNullException(nameof(selectDateExpr));
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            if (src.Count == 0)
            {
                return false;
            }

            TKey oldestKey = default;
            DateTime oldestDate = DateTime.MaxValue;
            bool found = false;

            foreach (var kvp in src)
            {
                if (filter(kvp.Value))
                {
                    DateTime currentDate = selectDateExpr(kvp.Value);
                    if (currentDate < oldestDate)
                    {
                        oldestDate = currentDate;
                        oldestKey = kvp.Key;
                        found = true;
                    }
                }
            }

            if (!found)
            {
                return false; // No valid item found matching the filter
            }

            return src.Remove(oldestKey);
        }


    }
}
