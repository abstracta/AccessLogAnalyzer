using System;
using System.Collections.Generic;

namespace Abstracta.AccessLogAnalyzer.DataExtractors
{
    public class IISDataExtractor : DataExtractor
    {
        public new bool NeedParameters = false;

        public static string Parameters
        {
            get { return "Don't need parameters"; }
        }

        public IISDataExtractor() 
        {
            LineFormat = string.Empty;
        }

        public override void SetLine(string input)
        {
            ContainsReestart = false;

            if (!IsCommentLine(input))
            {
                if (LineFormatIsKnown())
                {
                    SetValuesFromLine(input, ServerType.IIS);
                }
                else
                {
                    throw new Exception("Unknown format of line: " + input);
                }
            }
            else
            {
                if (LineContainsFormatDefinition(input))
                {
                    // #Fields: date time s-ip cs-method cs-uri-stem cs-uri-query s-port cs-username c-ip cs(User-Agent) cs-host sc-status sc-substatus sc-win32-status sc-bytes cs-bytes time-taken
                    InitializeDataExtractor(input.Split(':')[1].Trim());
                }
                else if (LineContainsReestartInformation(input))
                {
                    ContainsReestart = true;
                }
            }
        }

        private void InitializeDataExtractor(string format)
        {
            LineFormat = format;

            // HOST TIME URL RCODE RTIME RSIZE, DATE METHOD QUERY
            TemplateOrder = new[]
                {
                    TemplateOrderInitValue, TemplateOrderInitValue, TemplateOrderInitValue, 
                    TemplateOrderInitValue, TemplateOrderInitValue, TemplateOrderInitValue,
                    TemplateOrderInitValue, TemplateOrderInitValue, TemplateOrderInitValue
                };

            var elements = ExtractElementsOfIISFormat(format);

            TemplateOrder[HOST]     = FindIndexOf(elements, new[] { "c-ip", "cs-host" });

            TemplateOrder[DATE]     = FindIndexOf(elements, new[] { "date" });
            TemplateOrder[TIME]     = FindIndexOf(elements, new[] { "time" });

            TemplateOrder[METHOD]   = FindIndexOf(elements, new[] { "cs-method" }); 
            TemplateOrder[URL]      = FindIndexOf(elements, new[] { "cs-uri-stem" });
            TemplateOrder[QUERY]    = FindIndexOf(elements, new[] { "cs-uri-query" });

            TemplateOrder[RCODE]    = FindIndexOf(elements, new[] { "sc-status" });
            TemplateOrder[RSIZE]    = FindIndexOf(elements, new[] { "sc-bytes" });
            TemplateOrder[RTIME]    = FindIndexOf(elements, new[] { "time-taken" });
            TimeUnit                = TimeUnitType.Milliseconds;

            // Validate the format contains all the data needed
            ValidateIISFormat(TemplateOrder);

            // Create the regular expression
            Pattern = string.Empty;
            for (var i = 0; i < elements.Count; i++)
            {
                if (TemplateOrder[HOST] == i)
                {
                    var tmp = elements[i].Replace("c-ip", "(\\S+)");
                    tmp = tmp.Replace("cs-host", "(\\S+)");
                    Pattern += tmp;
                }
                else if (TemplateOrder[DATE] == i)
                {
                    Pattern += elements[i].Replace("date", "(\\S+)");
                }
                else if (TemplateOrder[TIME] == i)
                {
                    Pattern += elements[i].Replace("time", "(\\S+)");
                }
                else if (TemplateOrder[METHOD] == i)
                {
                    Pattern += elements[i].Replace("cs-method", "(\\S+)");
                }
                else if (TemplateOrder[URL] == i)
                {
                    Pattern += elements[i].Replace("cs-uri-stem", "(\\S+)");
                }
                else if (TemplateOrder[QUERY] == i)
                {
                    Pattern += elements[i].Replace("cs-uri-query", "(\\S+)");
                }
                else if (TemplateOrder[RCODE] == i)
                {
                    Pattern += elements[i].Replace("sc-status", "(\\d+)");
                }
                else if (TemplateOrder[RSIZE] == i)
                {
                    Pattern += elements[i].Replace("sc-bytes", "(\\d+)");
                }
                else if (TemplateOrder[RTIME] == i)
                {
                    Pattern += elements[i].Replace("time-taken", "(\\d+)");
                }
                else
                {
                    // all other elements that aren't used
                    var t1 = elements[i].Replace(elements[i].Trim(), "(\\S+)");
                    Pattern += t1;
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

        private static void ValidateIISFormat(IList<int> templateOrder)
        {
            if (templateOrder == null) throw new ArgumentNullException("templateOrder");

            if (templateOrder[METHOD] == TemplateOrderInitValue)
            {
                throw new Exception("Parameter for METHOD not present: 'cs-method'");
            }

            if (templateOrder[URL] == TemplateOrderInitValue)
            {
                throw new Exception("Parameter for URL not present: 'cs-uri-stem'");
            }

            if (templateOrder[DATE] == TemplateOrderInitValue)
            {
                throw new Exception("Parameter for DATE not present: 'date'");
            }

            if (templateOrder[TIME] == TemplateOrderInitValue)
            {
                throw new Exception("Parameter for TIME not present: 'time'");
            }

            if (templateOrder[RTIME] == TemplateOrderInitValue)
            {
                throw new Exception("Parameter for RESPONSE TIME not present: 'time-taken'");
            }

            if (templateOrder[RCODE] == TemplateOrderInitValue)
            {
                throw new Exception("Parameter for RESPONSE CODE not present: 'sc-status'");
            }
        }

        private static IList<string> ExtractElementsOfIISFormat(string format)
        {
            var tmp = format.Split(' ');
            var itemsCount = tmp.Length -1;
            for (var i = 0; i < itemsCount; i++)
            {
                tmp[i] = tmp[i] + " ";
            }

            return tmp;
        }

        protected override DateTime FormatDateTime(string value)
        {
            //  2014-12-23 01:29:21
            return DateTime.Parse(value);
        }

        private bool LineFormatIsKnown()
        {
            return LineFormat != string.Empty;
        }

        private static bool IsCommentLine(string line)
        {
            return line.StartsWith("#");
        }

        private static bool LineContainsFormatDefinition(string line)
        {
            return line.StartsWith("#Fields:");
        }

        private static bool LineContainsReestartInformation(string line)
        {
            return line.StartsWith("#Date:");
        }
    }
}