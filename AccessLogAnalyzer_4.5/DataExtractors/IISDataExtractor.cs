using System;
using System.Text.RegularExpressions;

namespace Abstracta.AccessLogAnalyzer.DataExtractors
{
    public class IISDataExtractor : DataExtractor
    {
        public static string Parameters
        {
            get { return "NOT YET IMPLEMENTED"; }
        }

        public IISDataExtractor(string format)
        {
            LineFormat = format;

            throw new NotImplementedException(format);
        }

        protected override DateTime FormatDateTime(string value)
        {
            throw new NotImplementedException(value);

            value = Regex.Replace(value, "(\\S+):(\\d\\d.\\d\\d.\\d\\d)", "$1 $2");

            return DateTime.Parse(value);
        }
    }
}