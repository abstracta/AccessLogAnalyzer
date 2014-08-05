using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AccessLogAnalyzer.Logic
{
    internal class Interval
    {
        private const char StrSeparator = '\t';

        private readonly int _maxItemsInInterval;

        private readonly bool _keepListOfHTTP500, _keepListOfHTTP400;

        private int _countOfHTTP500, _countOfHTTP400, _totalCount;

        private readonly static int[] StadisticalPoints = new[] { 2, 4, 6, 8, 10, 15, 20, 30, 40, 60, 80, 100, 120, int.MaxValue };

        private AccessLogComparerByResponseTime AccessLogComparer { get; set; }

        internal DateTime StartInterval { get; private set; }

        internal DateTime EndInterval { get; private set; }

        internal Dictionary<int, long> StadisticalInformation { get; private set; }

        internal List<AccessLog> TopOfInterval { get; private set; }

        internal List<AccessLog> LogsHTTP400OfInterval { get; private set; }

        internal List<AccessLog> LogsHTTP500OfInterval { get; private set; }

        internal Interval(DateTime time, IntervalSize intervalSize, TopTypes top, bool keepListOfHTTP500, bool keepListOfHTTP400)
        {
            StadisticalInformation = new Dictionary<int, long>();
            _maxItemsInInterval = GetTopIntValueFromTopType(top);
            _keepListOfHTTP500 = keepListOfHTTP500;
            _keepListOfHTTP400 = keepListOfHTTP400;

            var start = time.Subtract(new TimeSpan(0, 0, 0, time.Second));
            var end = start.AddMinutes(GetMinutesFromInterval(intervalSize));
            Initialize(start, end);
        }

        internal Interval(DateTime startTime, DateTime endTime, TopTypes top, bool keepListOfHTTP500, bool keepListOfHTTP400)
        {
            _maxItemsInInterval = GetTopIntValueFromTopType(top);
            _keepListOfHTTP500 = keepListOfHTTP500;
            _keepListOfHTTP400 = keepListOfHTTP400;

            Initialize(startTime, endTime);
        }

        private void Initialize(DateTime startTime, DateTime endTime)
        {
            StartInterval = startTime;
            EndInterval = endTime;
            TopOfInterval = new List<AccessLog>(_maxItemsInInterval);
            LogsHTTP400OfInterval = new List<AccessLog>();
            LogsHTTP500OfInterval = new List<AccessLog>();

            AccessLogComparer = new AccessLogComparerByResponseTime();

            _countOfHTTP500 = 0;
            _countOfHTTP400 = 0;
            _totalCount = 0;

            foreach (var point in StadisticalPoints)
            {
                StadisticalInformation.Add(point, 0);
            }
        }

        internal bool IsEmpty()
        {
            return _totalCount == 0;
        }

        internal void Add(AccessLog accessLog)
        {
            int index;

            // sort insert, TopOfInterval contains just the slowest 'n' accessLogs
            if (TopOfInterval.Count < _maxItemsInInterval)
            {
                index = TopOfInterval.BinarySearch(accessLog, AccessLogComparer);
                if (index < 0) index = ~index;
                TopOfInterval.Insert(index, accessLog);
            }
            else
            {
                // if the slowest accessLog of the collection is slower than the current accessLog, then delete it
                if (TopOfInterval[0].ResponseTime < accessLog.ResponseTime)
                {
                    TopOfInterval.RemoveAt(0);

                    index = TopOfInterval.BinarySearch(accessLog, AccessLogComparer);
                    if (index < 0) index = ~index;
                    TopOfInterval.Insert(index, accessLog);
                }

                // otherwise, don't add the accessLog to the collection
            }

            if (accessLog.ResponseCode >= 500)
            {
                if (_keepListOfHTTP500)
                {
                    LogsHTTP500OfInterval.Add(accessLog);
                }

                _countOfHTTP500++;
            }
            else if (accessLog.ResponseCode >= 400)
            {
                if (_keepListOfHTTP400)
                {
                    LogsHTTP400OfInterval.Add(accessLog);
                }

                _countOfHTTP400++;
            }

            _totalCount++;

            // add stadistical information of the accessLog
            var stadisticalPoint = accessLog.IndexOfResponseTimeInArray(StadisticalPoints);
            if (stadisticalPoint > 0)
            {
                StadisticalInformation[stadisticalPoint]++;    
            }
        }

        internal static int CalculateInitialSize(IntervalSize interval)
        {
            return (24 * 60) / GetMinutesFromInterval(interval);
        }

        internal static int GetMinutesFromInterval(IntervalSize interval)
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

        internal static IntervalSize GetIntervalFromMinutes(int minutes)
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

        internal static IntervalSize GetIntervalSelectedByUser(string intervalName)
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
        
        internal static int GetTopIntValueFromTopType(TopTypes top)
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

        internal static TopTypes GetTopTypeFromTopIntValue(int top)
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

        internal static TopTypes GetTopSelectedByUser(string top)
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

        internal static string StadisticalHeaders
        {
            get
            {
                var res = "between 0  and " + StadisticalPoints[0] + " segs" + StrSeparator;

                for (var i = 0; i < StadisticalPoints.Length -1; i++)
                {
                    res += "between " + StadisticalPoints[i] + " and " + StadisticalPoints[i + 1] + " segs" + StrSeparator;
                }

                return res;
            }
        }

        internal static readonly string ToStringHeader = "StartInterval" + StrSeparator + "TotalCount" + StrSeparator +
                                                         "HTTP_5??" + StrSeparator + "HTTP_4??" + StrSeparator + StadisticalHeaders;

        public override string ToString()
        {
            var stadisticalInformationString = String.Join(StrSeparator.ToString(CultureInfo.InvariantCulture),
                                                           StadisticalInformation.Values.ToArray());
            return "" + StartInterval + StrSeparator
                   // + EndInterval + StrSeparator 
                   + _totalCount + StrSeparator
                   + _countOfHTTP500 + StrSeparator
                   + _countOfHTTP400 + StrSeparator
                   + stadisticalInformationString;
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