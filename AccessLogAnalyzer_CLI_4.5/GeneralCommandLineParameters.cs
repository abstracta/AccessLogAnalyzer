using Abstracta.AccessLogAnalyzer;
using Abstracta.AccessLogAnalyzer.DataExtractors;
using CommandLine;
using CommandLine.Text;

namespace Abstracta.AccessLogAnalyzerUI
{
    internal abstract class GeneralCommandLineParameters : AbstractCommandLineParameters
    {
        [Option('s', "startProcess", HelpText = HelpTextStartProcess)]
        public override bool StartProcess { get; set; }

        [Option('l', "formatOfLine", HelpText = HelpTextLineFormat, DefaultValue = Constants.LineFormatDefaultValue)]
        public override string LineFormat { get; set; }

        [Option('o', "outputFile", HelpText = HelpTextOutputFile, DefaultValue = Constants.OutputFileDefaultValue)]
        public override string OutputFile { get; set; }

        [Option("loghttp500", HelpText = HelpTextLogHttp500, DefaultValue = Constants.LogHttp500DefaultValue)]
        public override bool LogHttp500 { get; set; }

        [Option("loghttp400", HelpText = HelpTextLogHttp400, DefaultValue = Constants.LogHttp400DefaultValue)]
        public override bool LogHttp400 { get; set; }

        [Option("filterStaticRequests", HelpText = HelpTextFilterStaticRequests, DefaultValue = Constants.FilterStaticRequestDefaultValue)]
        public override bool FilterStaticRequests { get; set; }

        [Option('h', "hideEmptyIntervals", HelpText = HelpTextHideEmptyIntervals, DefaultValue = Constants.HideEmptyIntervalsDefaultValue)]
        public override bool HideEmptyIntervals { get; set; }

        [Option('f', "filterFileName", HelpText = HelpTextFilterFileName, DefaultValue = Constants.FilterFileNameDefaultValue)]
        public override string FilterFileName { get; set; }

        [Option('c', "configFileName", HelpText = ConfigTextFilterFileName)]
        public override string ConfigFileName { get; set; }

        [Option("filter3??", HelpText = HelpTextFilter300, DefaultValue = Constants.Filter300DefaultValue)]
        public override bool Filter300 { get; set; }

        [Option("verbose", HelpText = HelpTextVerbose, DefaultValue = Constants.VerboseDefaultValue)]
        public override bool Verbose { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        public override string DefaultOutputFile
        {
            get { return CommandLineParameterAux.CreateResultFileName(InputFile); }
        }
    }

    // Define a class to receive parsed values
    internal class CommandLineParametersWhenGUI : GeneralCommandLineParameters
    {
        [HelpOption]
        public override string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        [Option('v', "interval", HelpText = HelpTextInterval, DefaultValue = Constants.IntervalDefaultValue)]
        public override int Interval { get; set; }

        [Option('t', "top", HelpText = HelpTextTop, DefaultValue = Constants.TopDefaultValue)]
        public override int Top { get; set; }

        [Option('i', "inputFile", HelpText = HelpTextInputFile, DefaultValue = Constants.InputFileDefaultValue)]
        public override string InputFile { get; set; }

        [Option('j', "serverType", HelpText = HelpTextServerType, DefaultValue = Constants.ServerTypeDefaultValue)]
        public override ServerType ServerType { get; set; }
    }

    // Define a class to receive parsed values
    internal class CommandLineParametersWhenNonGUI : GeneralCommandLineParameters
    {
        [HelpOption]
        public override string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        [Option('v', "interval", Required = true, HelpText = HelpTextInterval, DefaultValue = Constants.IntervalDefaultValue)]
        public override int Interval { get; set; }

        [Option('t', "top", Required = true, HelpText = HelpTextTop, DefaultValue = Constants.TopDefaultValue)]
        public override int Top { get; set; }

        [Option('i', "inputFile", Required = true, HelpText = HelpTextInputFile, DefaultValue = Constants.InputFileDefaultValue)]
        public override string InputFile { get; set; }

        [Option('j', "serverType", Required = true, HelpText = HelpTextServerType, DefaultValue = Constants.ServerTypeDefaultValue)]
        public override ServerType ServerType { get; set; }
    }
}