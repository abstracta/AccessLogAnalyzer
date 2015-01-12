using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Abstracta.AccessLogAnalyzer.DataExtractors
{
    public abstract class DataExtractor
    {
        protected const int TemplateOrderInitValue = -1;

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

        public bool ContainsReestart { get; protected set; }

        public string Line { get; protected set; }

        public string LineFormat { get; protected set; }

        public virtual bool NeedParameters { get; protected set; }

        // indexes
        public const int HOST   = 0;
        public const int TIME   = 1;
        public const int URL    = 2;
        public const int RCODE  = 3;
        public const int RTIME  = 4;
        public const int RSIZE  = 5;
        public const int DATE   = 6;
        public const int METHOD = 7;
        public const int QUERY  = 8;

        public const string EmptyValue = "-";

        protected string[] FormatItems;
        protected int[] TemplateOrder;
        protected string Pattern;

        protected DataExtractor()
        {
            ContainsReestart = false;
        }

        public virtual void SetLine(string input)
        {
            SetValuesFromLine(input);
        }

        public virtual bool Contains(int parameter)
        {
            return TemplateOrder[parameter] != TemplateOrderInitValue;
        }

        public static DataExtractor CreateDataExtractor(string serverType, string format)
        {
            return CreateDataExtractor(GetServerTypeFromString(serverType), format);
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

                case "jboss":
                    return ServerType.JBoss;

                case "accesslogformat":
                    return ServerType.AccessLogFormat;

                default:
                    return ServerType.None;
            }
        }

        public static DataExtractor CreateDataExtractor(ServerType serverType, string format)
        {
            switch (serverType)
            {
                case ServerType.Apache:
                    return new ApacheDataExtractor(format);

                case ServerType.JBoss:
                case ServerType.Tomcat:
                    return new TomcatDataExtractor(format);

                case ServerType.IIS:
                    return new IISDataExtractor();

                case ServerType.AccessLogFormat:
                    return new AccessLogExtractor(format);
            }

            return null;
        }

        public static string ParametersOfServerType(ServerType serverType)
        {
            switch (serverType)
            {
                case ServerType.Apache:
                    return ApacheDataExtractor.Parameters;

                case ServerType.JBoss:
                case ServerType.Tomcat:
                    return TomcatDataExtractor.Parameters;

                case ServerType.IIS:
                    return IISDataExtractor.Parameters;

                default:
                    return AccessLogExtractor.Parameters;
            }
        }

        public static bool ServerTypeNeedsParameters(ServerType serverType)
        {
            switch (serverType)
            {
                case ServerType.IIS:
                    return false;

                default:
                    return true;
            }
        }

        public bool IsValid()
        {
            return Line != null && Url != null;
        }

        protected abstract DateTime FormatDateTime(string dateTime);

        protected DateTime FormatDateTime(string date, string time)
        {
            return DateTime.Parse(date + " " + time);
        }

        protected void SetValuesFromLine(string input, ServerType sType = ServerType.AccessLogFormat)
        {
            // Execute the Regular Expression to extract the values
            try
            {
                Line = input;

                var groups = Regex.Match(input, Pattern).Groups;

                if (LineContains(HOST))
                {
                    RemoteHost = groups[TemplateOrder[HOST]].Value;
                }

                if (sType == ServerType.IIS)
                {
                    Time = FormatDateTime(groups[TemplateOrder[DATE]].Value, groups[TemplateOrder[TIME]].Value);
                    Url = groups[TemplateOrder[METHOD]].Value + " " + 
                          groups[TemplateOrder[URL]].Value +
                          (LineContains(QUERY) && groups[TemplateOrder[QUERY]].Value != EmptyValue ? "?" + groups[TemplateOrder[QUERY]].Value : string.Empty);
                }
                else
                {
                    Time = FormatDateTime(groups[TemplateOrder[TIME]].Value);
                    Url = groups[TemplateOrder[URL]].Value;
                }
                
                ResponseCode = int.Parse(groups[TemplateOrder[RCODE]].Value);

                if (LineContains(RSIZE))
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

        protected bool LineContains(int value)
        {
            return TemplateOrder[value] != TemplateOrderInitValue;
        }

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
            if (templateOrderList[URL] == TemplateOrderInitValue)
            {
                throw new Exception("Parameter for URL not present: " + "'%r','%U' / URL");
            }

            if (templateOrderList[TIME] == TemplateOrderInitValue)
            {
                throw new Exception("Parameter for TIME not present: " + "'%t' / TIME");
            }

            if (templateOrderList[RTIME] == TemplateOrderInitValue)
            {
                throw new Exception("Parameter for RESPONSE TIME not present: " + "'%D','%T' / RTIME");
            }

            if (templateOrderList[RCODE] == TemplateOrderInitValue)
            {
                throw new Exception("Parameter for RESPONSE CODE not present: " + "'%s' / RCODE");
            }
        }

        protected static long GetResponseSize(string value)
        {
            return value == "-" ? 0 : long.Parse(value);
        }

        protected static int FindIndexOf(IList<string> elementList, IEnumerable<string> elementsToFind)
        {
            foreach (var t in elementsToFind)
            {
                for (var j = 0; j < elementList.Count; j++)
                {
                    if (elementList[j].StartsWith(t))
                    {
                        return j;
                    }
                }
            }

            return -1;
        }

        protected static DateTime ExtractDateHttpdTomcatJBoss(string value)
        {
            value = value.Replace("/Jan/", "/01/");
            value = value.Replace("/Feb/", "/02/");
            value = value.Replace("/Mar/", "/03/");
            value = value.Replace("/Apr/", "/04/");
            value = value.Replace("/May/", "/05/");
            value = value.Replace("/Jun/", "/06/");
            value = value.Replace("/Jul/", "/07/");
            value = value.Replace("/Aug/", "/08/");
            value = value.Replace("/Sep/", "/09/");
            value = value.Replace("/Oct/", "/10/");
            value = value.Replace("/Nov/", "/11/");
            value = value.Replace("/Dec/", "/12/");

            value = Regex.Replace(value, "(\\S+):(\\d\\d.\\d\\d.\\d\\d)", "$1 $2");

            return DateTime.Parse(value);
        }
    }
}