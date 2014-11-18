using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Abstracta.AccessLogAnalyzer
{
    public static class ServersManager
    {
        private static Dictionary<string, int> ServersIndex { get; set; }

        static ServersManager()
        {
            ServersIndex = new Dictionary<string, int>();
        }

        internal static int GetServerIndex(string serverName)
        {
            return ServersIndex[serverName];
        }

        internal static int AddServer(string serverName)
        {
            if (ServersIndex.ContainsKey(serverName))
            {
                return ServersIndex[serverName];
            }

            ServersIndex.Add(serverName, ServersIndex.Count);

            return ServersIndex.Count - 1;
        }

        internal static void Reset()
        {
            ServersIndex = new Dictionary<string, int>();
        }
    }

    public class Interval
    {
        private List<ServerInInterval> Servers { get; set; }

        internal AccessLogComparerByResponseTime AccessLogComparer { get; set; }

        public readonly static int[] StadisticalPoints = new[] { 2, 4, 6, 8, 10, 15, 20, 30, 40, 60, 80, 100, 120, int.MaxValue };

        public const char StrSeparator = '\t';

        public DateTime StartInterval { get; private set; }

        public DateTime EndInterval { get; private set; }

        public Interval(IEnumerable<string> serverNames, DateTime time, IntervalSize intervalSize, TopTypes top, bool keepListOfHTTP500, bool keepListOfHTTP400)
        {
            var start = time.Subtract(new TimeSpan(0, 0, 0, time.Second));
            var end = start.AddMinutes(GetMinutesFromInterval(intervalSize));
            Initialize(start, end);

            Servers = new List<ServerInInterval>();
            
            var maxItemsInInterval = GetTopIntValueFromTopType(top);
            foreach (var serverName in serverNames)
            {
                var server = new ServerInInterval(this, serverName, maxItemsInInterval, keepListOfHTTP500, keepListOfHTTP400);
                Servers.Add(server);
            }
        }

        internal bool IsEmpty()
        {
            var s2 = Servers.FirstOrDefault(s => !s.IsEmpty());
            return s2 == null;
        }

        //internal void Add(string serverName, AccessLog accessLog)
        //{
        //    Servers[GetServerIndex(serverName)].Add(accessLog);
        //}

        //internal List<AccessLog> GetTopOfInterval(string serverName)
        //{
        //    return Servers[GetServerIndex(serverName)].TopOfInterval;
        //}

        //internal List<AccessLog> GetLogsHTTP500OfInterval(string serverName)
        //{
        //    return Servers[GetServerIndex(serverName)].LogsHTTP500OfInterval;
        //}

        //internal List<AccessLog> GetLogsHTTP400OfInterval(string serverName)
        //{
        //    return Servers[GetServerIndex(serverName)].LogsHTTP400OfInterval;
        //}

        internal void Add(int serverIndex, AccessLog accessLog)
        {
            Servers[serverIndex].Add(accessLog);
        }

        internal List<AccessLog> GetTopOfInterval(int serverIndex)
        {
            return Servers[serverIndex].TopOfInterval;
        }

        internal List<AccessLog> GetLogsHTTP500OfInterval(int serverIndex)
        {
            return Servers[serverIndex].LogsHTTP500OfInterval;
        }

        internal List<AccessLog> GetLogsHTTP400OfInterval(int serverIndex)
        {
            return Servers[serverIndex].LogsHTTP400OfInterval;
        }

        public static int CalculateInitialSize(IntervalSize interval)
        {
            return (24 * 60) / GetMinutesFromInterval(interval);
        }

        public static int GetMinutesFromInterval(IntervalSize interval)
        {
            switch (interval)
            {
                case IntervalSize.UnMinuto:
                    return 1;

                case IntervalSize.CincoMinutos:
                    return 5;

                case IntervalSize.DiezMinutos:
                    return 10;

                case IntervalSize.QuinceMinutos:
                    return 15;

                case IntervalSize.MediaHora:
                    return 30;

                case IntervalSize.UnaHora:
                    return 60;

                case IntervalSize.DosHoras:
                    return 120;
            }

            var posiblesValores = Enum.GetNames(typeof(IntervalSize)).Aggregate(string.Empty, (current, value) => current + value);
            throw new Exception("Valor no reconocido: " + interval + ". Posibles valores: " + posiblesValores);
        }

        public static IntervalSize GetIntervalFromMinutes(int minutes)
        {
            switch (minutes)
            {
                case 1:
                    return IntervalSize.UnMinuto;

                case 5:
                    return IntervalSize.CincoMinutos;

                case 10:
                    return IntervalSize.DiezMinutos;

                case 15:
                    return IntervalSize.QuinceMinutos;

                case 30:
                    return IntervalSize.MediaHora;

                case 60:
                    return IntervalSize.UnaHora;

                case 120:
                    return IntervalSize.DosHoras;
            }

            var posiblesValores = Enum.GetNames(typeof(IntervalSize)).Aggregate(string.Empty, (current, value) => current + value);
            throw new Exception("Valor no reconocido: " + minutes + ". Posibles valores: " + posiblesValores);
        }

        public static IntervalSize GetIntervalSelectedByUser(string intervalName)
        {
            switch (intervalName)
            {
                case "UnMinuto":
                    return IntervalSize.UnMinuto;

                case "CincoMinutos":
                    return IntervalSize.CincoMinutos;

                case "DiezMinutos":
                    return IntervalSize.DiezMinutos;

                case "QuinceMinutos":
                    return IntervalSize.QuinceMinutos;

                case "MediaHora":
                    return IntervalSize.MediaHora;

                case "UnaHora":
                    return IntervalSize.UnaHora;

                case "DosHoras":
                    return IntervalSize.DosHoras;
            }

            var posiblesValores = Enum.GetNames(typeof(IntervalSize)).Aggregate(string.Empty, (current, value) => current + value);
            throw new Exception("Valor no reconocido: " + intervalName + ". Posibles valores: " + posiblesValores);
        }

        public static int GetTopIntValueFromTopType(TopTypes top)
        {
            switch (top)
            {
                case TopTypes.Top5:
                    return 5;

                case TopTypes.Top10:
                    return 10;

                case TopTypes.Top20:
                    return 20;
            }

            var posiblesValores = Enum.GetNames(typeof(TopTypes)).Aggregate(string.Empty, (current, value) => current + value);
            throw new Exception("Valor no reconocido: " + top + ". Posibles valores: " + posiblesValores);
        }

        public static TopTypes GetTopTypeFromTopIntValue(int top)
        {
            switch (top)
            {
                case 5:
                    return TopTypes.Top5;

                case 10:
                    return TopTypes.Top10;

                case 20:
                    return TopTypes.Top20;
            }

            var posiblesValores = Enum.GetNames(typeof(TopTypes)).Aggregate(string.Empty, (current, value) => current + value);
            throw new Exception("Valor no reconocido: " + top + ". Posibles valores: " + posiblesValores);
        }

        public static TopTypes GetTopSelectedByUser(string top)
        {
            switch (top)
            {
                case "Top5":
                    return TopTypes.Top5;

                case "Top10":
                    return TopTypes.Top10;

                case "Top20":
                    return TopTypes.Top20;
            }

            var posiblesValores = Enum.GetNames(typeof(TopTypes)).Aggregate(string.Empty, (current, value) => current + value);
            throw new Exception("Valor no reconocido: " + top + ". Posibles valores: " + posiblesValores);
        }

        public static string StadisticalHeaders
        {
            get
            {
                var res = "between 0  and " + StadisticalPoints[0] + " segs" + StrSeparator;

                for (var i = 0; i < StadisticalPoints.Length - 1; i++)
                {
                    res += "between " + StadisticalPoints[i] + " and " + StadisticalPoints[i + 1] + " segs" + StrSeparator;
                }

                return res;
            }
        }

        public static string GetStringHeader(IList<string> serversName)
        {
            return GetFirstLine(serversName) + "\n" + GetSecondLine(serversName.Count());
        } 

        public override string ToString()
        {
            var serverId = 0;
            var res = new StringBuilder();
            foreach (var serverInInterval in Servers)
            {
                var stadisticalInformationString = String.Join(StrSeparator.ToString(CultureInfo.InvariantCulture),
                                                               serverInInterval.RequestsByResponseTime.Values.ToArray());
                if (serverId == 0)
                {
                    res.Append("" + StartInterval + StrSeparator
                               + serverInInterval.TotalCount + StrSeparator
                               + serverInInterval.CountOfHTTP500 + StrSeparator
                               + serverInInterval.CountOfHTTP400 + StrSeparator
                               + serverInInterval.CountOfHTTP300 + StrSeparator
                               + stadisticalInformationString + StrSeparator);

                }
                else
                {
                    res.Append(""
                               + serverInInterval.TotalCount + StrSeparator
                               + serverInInterval.CountOfHTTP500 + StrSeparator
                               + serverInInterval.CountOfHTTP400 + StrSeparator
                               + serverInInterval.CountOfHTTP300 + StrSeparator
                               + stadisticalInformationString + StrSeparator);
                }

                serverId++;
            }

            return res.ToString();
        }

        private static string GetFirstLine(IEnumerable<string> serversName)
        {
            var res = string.Empty;
            var arrayTmp = new string[StadisticalPoints.Length];

            var tmp = string.Join(StrSeparator.ToString(CultureInfo.InvariantCulture), arrayTmp);

            var serverId = 0;
            foreach (var s in serversName)
            {
                if (serverId == 0)
                {
                    res += s + StrSeparator +
                           StrSeparator + // "TotalCount"
                           StrSeparator + // "HTTP_5??"
                           StrSeparator + // "HTTP_4??"
                           StrSeparator + // "HTTP_3??"
                           tmp + StrSeparator;

                }
                else
                {
                    res += s + StrSeparator +
                           StrSeparator + // "TotalCount"
                           StrSeparator + // "HTTP_5??"
                           StrSeparator + // "HTTP_4??"
                           StrSeparator + // "HTTP_3??"
                           tmp;
                }

                serverId++;
            }

            return res;
        }

        private static string GetSecondLine(int serversCount)
        {
            var res = string.Empty;

            for (var i = 0; i < serversCount; i++)
            {
                if (i == 0)
                {
                    res += "StartInterval" + StrSeparator +
                           "TotalCount" + StrSeparator +
                           "HTTP_5??" + StrSeparator +
                           "HTTP_4??" + StrSeparator +
                           "HTTP_3??" + StrSeparator +
                           StadisticalHeaders;
                }
                else
                {
                    res += "TotalCount" + StrSeparator +
                           "HTTP_5??" + StrSeparator +
                           "HTTP_4??" + StrSeparator +
                           "HTTP_3??" + StrSeparator +
                           StadisticalHeaders;
                }
            }

            return res;
        }

        private void Initialize(DateTime startTime, DateTime endTime)
        {
            StartInterval = startTime;
            EndInterval = endTime;
            AccessLogComparer = new AccessLogComparerByResponseTime();
        }
    }

    internal class AccessLogComparerByResponseTime : IComparer<AccessLog>
    {
        public int Compare(AccessLog x, AccessLog y)
        {
            return x.ResponseTime.CompareTo(y.ResponseTime);
        }
    }
}
