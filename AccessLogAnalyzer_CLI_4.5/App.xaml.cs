using System;
using System.Runtime.InteropServices;
using System.Windows;
using Abstracta.AccessLogAnalyzer.DataExtractors;
using CommandLine;

namespace Abstracta.AccessLogAnalyzer
{
    public partial class App
    {
        private const uint AttachParentProcess = 0x0ffffffff;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId);

        private void Initialize(object sender, StartupEventArgs e)
        {
            AttachConsole(AttachParentProcess);
            AbstractCommandLineParameters options = new CommandLineParametersWhenGUI();

            if (!Parser.Default.ParseArguments(e.Args, options))
            {
                Current.Shutdown();
                return;
            }

            try
            {
                var cm = ConfigurationManager.GetInstance();
                cm.Initialize(options);

                if (options.StartProcess)
                {
                    options = new CommandLineParametersWhenNonGUI();

                    if (!Parser.Default.ParseArguments(e.Args, options))
                    {
                        Current.Shutdown();
                        return;
                    }

                    var inputFile = cm.GetValueAsString(Constants.InputFile);
                    var outputFile = (string.IsNullOrWhiteSpace(cm.GetValueAsString(Constants.OutputFile)))
                                          ? CommandLineParameterAux.CreateResultFileName(inputFile)
                                          : cm.GetValueAsString(Constants.OutputFile);

                    var format = cm.GetValueAsString(Constants.LineFormat);
                    var serverType = cm.GetValueAsString(Constants.ServerType);
                    var dateLineExtractor = DataExtractor.CreateDataExtractor(serverType, format);

                    var parameters = new GuiParameters
                    {
                        IntervaloDefinido = Interval.GetIntervalFromMinutes(cm.GetValueAsInteger(Constants.Interval)),
                        Top = Interval.GetTopTypeFromTopIntValue(cm.GetValueAsInteger(Constants.Top)),
                        LogFileName = inputFile,
                        ResultFileName = outputFile,
                        HideEmptyIntervals = cm.GetValueAsBool(Constants.HideEmptyIntervals),
                        LogHTTP500List = cm.GetValueAsBool(Constants.LogHttp500),
                        LogHTTP400List = cm.GetValueAsBool(Constants.LogHttp400),
                        FilterStaticReqs = cm.GetValueAsBool(Constants.FilterStaticRequests),
                        Verbose = cm.GetValueAsBool(Constants.Verbose),
                        Filter300 = cm.GetValueAsBool(Constants.Filter300),
                        DataLineExtractor = dateLineExtractor,
                    };

                    var result = Procesor.ProcessAccessLog(null, parameters);
                    if (result != null)
                    {
                        if (result.Count == 0)
                        {
                            Console.WriteLine(@"There are NO (zero) matching lines acording to format.");
                        }
                        else
                        {
                            Procesor.SaveResultToFile(result, parameters);
                            Console.WriteLine(@"File created");
                        }
                    }

                    Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Current.Shutdown();
            }

            if (!options.StartProcess)
            {
                StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
            }
        }
    }
}
