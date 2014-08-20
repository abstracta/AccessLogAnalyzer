using System;
using Abstracta.AccessLogAnalyzer.DataExtractors;

namespace Abstracta.AccessLogAnalyzer
{
    internal class AccessLog
    {
        internal string Host { get; set; }

        internal DateTime Time { get; set; }

        internal string URL { get; set; }

        internal int ResponseCode { get; set; }

        internal double ResponseTime { get; set; }

        internal long ResponseSize { get; set; }

        internal TimeUnitType UnitType { get; private set; }

        internal DataExtractor DataLineExtractor { get; private set; }

        private AccessLog(DataExtractor lineFormat)
        {
            Host = string.Empty;
            Time = DateTime.Now;
            URL = string.Empty;
            ResponseCode = 0;
            ResponseTime = -1;

            UnitType = lineFormat.TimeUnit;
            DataLineExtractor = lineFormat;
        }

        internal static AccessLog CreateFromLine(DataExtractor formatedLine, bool filterStaticRequests, bool filter300)
        {
            var result = new AccessLog(formatedLine);

            try
            {
                result.URL = formatedLine.Url;
                if (filterStaticRequests && URLFilterSingleton.IsStaticResource(result.URL))
                {
                    Logger.GetInstance().AddLog("Static resource filtered: " + result.URL);
                    return null;
                }

                // Filter URLs
                if (URLFilterSingleton.GetInstance().DiscardUrlByFilterRules(result.URL))
                {
                    Logger.GetInstance().AddLog("URL filtered: " + result.URL);
                    return null;
                }
            }
            catch (Exception)
            {
                Logger.GetInstance().AddLog("URL Malformed: " + formatedLine.Line);
                return null;
            }

            try
            {
                result.ResponseCode = formatedLine.ResponseCode;
                if (filter300 && result.ResponseCode >= 300 && result.ResponseCode < 400)
                {
                    Logger.GetInstance().AddLog("URL filtered by 3??: " + result.URL);
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }

            try
            {
                result.Host = formatedLine.RemoteHost;
                result.Time = formatedLine.Time;
            }
            catch (Exception)
            {
                return null;
            }

            try
            {
                result.ResponseSize = formatedLine.ResponseSize;
            }
            catch (Exception)
            {
                result.ResponseSize = 0;
            }

            return result;
        }

        public override string ToString()
        {
            return DataLineExtractor.ToString();
        }

        public int IndexOfResponseTimeInArray(int[] stadisticalPoints)
        {
            var rtimeInSeconds = -1;
            switch (UnitType)
            {
                case TimeUnitType.Seconds:
                    rtimeInSeconds = (int) ResponseTime;
                    break;

                case TimeUnitType.Milliseconds:
                    rtimeInSeconds = (int) ResponseTime/1000;
                    break;

                case TimeUnitType.Microseconds:
                    rtimeInSeconds = (int) ResponseTime/1000000;
                    break;
            }

            var i = 0;
            for (; i < stadisticalPoints.Length; i++)
            {
                if (rtimeInSeconds < stadisticalPoints[i])
                {
                    return stadisticalPoints[i];
                }
            }

            return -1;
        }
    }
}