﻿using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Abstracta.AccessLogAnalyzer.DataExtractors
{
    public class TomcatDataExtractor : DataExtractor
    {
        // https://tomcat.apache.org/tomcat-7.0-doc/api/org/apache/catalina/valves/AccessLogValve.html

        public new bool NeedParameters = true;

        public static string Parameters
        {
            get { return "%A %b %B %H \"%{Referer}i\" \"%{User-Agent}i\" %m %p %q %r %s %t %U %v %T %I"; }
        }
        
        // TODO: replace the way it works by the way Apache Data Extractor works.
        public TomcatDataExtractor(string format)
        {
            LineFormat = format;

            // HOST TIME URL RCODE RTIME RSIZE
            TemplateOrder = new[]
                {
                    TemplateOrderInitValue, TemplateOrderInitValue, TemplateOrderInitValue, 
                    TemplateOrderInitValue, TemplateOrderInitValue, TemplateOrderInitValue
                };

            var propertySource = new[] { 0, 0, 0, 0, 0, 0 };

            // create regular expression from format string
            FormatItems = ExtractElementsOfFormat(format);
            for (int i = 0, j = 1; i < FormatItems.Length; i++)
            {
                // a - Remote IP address
                if (FormatItems[i].StartsWith("%a"))
                {
                    if (propertySource[HOST] < 4)
                    {
                        FormatItems[i] = FormatItems[i].Replace("%a", "(\\S+)");
                        TemplateOrder[HOST] = j;
                        propertySource[HOST] = 4;
                        j++;
                    }
                    else
                    {
                        FormatItems[i] = FormatItems[i].Replace("%a", "\\S+");
                    }
                }
                    // A - Local IP address
                else if (FormatItems[i].StartsWith("%A"))
                {
                    if (propertySource[HOST] < 1)
                    {
                        FormatItems[i] = FormatItems[i].Replace("%A", "(\\S+)");
                        TemplateOrder[HOST] = j;
                        propertySource[HOST] = 1;
                        j++;
                    }
                    else
                    {
                        FormatItems[i] = FormatItems[i].Replace("%A", "\\S+");
                    }
                }
                    // b - Bytes sent, excluding HTTP headers, or '-' if zero
                else if (FormatItems[i].StartsWith("%b"))
                {
                    if (propertySource[RSIZE] < 1)
                    {
                        FormatItems[i] = FormatItems[i].Replace("%b", "(\\S+)");
                        TemplateOrder[RSIZE] = j;
                        propertySource[RSIZE] = 1;
                        j++;
                    }
                    else
                    {
                        FormatItems[i] = FormatItems[i].Replace("%b", "\\S+");
                    }
                }
                    // B - Bytes sent, excluding HTTP headers
                else if (FormatItems[i].StartsWith("%B"))
                {
                    if (propertySource[RSIZE] < 2)
                    {
                        FormatItems[i] = FormatItems[i].Replace("%B", "(\\S+)");
                        TemplateOrder[RSIZE] = j;
                        propertySource[RSIZE] = 2;
                        j++;
                    }
                    else
                    {
                        FormatItems[i] = FormatItems[i].Replace("%B", "\\S+");
                    }
                }
                    // h - Remote host name (or IP address if resolveHosts is false)
                else if (FormatItems[i].StartsWith("%h"))
                {
                    if (propertySource[HOST] < 3)
                    {
                        FormatItems[i] = FormatItems[i].Replace("%h", "(\\S+)");
                        TemplateOrder[HOST] = j;
                        propertySource[HOST] = 3;
                        j++;
                    }
                    else
                    {
                        FormatItems[i] = FormatItems[i].Replace("%h", "\\S+");
                    }
                }
                    // H - Request protocol
                else if (FormatItems[i].StartsWith("%H"))
                {
                    FormatItems[i] = FormatItems[i].Replace("%H", "(\\S+)");
                    j++;
                }
                    // l - Remote logical username from identd (always returns '-')
                else if (FormatItems[i].StartsWith("%l"))
                {
                    FormatItems[i] = FormatItems[i].Replace("%l", "\\S+");
                }
                    // m - Request method (GET, POST, etc.)
                else if (FormatItems[i].StartsWith("%m"))
                {
                    FormatItems[i] = FormatItems[i].Replace("%m", "\\S+");
                }
                    // p - Local port on which this request was received
                else if (FormatItems[i].StartsWith("%p"))
                {
                    FormatItems[i] = FormatItems[i].Replace("%p", "\\S+");
                }
                    // q - Query string (prepended with a '?' if it exists) // can be empty
                else if (FormatItems[i].StartsWith("%q"))
                {
                    FormatItems[i] = FormatItems[i].Replace("%q", ".*");
                }
                    // r - First line of the request (method and request URI)
                else if (FormatItems[i].StartsWith("%r"))
                {
                    FormatItems[i] = FormatItems[i].Replace("%r", "(\\S+ .+) HTTP?/\\S+");
                    TemplateOrder[URL] = j;
                    j++;
                }
                    // s - HTTP status code of the response
                else if (FormatItems[i].StartsWith("%s"))
                {
                    FormatItems[i] = FormatItems[i].Replace("%s", "(\\S+)");
                    TemplateOrder[RCODE] = j;
                    j++;
                }
                    // S - User session ID
                else if (FormatItems[i].StartsWith("%S"))
                {
                    FormatItems[i] = FormatItems[i].Replace("%S", "(\\S+)");
                    j++;
                }
                    // t - Date and time, in Common Log Format
                else if (FormatItems[i].StartsWith("%t"))
                {
                    FormatItems[i] = FormatItems[i].Replace("%t", "\\[(\\S+ \\S+)\\]");
                    TemplateOrder[TIME] = j;
                    j++;
                }
                    // u - Remote user that was authenticated (if any), else '-'
                else if (FormatItems[i].StartsWith("%u"))
                {
                    FormatItems[i] = FormatItems[i].Replace("%u", "\\S+");
                }
                    // U - Requested URL path
                else if (FormatItems[i].StartsWith("%U"))
                {
                    var hasURL = FormatItems.FirstOrDefault(formatItem => formatItem.StartsWith("%r")) != null;
                    if (TemplateOrder[URL] == -1 && !hasURL)
                    {
                        FormatItems[i] = FormatItems[i].Replace("%U", "(\\S+)");
                        TemplateOrder[URL] = j;
                        j++;
                    }
                    else
                    {
                        FormatItems[i] = FormatItems[i].Replace("%U", "\\S+");    
                    }
                }
                    // v - Local server name
                else if (FormatItems[i].StartsWith("%v"))
                {
                    if (propertySource[HOST] < 2)
                    {
                        FormatItems[i] = FormatItems[i].Replace("%v", "(\\S+)");
                        TemplateOrder[HOST] = j;
                        propertySource[HOST] = 2;
                        j++;
                    }
                    else
                    {
                        FormatItems[i] = FormatItems[i].Replace("%v", "\\S+");
                    }
                }
                    // D - Time taken to process the request, in millis
                else if (FormatItems[i].StartsWith("%D"))
                {
                    if (propertySource[RTIME] < 2)
                    {
                        FormatItems[i] = FormatItems[i].Replace("%D", "(\\S+)");
                        TemplateOrder[RTIME] = j;
                        propertySource[RTIME] = 2;
                        TimeUnit = TimeUnitType.Milliseconds;
                        j++;
                    }
                    else
                    {
                        FormatItems[i] = FormatItems[i].Replace("%D", "\\S+");
                    }
                }
                    // T - Time taken to process the request, in seconds
                else if (FormatItems[i].StartsWith("%T"))
                {
                    if (propertySource[RTIME] < 1)
                    {
                        FormatItems[i] = FormatItems[i].Replace("%T", "(\\S+)");
                        TemplateOrder[RTIME] = j;
                        propertySource[RTIME] = 1;
                        TimeUnit = TimeUnitType.Seconds;
                        j++;
                    }
                    else
                    {
                        FormatItems[i] = FormatItems[i].Replace("%T", "\\S+");
                    }
                }
                    // I - current request thread name (can compare later with stacktraces)
                else if (FormatItems[i].StartsWith("%I"))
                {
                    FormatItems[i] = FormatItems[i].Replace("%I", "\\S+");
                }
                else
                {
                    // all other 'one digit' elements that aren't used
                    const string regExp1 = "(.*)(%[AfHklLmpPqRuUvVXIOS])(.*)";

                    // all elements of the form '%{name}?'
                    const string regExp2 = "(.*)(%\\{\\S+\\}[CeinopPt])(.*)";

                    var t1 = Regex.Replace(FormatItems[i], regExp1, "$1\\S+$3");
                    var t2 = Regex.Replace(FormatItems[i], regExp2, "$1[^\"]*$3");

                    FormatItems[i] = (t1 == FormatItems[i]) ? t2 : t1;
                }
            }

            // Validate the format contains all the data needed
            ValidateFormat(TemplateOrder);

            Pattern = String.Join("", FormatItems);
        }

        protected override DateTime FormatDateTime(string value)
        {
            return ExtractDateHttpdTomcatJBoss(value);
        }
    }
}