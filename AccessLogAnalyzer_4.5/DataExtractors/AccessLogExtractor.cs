using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Abstracta.AccessLogAnalyzer.DataExtractors
{
    public class AccessLogExtractor : DataExtractor
    {
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

        public static string Parameters
        {
            get { return String.Join(" ", FormatParameters); }
        }

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

        public AccessLogExtractor(string format)
        {
            if (format.Contains(StrMicrosecond))
            {
                TimeUnit = TimeUnitType.Microseconds;
                format = format.Replace(StrMicrosecond, string.Empty).Trim();
            }
            else if (format.Contains(StrMillisecond))
            {
                TimeUnit = TimeUnitType.Milliseconds;
                format = format.Replace(StrMillisecond, string.Empty).Trim();
            }
            else if (format.Contains(StrSecond))
            {
                TimeUnit = TimeUnitType.Seconds;
                format = format.Replace(StrSecond, string.Empty).Trim();
            }
            else
            {
                TimeUnit = TimeUnitType.Milliseconds;
            }

            // Format: HOST TIME URL RCODE RTIME|RENDTIME RSIZE (SECOND|MILLISECOND|MICROSECOND)
            var elements = format.Split(' ');

            TemplateOrder = new[] { -1, -1, -1, -1, -1, -1 };

            for (var i = 0; i < elements.Length; i++)
            {
                switch (elements[i].Trim())
                {
                    case StrHost:
                        TemplateOrder[HOST] = i;
                        break;
                    case StrTime:
                        TemplateOrder[TIME] = i;
                        break;
                    case StrURL:
                        TemplateOrder[URL] = i;
                        break;
                    case StrRcode:
                        TemplateOrder[RCODE] = i;
                        break;
                    case StrRtime:
                        TemplateOrder[RTIME] = i;
                        break;
                    case StrREndTime:
                        // todo, add support for this case
                        TemplateOrder[RTIME] = i;
                        break;
                    case StrRsize:
                        TemplateOrder[RSIZE] = i;
                        break;
                }
            }

            // Validate the format contains all the data needed
            ValidateFormat(TemplateOrder);

            // Create the regular expression
            Pattern = "(.*)";
            for (var i = 1; i < TemplateOrder.Length; i++)
            {
                if (TemplateOrder[i] != -1)
                {
                    Pattern += "\t(.*)";
                }
            }
        }

        public override void SetLine(string input)
        {
            try
            {
                Line = input;
                var groups = input.Split(new[] {'\t'});

                if (groups.Length != CountGroups(TemplateOrder))
                {
                    return;
                }

                if (TemplateOrder[HOST] != -1)
                {
                    RemoteHost = groups[TemplateOrder[HOST]];
                }

                Time = FormatDateTime(groups[TemplateOrder[TIME]]);
                Url = groups[TemplateOrder[URL]];
                ResponseCode = int.Parse(groups[TemplateOrder[RCODE]]);

                if (TemplateOrder[RSIZE] != -1)
                {
                    ResponseSize = GetResponseSize(groups[TemplateOrder[RSIZE]]);
                }

                ResponseTime = double.Parse(groups[TemplateOrder[RTIME]], CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                Logger.GetInstance().AddLog("Couldn't extract the values from the line: " + input);
            }
        }

        private static int CountGroups(IEnumerable<int> templateOrder)
        {
            return templateOrder.Count(t => t != -1);
        }

        public override bool Contains(int parameter)
        {
            return TemplateOrder[parameter] > -1;
        }

        protected override DateTime FormatDateTime(string value)
        {
            return DateTime.Parse(value);
        }
    }
}