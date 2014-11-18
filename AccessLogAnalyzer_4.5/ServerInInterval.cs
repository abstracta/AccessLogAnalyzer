using System.Collections.Generic;

namespace Abstracta.AccessLogAnalyzer
{
    public class ServerInInterval
    {
        private readonly Interval _myInterval;

        private readonly int _maxItemsInInterval;

        private readonly bool _keepListOfHTTP500, _keepListOfHTTP400;

        internal Dictionary<int, long> RequestsByResponseTime { get; private set; }

        internal List<AccessLog> TopOfInterval { get; private set; }

        internal List<AccessLog> LogsHTTP400OfInterval { get; private set; }

        internal List<AccessLog> LogsHTTP500OfInterval { get; private set; }

        public string ServerName { get; private set; }

        public int CountOfHTTP500 { get; private set; }

        public int CountOfHTTP400 { get; private set; }

        public int CountOfHTTP300 { get; private set; }

        public int TotalCount { get; private set; }

        internal ServerInInterval(Interval myInterval, string serverName, int top, bool keepListOfHTTP500, bool keepListOfHTTP400)
        {
            ServerName = serverName;
            _myInterval = myInterval;
            _maxItemsInInterval = top;
            _keepListOfHTTP500 = keepListOfHTTP500;
            _keepListOfHTTP400 = keepListOfHTTP400;
            RequestsByResponseTime = new Dictionary<int, long>();
            TopOfInterval = new List<AccessLog>(top);
            LogsHTTP400OfInterval = new List<AccessLog>();
            LogsHTTP500OfInterval = new List<AccessLog>();

            CountOfHTTP500 = 0;
            CountOfHTTP400 = 0;
            CountOfHTTP300 = 0;
            TotalCount = 0;

            foreach (var point in Interval.StadisticalPoints)
            {
                RequestsByResponseTime.Add(point, 0);
            }
        }

        internal bool IsEmpty()
        {
            return TotalCount == 0;
        }

        internal void Add(AccessLog accessLog)
        {
            int index;

            // sort insert, TopOfInterval contains just the slowest 'n' accessLogs
            if (TopOfInterval.Count < _maxItemsInInterval)
            {
                index = TopOfInterval.BinarySearch(accessLog, _myInterval.AccessLogComparer);
                if (index < 0) index = ~index;
                TopOfInterval.Insert(index, accessLog);
            }
            else
            {
                // if the slowest accessLog of the collection is slower than the current accessLog, then delete it
                if (TopOfInterval[0].ResponseTime < accessLog.ResponseTime)
                {
                    TopOfInterval.RemoveAt(0);

                    index = TopOfInterval.BinarySearch(accessLog, _myInterval.AccessLogComparer);
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

                CountOfHTTP500++;
            }
            else if (accessLog.ResponseCode >= 400)
            {
                if (_keepListOfHTTP400)
                {
                    LogsHTTP400OfInterval.Add(accessLog);
                }

                CountOfHTTP400++;
            } 
            else if (accessLog.ResponseCode >= 300)
            {
                CountOfHTTP300++;
            }

            TotalCount++;

            // add stadistical information of the accessLog
            var stadisticalPoint = accessLog.IndexOfResponseTimeInArray(Interval.StadisticalPoints);
            if (stadisticalPoint > 0)
            {
                RequestsByResponseTime[stadisticalPoint]++;    
            }
        }
    }
}