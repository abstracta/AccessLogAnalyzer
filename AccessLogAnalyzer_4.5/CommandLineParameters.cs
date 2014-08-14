namespace Abstracta.AccessLogAnalyzer
{
    public abstract class AbstractCommandLineParameters
    {
        public const string HelpTextStartProcess = "Runs the process without graphic UI";
        public const string HelpTextLineFormat = "Line format must be space separated. Line can contain the following: " + Constants.LineFormatDefaultValue;
        public const string HelpTextOutputFile = "Output file name";
        public const string HelpTextLogHttp500 = "Log to file a list with the HTTP 5??. Default value: " + (Constants.LogHttp500DefaultValue ? "True" : "False");
        public const string HelpTextLogHttp400 = "Log to file a list with the HTTP 4??. Default value: " + (Constants.LogHttp400DefaultValue ? "True" : "False");
        public const string HelpTextFilterStaticRequests = "Discard all HTTP request to static resources. Default value: " + (Constants.FilterStaticRequestDefaultValue ? "True" : "False");
        public const string HelpTextHideEmptyIntervals = "Hide empty intervals. Default value: " + (Constants.HideEmptyIntervalsDefaultValue ? "True" : "False");
        public const string HelpTextFilterFileName = "File that contains texts that will filter URLs. Uses 'contains' comparer. Doesn't compare parameters of URL";
        public const string HelpTextFilter300 = "Discard all requests that have 300 <= RCode < 400. Default value: " + (Constants.Filter300DefaultValue ? "True" : "False");
        public const string HelpTextVerbose = "Log to file internal info about AccessLogAnalizer. Default value: " + (Constants.VerboseDefaultValue ? "True" : "False");

        public const string HelpTextInterval = "Interval length in minutes";
        public const string HelpTextTop = "The 'n' slowest URLs on each interval";
        public const string HelpTextInputFile = "Log file name that is going to be procesed";

        public abstract int Interval { get; set; }

        public abstract int Top { get; set; }

        public abstract string InputFile { get; set; }

        public abstract bool StartProcess { get; set; }

        public abstract string LineFormat { get; set; }

        public abstract string OutputFile { get; set; }

        public abstract bool LogHttp500 { get; set; }

        public abstract bool LogHttp400 { get; set; }

        public abstract bool FilterStaticRequests { get; set; }

        public abstract bool HideEmptyIntervals { get; set; }

        public abstract string FilterFileName { get; set; }

        public abstract bool Filter300 { get; set; }

        public abstract bool Verbose { get; set; }

        public abstract string GetUsage();

        public abstract string DefaultOutputFile { get; }
    }
}