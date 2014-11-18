using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Abstracta.AccessLogAnalyzer.DataExtractors
{
    public enum ServerType
    {
        Apache, 
        IIS,
        Tomcat,
        AccessLogFormat,
        None,
    }

    public abstract class DataExtractor
    {
        // TOMCAT: h, a
        public string RemoteHost { get; protected set; }

        // TOMCAT: t
        public DateTime Time { get; protected set; }

        // TOMCAT: r, m + " " + U + q     -> Use priority here?
        public string Url { get; protected set; }

        // TOMCAT: s
        public int ResponseCode { get; protected set; }

        // TOMCAT: D (millis), T (secs)
        public double ResponseTime { get; protected set; }

        // TOMCAT: B, b(replace '-' by 0)
        public long ResponseSize { get; protected set; }

        // TOMCAT: From ResponseTime
        public TimeUnitType TimeUnit { get; protected set; }

        public string Line { get; protected set; }

        public string LineFormat { get; protected set; }

        // indexes
        public const int HOST = 0;
        public const int TIME = 1;
        public const int URL = 2;
        public const int RCODE = 3;
        public const int RTIME = 4;
        public const int RSIZE = 5;

        protected string[] FormatItems;
        protected int[] TemplateOrder;
        protected string Pattern;

        public virtual void SetLine(string input)
        {
            // Execute the Regular Expression to extract the values
            try
            {
                Line = input;

                var groups = Regex.Match(input, Pattern).Groups;

                if (TemplateOrder[HOST] != 0)
                {
                    RemoteHost = groups[TemplateOrder[HOST]].Value;
                }

                Time = FormatDateTime(groups[TemplateOrder[TIME]].Value);
                Url = groups[TemplateOrder[URL]].Value;
                ResponseCode = int.Parse(groups[TemplateOrder[RCODE]].Value);

                if (TemplateOrder[RSIZE] != 0)
                {
                    ResponseSize = GetResponseSize(groups[TemplateOrder[RSIZE]].Value);
                }

                ResponseTime = double.Parse(groups[TemplateOrder[RTIME]].Value, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                Logger.GetInstance().AddLog("Couldn't extract the values from the line: " + input);
            }
        }

        public virtual bool Contains(int parameter)
        {
            return TemplateOrder[parameter] > 0;
        }

        protected abstract DateTime FormatDateTime(string value);

        protected static string CreatRegExpTemplate(IList<int> templateOrder)
        {
            var result = "$";
            for (var i = 0; i < templateOrder.Count; i++)
            {
                if (templateOrder[i] <= 0) continue;

                if (i == templateOrder.Count - 1)
                {
                    result += templateOrder[i];
                }
                else
                {
                    result += templateOrder[i] + "\t$";
                }
            }

            return result;
        }

        /// <summary>
        /// Parse formatLine of Tomcat and Apache (uses '%' as starter of elements) to create a list of elements
        /// Apache: %h %l %u %t \"%r\" %>s %b \"%{Referer}i\" \"%{User-Agent}i\
        /// Tomcat: %A %b %B %H %m %p %q %r %s %t %U %v %T %I
        /// </summary>
        /// <param name="format">Format line: %A %b %B %H %m %p %q %r %s %t %U %v %T %I</param>
        /// <returns>List of elements: { "%A " , "%b ", ... , "%T " , "%I" } </returns>
        protected static string[] ExtractElementsOfFormat(string format)
        {
            // "%%" is valid, means the character '%'
            var result = new List<string>();

            if (format != null)
            {
                var element = "%";
                var startIndex = format.IndexOf('%');
                for (var i = startIndex + 1; i < format.Length; i++)
                {
                    if (format[i] == '%')
                    {
                        if (format[i + 1] == '%')
                        {
                            i++;
                            element += "%%";
                        }
                        else
                        {
                            result.Add(element);
                            element = "%";
                        }
                    }
                    else
                    {
                        element += format[i];
                    }
                }

                result.Add(element);
            }

            return result.ToArray();
        }

        protected static void ValidateFormat(int[] templateOrderList)
        {
            if (templateOrderList[URL] == -1)
            {
                throw new Exception("Parameter for URL not present: " + "'%r' / URL");
            }

            if (templateOrderList[TIME] == -1)
            {
                throw new Exception("Parameter for TIME not present: " + "'%t' / TIME");
            }

            if (templateOrderList[RTIME] == -1)
            {
                throw new Exception("Parameter for RESPONSE TIME not present: " + "'%D','%T' / RTIME");
            }

            if (templateOrderList[RCODE] == -1)
            {
                throw new Exception("Parameter for RESPONSE CODE not present: " + "'%>s' / RCODE");
            }
        }

        protected static long GetResponseSize(string value)
        {
            return value == "-" ? 0 : long.Parse(value);
        }

        public static ServerType GetServerTypeFromString(string serverTypeStr)
        {
            switch (serverTypeStr.ToLower())
            {
                case "apache":
                    return ServerType.Apache;

                case "tomcat":
                    return ServerType.Tomcat;
                    
                case "iis":
                    return ServerType.IIS;

                case "accesslogformat":
                    return ServerType.AccessLogFormat;

                default:
                    return ServerType.None;
            }
        }

        public static DataExtractor CreateDataExtractor(string serverType, string format)
        {
            return CreateDataExtractor(GetServerTypeFromString(serverType), format);
        }

        public static DataExtractor CreateDataExtractor(ServerType serverType, string format)
        {
            switch (serverType)
            {
                case ServerType.Apache:
                    return new ApacheDataExtractor(format);
                case ServerType.Tomcat:
                    return new TomcatDataExtractor(format);
                case ServerType.IIS:
                    return new IISDataExtractor(format);
                case ServerType.AccessLogFormat:
                    return new AccessLogExtractor(format);
            }

            return null;
        }
    }
}