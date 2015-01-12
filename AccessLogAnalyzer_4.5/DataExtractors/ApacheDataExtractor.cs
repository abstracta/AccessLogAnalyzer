using System;
using System.Text.RegularExpressions;

namespace Abstracta.AccessLogAnalyzer.DataExtractors
{
    public class ApacheDataExtractor : DataExtractor
    {
        // http://httpd.apache.org/docs/2.2/logs.html
        // http://httpd.apache.org/docs/current/mod/mod_log_config.html

        public new bool NeedParameters = true;

        public static string Parameters
        {
            get { return "%D %a %l %u %t \"%r\" %>s %b \"%{Referer}i\" \"%{User-Agent}i\""; }
        }

        public ApacheDataExtractor(string format)
        {
            LineFormat = format;

            // HOST TIME URL RCODE RTIME RSIZE
            TemplateOrder = new[]
                {
                    TemplateOrderInitValue, TemplateOrderInitValue, TemplateOrderInitValue, 
                    TemplateOrderInitValue, TemplateOrderInitValue, TemplateOrderInitValue
                };

            var elements = ExtractElementsOfFormat(format);

            TemplateOrder[HOST] = FindIndexOf(elements, new[] { "%h", "%a" });
            TemplateOrder[TIME] = FindIndexOf(elements, new[] { "%t" });
            TemplateOrder[URL] = FindIndexOf(elements, new[] { "%r" });
            TemplateOrder[RCODE] = FindIndexOf(elements, new[] { "%>s", "%s" });
            TemplateOrder[RSIZE] = FindIndexOf(elements, new[] { "%B", "%b" });

            TemplateOrder[RTIME] = FindIndexOf(elements, new[] { "%D" });
            if (TemplateOrder[RTIME] != -1)
            {
                TimeUnit = TimeUnitType.Microseconds;
            }
            else 
            {
                TemplateOrder[RTIME] = FindIndexOf(elements, new[] { "%T" });
                TimeUnit = TimeUnitType.Seconds;
            }

            // Validate the format contains all the data needed
            ValidateFormat(TemplateOrder);

            // Create the regular expression
            Pattern = string.Empty;
            for (var i = 0; i < elements.Length; i++)
            {
                if (TemplateOrder[HOST] == i)
                {
                    var tmp = elements[i].Replace("%h", "(\\S+)");
                    tmp = tmp.Replace("%a", "(\\S+)");
                    Pattern += tmp;
                }
                else if (TemplateOrder[TIME] == i)
                {
                    Pattern += elements[i].Replace("%t", "\\[(\\S+ \\S+)\\]");
                }
                else if (TemplateOrder[URL] == i)
                {
                    Pattern += elements[i].Replace("%r", "(\\S+ .+) HTTP?/\\S+");
                }
                else if (TemplateOrder[RCODE] == i)
                {
                    var tmp = elements[i].Replace("%>s", "(\\d+)");
                    tmp = tmp.Replace("%s", "(\\d+)");
                    Pattern += tmp;
                }
                else if (TemplateOrder[RSIZE] == i)
                {
                    var tmp = elements[i].Replace("%B", "(\\S+)");
                    tmp = tmp.Replace("%b", "(\\S+)");
                    Pattern += tmp;
                }
                else if (TemplateOrder[RTIME] == i)
                {
                    var tmp = elements[i].Replace("%D", "(\\S+)");
                    tmp = tmp.Replace("%T", "(\\S+)");
                    Pattern += tmp;
                }
                else
                {
                    // all other 'one digit' elements that aren't used
                    const string regExp1 = "(.*)(%[AfHklLmpPqRuUvVXIOS])(.*)";

                    // all elements of the form '%{name}?'
                    const string regExp2 = "(.*)(%\\{\\S+\\}[CeinopPt])(.*)";
                    var t1 = Regex.Replace(elements[i], regExp1, "$1(\\S+)$3");
                    var t2 = Regex.Replace(elements[i], regExp2, "$1([^\"]*)$3");
                    Pattern += (t1 == elements[i]) ? t2 : t1;
                }
            }

            // Increment all indexes because the groups start on '1' instead of in '0'
            for (var i = 0; i < TemplateOrder.Length; i++)
            {
                if (TemplateOrder[i] >= 0)
                {
                    TemplateOrder[i]++;
                }
            }
        }

        protected override DateTime FormatDateTime(string value)
        {
            return ExtractDateHttpdTomcatJBoss(value);
        }
    }
}