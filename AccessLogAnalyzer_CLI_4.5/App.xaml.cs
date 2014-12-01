using System;
using System.Windows;
using Abstracta.AccessLogAnalyzer;
using CommandLine;

namespace Abstracta.AccessLogAnalyzerUI
{
    public partial class App
    {
        private void Initialize(object sender, StartupEventArgs e)
        {
            AbstractCommandLineParameters options = new CommandLineParametersWhenGUI();

            if (!Parser.Default.ParseArguments(e.Args, options))
            {
                Current.Shutdown();
                Procesor.SaveLogFile();
                return;
            }

            try
            {
                var cm = ConfigurationManager.GetInstance();
                cm.Initialize(options);

                if (options.StartProcess)
                {
                    if (string.IsNullOrEmpty(options.ConfigFileName))
                    {
                        options = new CommandLineParametersWhenNonGUI();

                        if (!Parser.Default.ParseArguments(e.Args, options))
                        {
                            Current.Shutdown();
                            return;
                        }
                    }

                    var outputFile = cm.GetValueAsString(Constants.OutputFile);
                    if (string.IsNullOrEmpty(outputFile))
                    {
                        outputFile = Constants.OutputFileDefaultValue;
                    }

                    var servers = cm.GetListOfServerDefinitions();

                    var parameters = new GuiParameters
                    {
                        Servers = servers,
                        Top = Interval.GetTopTypeFromTopIntValue(cm.GetValueAsInteger(Constants.Top)),
                        IntervaloDefinido = Interval.GetIntervalFromMinutes(cm.GetValueAsInteger(Constants.Interval)),
                        
                        ResultFileName = outputFile,
                        LogHTTP500List = cm.GetValueAsBool(Constants.LogHttp500),
                        LogHTTP400List = cm.GetValueAsBool(Constants.LogHttp400),
                        HideEmptyIntervals = cm.GetValueAsBool(Constants.HideEmptyIntervals),

                        Verbose = cm.GetValueAsBool(Constants.Verbose),
                    };

                    URLFilterSingleton.GetInstance().UpdateFilter(cm.GetValueAsString(Constants.FilterFileName));

                    var result = Procesor.ProcessAccessLog(null, parameters, LoggerType.Console);
                    if (result != null)
                    {
                        if (result.Count == 0)
                        {
                            Console.WriteLine(@"There are NO (zero) matching lines according to format.");
                        }
                        else
                        {
                            Procesor.SaveResultToFile(result, parameters);
                            Console.WriteLine(@"File created");
                        }
                    }

                    Procesor.SaveLogFile();
                    Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                Procesor.SaveLogFile();
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
