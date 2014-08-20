using System;

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

            // Increment all indexes because the groups start on '1' instead of in '0'
            for (var i = 0; i < TemplateOrder.Length; i++)
            {
                TemplateOrder[i]++;
            }
        }

        protected override DateTime FormatDateTime(string value)
        {
            return DateTime.Parse(value);
        }
    }
}