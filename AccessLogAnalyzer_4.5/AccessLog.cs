using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Abstracta.AccessLogAnalyzer.DataExtractors;

namespace Abstracta.AccessLogAnalyzer
{
    internal class AccessLog
    {
        private const string StrSeparator = "\t";

        internal string Host { get; set; }

        internal DateTime Time { get; set; }

        internal string URLWithoutParameters { get; set; }

        internal string ParametersFromURL { get; set; }

        internal int ResponseCode { get; set; }

        internal double ResponseTime { get; set; }

        internal long ResponseSize { get; set; }

        internal TimeUnitType UnitType { get; private set; }

        internal DataExtractor DataLineExtractor { get; private set; }

        internal bool ContainsReestart { get; private set; }

        private AccessLog(DataExtractor lineFormat)
        {
            Host = string.Empty;
            Time = DateTime.Now;
            URLWithoutParameters = string.Empty;
            ResponseCode = 0;
            ResponseTime = -1;
            ContainsReestart = false;

            UnitType = lineFormat.TimeUnit;
            DataLineExtractor = lineFormat;
        }

        internal static AccessLog CreateFromLine(DataExtractor formatedLine, bool filterStaticRequests, bool filter300)
        {
            if (!formatedLine.IsValid())
            {
                Logger.GetInstance().AddLog("Line discarded: " + formatedLine.Line);
                return null;
            }

            var result = new AccessLog(formatedLine);

            try
            {
                var urlWithoutParameters = formatedLine.Url;
                var parameters = string.Empty;
                if (formatedLine.Url.Contains("?"))
                {
                    urlWithoutParameters = Regex.Replace(formatedLine.Url, "\\?.*", "?");
                    parameters = Regex.Replace(formatedLine.Url, ".*\\?", "?");
                }

                result.URLWithoutParameters = urlWithoutParameters;
                if (filterStaticRequests && URLFilterSingleton.IsStaticResource(result.URLWithoutParameters))
                {
                    Logger.GetInstance().AddLog("Static resource filtered: " + result.URLWithoutParameters);
                    return null;
                }

                // Filter URLs
                if (URLFilterSingleton.GetInstance().DiscardUrlByFilterRules(result.URLWithoutParameters))
                {
                    Logger.GetInstance().AddLog("URL filtered: " + result.URLWithoutParameters);
                    return null;
                }

                result.ParametersFromURL = parameters;
            }
            catch (Exception)
            {
                Logger.GetInstance().AddLog("Line Malformed: " + formatedLine.Line);
                return null;
            }

            try
            {
                result.ResponseCode = formatedLine.ResponseCode;
                if (filter300 && result.ResponseCode >= 300 && result.ResponseCode < 400)
                {
                    Logger.GetInstance().AddLog("URL filtered by 3??: " + result.URLWithoutParameters);
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
                result.ResponseTime = formatedLine.ResponseTime;
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

            result.ContainsReestart = formatedLine.ContainsReestart;

            return result;
        }

        public override string ToString()
        {
            return
                (DataLineExtractor.Contains(DataExtractor.HOST) ? (Host + StrSeparator) : "?" + StrSeparator) +
                (DataLineExtractor.Contains(DataExtractor.TIME) ? (Time.ToString("dd/MM/yyyy H:mm:ss") + StrSeparator) : "?" + StrSeparator) +
                (DataLineExtractor.Contains(DataExtractor.URL) ? (URLWithoutParameters + StrSeparator) : "?" + string.Empty) +
                (DataLineExtractor.Contains(DataExtractor.RCODE) ? (ResponseCode.ToString(CultureInfo.CurrentCulture) + StrSeparator) : "?" + StrSeparator) +
                (DataLineExtractor.Contains(DataExtractor.RSIZE) ? (ResponseSize.ToString(CultureInfo.CurrentCulture) + StrSeparator) : "?" + StrSeparator) +
                (DataLineExtractor.Contains(DataExtractor.RTIME) ? (ResponseTime.ToString(CultureInfo.CurrentCulture) + StrSeparator) : "?" + StrSeparator) +
                (DataLineExtractor.Contains(DataExtractor.URL) ? (ParametersFromURL) : "-" + string.Empty);
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