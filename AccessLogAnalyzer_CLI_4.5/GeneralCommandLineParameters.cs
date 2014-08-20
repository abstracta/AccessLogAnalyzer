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

        [Option('l', "formatOfLine", HelpText = HelpTextLineFormat)]
        public override string LineFormat { get; set; }

        [Option('o', "outputFile", HelpText = HelpTextOutputFile)]
        public override string OutputFile { get; set; }

        [Option("loghttp500", HelpText = HelpTextLogHttp500)]
        public override bool LogHttp500 { get; set; }

        [Option("loghttp400", HelpText = HelpTextLogHttp400)]
        public override bool LogHttp400 { get; set; }

        [Option("filterStaticRequests", HelpText = HelpTextFilterStaticRequests)]
        public override bool FilterStaticRequests { get; set; }

        [Option('h', "hideEmptyIntervals", HelpText = HelpTextHideEmptyIntervals)]
        public override bool HideEmptyIntervals { get; set; }

        [Option('f', "filterFileName", HelpText = HelpTextFilterFileName)]
        public override string FilterFileName { get; set; }

        [Option("filter3??", HelpText = HelpTextFilter300)]
        public override bool Filter300 { get; set; }

        [Option("verbose", HelpText = HelpTextVerbose)]
        public override bool Verbose { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public override string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        public override string DefaultOutputFile
        {
            get { return CommandLineParameterAux.CreateResultFileName(InputFile); }
        }
    }

    // Define a class to receive parsed values
    internal class CommandLineParametersWhenGUI : GeneralCommandLineParameters
    {
        [Option('v', "interval", HelpText = HelpTextInterval)]
        public override int Interval { get; set; }

        [Option('t', "top", HelpText = HelpTextTop)]
        public override int Top { get; set; }

        [Option('i', "inputFile", HelpText = HelpTextInputFile)]
        public override string InputFile { get; set; }

        [Option('t', "serverType", HelpText = HelpTextServerType)]
        public override ServerType ServerType { get; set; }
    }

    // Define a class to receive parsed values
    internal class CommandLineParametersWhenNonGUI : GeneralCommandLineParameters
    {
        [Option('v', "interval", Required = true, HelpText = HelpTextInterval)]
        public override int Interval { get; set; }

        [Option('t', "top", Required = true, HelpText = HelpTextTop)]
        public override int Top { get; set; }

        [Option('i', "inputFile", Required = true, HelpText = HelpTextInputFile)]
        public override string InputFile { get; set; }

        [Option('t', "serverType", Required = true, HelpText = HelpTextServerType)]
        public override ServerType ServerType { get; set; }
    }
}