
using System.Linq;
using System;
using System.Collections.Generic;

namespace AccessLogAnalyzer.Logic
{
    internal class AccessLog
    {
        private const char StrSeparator = '\t';

        internal const string StrHost = "HOST";
        internal const string StrTime = "TIME";
        internal const string StrURL = "URL";
        internal const string StrRcode = "RCODE";

        internal const string StrRtime = "RTIME";
        internal const string StrREndTime = "RENDTIME";
        
        internal const string StrRsize = "RSIZE";

        internal const string StrSecond = "SECOND";
        internal const string StrMillisecond = "MILLISECOND";
        internal const string StrMicrosecond = "MICROSECOND";

        internal static string[] FormatTime = { StrRtime, StrREndTime };
        internal static string[] FormatTimeUnit = { StrSecond, StrMillisecond, StrMicrosecond };

        /// <summary>
        /// When update this, remember to update Constants.LineFormatDefaultValue
        /// </summary>
        internal static string[] FormatParameters =
            {
                StrHost, 
                StrTime, 
                StrURL, 
                StrRcode, 
                String.Join("|", FormatTime),
                StrRsize, 
                String.Join("|", FormatTimeUnit)
            };

        public static string Parameters
        {
            get 
            {
                return String.Join(" ", FormatParameters);
            }
        }

        private readonly string[] _format;

        internal string Host { get; set; }

        internal DateTime Time { get; set; }

        internal string URL { get; set; }

        internal int ResponseCode { get; set; }

        internal long ResponseTime { get; set; }

        internal long ResponseSize { get; set; }

        internal TimeUnitType UnitType { get; private set; }

        private AccessLog(string[] format, TimeUnitType timeUnitType)
        {
            Host = string.Empty;
            Time = DateTime.Now;
            URL = string.Empty;
            ResponseCode = 0;
            ResponseTime = -1;
            _format = format;
            UnitType = timeUnitType;
        }

        internal static AccessLog CreateFromLine(string line, string[] formatItems, TimeUnitType timeUnitType, bool isResponseEndTime, bool filterStaticRequests, bool filter300)
        {
            var lineParsed = line.Split(StrSeparator);

            var result = new AccessLog(formatItems, timeUnitType);

            if (lineParsed.Length != formatItems.Length)
            {
                Logger.GetInstance().AddLog("Line regected: " + line);
                return null;
            }

            try
            {
                result.URL = GetPropertyAsString(lineParsed, formatItems, StrURL);
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
                Logger.GetInstance().AddLog("URL Malformed: " + line);
                return null;
            }

            try
            {
                result.ResponseCode = GetPropertyAsInt(lineParsed, formatItems, StrRcode);
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
                result.Host = GetPropertyAsString(lineParsed, formatItems, StrHost);
                result.Time = GetPropertyAsDateTime(lineParsed, formatItems, StrTime);
                if (isResponseEndTime)
                {
                    var rEndTime = GetPropertyAsDateTime(lineParsed, formatItems, StrREndTime);
                    result.ResponseTime = Convert.ToInt64((rEndTime - result.Time).TotalMilliseconds);
                }
                else
                {
                    result.ResponseTime = GetPropertyAsLong(lineParsed, formatItems, StrRtime);
                }
            }
            catch (Exception)
            {
                return null;
            }

            try
            {
                result.ResponseSize = GetPropertyAsLong(lineParsed, formatItems, StrRsize);
            }
            catch (Exception)
            {
                result.ResponseSize = 0;
            }

            return result;
        }

        private static object GetProperty(IList<string> lineParsed, IList<string> formatItems, string property)
        {
            var i = IndexOf(property, formatItems);
            return i == -1 ? null : lineParsed[i];
        }

        private static string GetPropertyAsString(IList<string> lineParsed, IList<string> formatItems, string property)
        {
            var res = GetProperty(lineParsed, formatItems, property) as string;
            return res ?? string.Empty;
        }

        private static int GetPropertyAsInt(IList<string> lineParsed, IList<string> formatItems, string property)
        {
            var res = GetProperty(lineParsed, formatItems, property) as string;
            return res == null ? -1 : ((res == "-") ? -1 : int.Parse(res));
        }

        private static long GetPropertyAsLong(IList<string> lineParsed, IList<string> formatItems, string property)
        {
            var res = GetProperty(lineParsed, formatItems, property) as string;
            return res == null ? -1 : ((res == "-") ? -1 : long.Parse(res));
        }

        private static DateTime GetPropertyAsDateTime(IList<string> lineParsed, IList<string> formatItems, string property)
        {
            var res = GetProperty(lineParsed, formatItems, property) as string;
            return res == null ? DateTime.MinValue : DateTime.Parse(res);
        }

        private static int IndexOf(string resource, IList<string> formatItems)
        {
            if (formatItems == null)
            {
                return -1;
            }

            var i = 0;
            for (; i < formatItems.Count; i++)
            {
                if (formatItems[i] == resource)
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool Contains(string resource, IEnumerable<string> formatItems)
        {
            return formatItems != null && formatItems.Any(t => t == resource);
        }

        public override string ToString()
        {
            return
                (Contains(StrHost, _format) ? ("" + Host + StrSeparator) : string.Empty) +
                (Contains(StrTime, _format) ? ("" + Time + StrSeparator) : string.Empty) +
                (Contains(StrURL, _format) ? ("" + URL + StrSeparator) : string.Empty) +
                (Contains(StrRcode, _format) ? ("" + ResponseCode + StrSeparator) : string.Empty) +
                (Contains(StrRsize, _format) ? ("" + ResponseSize + StrSeparator) : string.Empty) +
                (Contains(StrRtime, _format) ? ("" + ResponseTime) : string.Empty);
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