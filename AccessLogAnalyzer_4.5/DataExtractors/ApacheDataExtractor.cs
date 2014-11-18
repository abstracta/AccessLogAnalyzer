using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Abstracta.AccessLogAnalyzer.DataExtractors
{
    public class ApacheDataExtractor : DataExtractor
    {
        // http://httpd.apache.org/docs/2.2/logs.html
        // http://httpd.apache.org/docs/current/mod/mod_log_config.html

        public static string Parameters
        {
            get { return "%D %a %l %u %t \"%r\" %>s %b \"%{Referer}i\" \"%{User-Agent}i\""; }
        }

        public ApacheDataExtractor(string format)
        {
            LineFormat = format;

            // HOST TIME URL RCODE RTIME RSIZE
            TemplateOrder = new[] { -1, -1, -1, -1, -1, -1 };

            var elements = ExtractElementsOfFormat(format);

            TemplateOrder[HOST] = FindIndexOf(elements, new[] { "%h", "%a" });
            TemplateOrder[TIME] = FindIndexOf(elements, new[] { "%t" });
            TemplateOrder[URL] = FindIndexOf(elements, new[] { "%r" });
            TemplateOrder[RCODE] = FindIndexOf(elements, new[] { "%>s" });
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
                    Pattern += elements[i].Replace("%t", "\\[(\\S+) \\S+\\]");
                }
                else if (TemplateOrder[URL] == i)
                {
                    Pattern += elements[i].Replace("%r", "(\\S+ \\S+) HTTP?/\\S+");
                }
                else if (TemplateOrder[RCODE] == i)
                {
                    Pattern += elements[i].Replace("%>s", "(\\d+)");
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
                TemplateOrder[i]++;
            }
        }

        protected override DateTime FormatDateTime(string value)
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

        private static int FindIndexOf(IList<string> elementList, IEnumerable<string> elementsToFind)
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
    }
}