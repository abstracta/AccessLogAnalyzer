using CommandLine;
using CommandLine.Text;

namespace AccessLogAnalizer.Logic.Configuration
{
    internal abstract class AbstractCommandLineParameters
    {
        public const string HelpTextStartProcess = "Runs the process without graphic UI";
        public const string HelpTextLineFormat = "Line format must be space separated. Line can contain the following: " + Constants.Constants.LineFormatDefaultValue;
        public const string HelpTextOutputFile = "Output file name";
        public const string HelpTextLogHttp500 = "Log to file a list with the HTTP 5??. Default value: " + (Constants.Constants.LogHttp500DefaultValue ? "True" : "False");
        public const string HelpTextLogHttp400 = "Log to file a list with the HTTP 4??. Default value: " + (Constants.Constants.LogHttp400DefaultValue ? "True" : "False");
        public const string HelpTextFilterStaticRequests = "Discard all HTTP request to static resources. Default value: " + (Constants.Constants.FilterStaticRequestDefaultValue ? "True" : "False");
        public const string HelpTextHideEmptyIntervals = "Hide empty intervals. Default value: " + (Constants.Constants.HideEmptyIntervalsDefaultValue ? "True" : "False");
        public const string HelpTextFilterFileName = "File that contains texts that will filter URLs. Uses 'contains' comparer. Doesn't compare parameters of URL";
        public const string HelpTextFilter300 = "Discard all requests that have 300 <= RCode < 400. Default value: " + (Constants.Constants.Filter300DefaultValue ? "True" : "False");
        public const string HelpTextVerbose = "Log to file internal info about AccessLogAnalizer. Default value: " + (Constants.Constants.VerboseDefaultValue ? "True" : "False");

        public const string HelpTextInterval = "Interval length in minutes";
        public const string HelpTextTop = "The 'n' slowest URLs on each interval";
        public const string HelpTextInputFile = "Log file name that is going to be procesed";

        public abstract int Interval { get; set; }

        public abstract int Top { get; set; }

        public abstract string InputFile { get; set; }

        [Option('s', "startProcess", HelpText = HelpTextStartProcess)]
        public bool StartProcess { get; set; }

        [Option('l', "formatOfLine", HelpText = HelpTextLineFormat)]
        public string LineFormat { get; set; }

        [Option('o', "outputFile", HelpText = HelpTextOutputFile)]
        public string OutputFile { get; set; }

        [Option("loghttp500", HelpText = HelpTextLogHttp500)]
        public bool LogHttp500 { get; set; }

        [Option("loghttp400", HelpText = HelpTextLogHttp400)]
        public bool LogHttp400 { get; set; }

        [Option("filterStaticRequests", HelpText = HelpTextFilterStaticRequests)]
        public bool FilterStaticRequests { get; set; }

        [Option('h', "hideEmptyIntervals", HelpText = HelpTextHideEmptyIntervals)]
        public bool HideEmptyIntervals { get; set; }

        [Option('f', "filterFileName", HelpText = HelpTextFilterFileName)]
        public string FilterFileName { get; set; }

        [Option("filter3??", HelpText = HelpTextFilter300)]
        public bool Filter300 { get; set; }

        [Option("verbose", HelpText = HelpTextVerbose)]
        public bool Verbose { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        public string DefaultOutputFile
        {
            get { return CommandLineParameterAux.CreateResultFileName(InputFile); }
        }
    }

    // Define a class to receive parsed values
    internal class CommandLineParametersWhenGUI : AbstractCommandLineParameters
    {
        [Option('v', "interval", HelpText = HelpTextInterval)]
        public override int Interval { get; set; }

        [Option('t', "top", HelpText = HelpTextTop)]
        public override int Top { get; set; }

        [Option('i', "inputFile", HelpText = HelpTextInputFile)]
        public override string InputFile { get; set; }
    }

    // Define a class to receive parsed values
    internal class CommandLineParametersWhenNonGUI : AbstractCommandLineParameters
    {
        [Option('v', "interval", Required = true, HelpText = HelpTextInterval)]
        public override int Interval { get; set; }

        [Option('t', "top", Required = true, HelpText = HelpTextTop)]
        public override int Top { get; set; }

        [Option('i', "inputFile", Required = true, HelpText = HelpTextInputFile)]
        public override string InputFile { get; set; }
    }

    public static class CommandLineParameterAux
    {
        public static string CreateResultFileName(string logFileName)
        {
            const string topStr = "-top.log";

            if (logFileName == null)
            {
                return topStr;
            }

            return logFileName + topStr;
        }
    }
}