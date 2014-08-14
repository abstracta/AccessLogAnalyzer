﻿using System.ComponentModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Abstracta.AccessLogAnalyzer
{
    public partial class MainWindow
    {
        private readonly BackgroundWorker _worker;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize tooltips
            ComboInterval.ToolTip = LabelInterval.ToolTip = AbstractCommandLineParameters.HelpTextInterval;
            ComboTop.ToolTip = LabelTop.ToolTip = AbstractCommandLineParameters.HelpTextTop;

            // TxtFormatExample.ToolTip = "";

            TxtLineFormat.ToolTip = LabelLineFormat.ToolTip = AbstractCommandLineParameters.HelpTextLineFormat;
            TxtInputFile.ToolTip = LabelInputFile.ToolTip = AbstractCommandLineParameters.HelpTextInputFile;
            TxtOutputFile.ToolTip = LabelOutputFile.ToolTip = AbstractCommandLineParameters.HelpTextOutputFile;
            TxtFilterFileName.ToolTip = LabelFilterFileName.ToolTip = AbstractCommandLineParameters.HelpTextFilterFileName;

            LogHTTP500ListCheck.ToolTip = AbstractCommandLineParameters.HelpTextLogHttp500;
            LogHTTP400ListCheck.ToolTip = AbstractCommandLineParameters.HelpTextLogHttp400;
            HideEmptyIntervalsCheck.ToolTip = AbstractCommandLineParameters.HelpTextHideEmptyIntervals;
            FilterStaticRequests.ToolTip = AbstractCommandLineParameters.HelpTextFilterStaticRequests;
            Filter300.ToolTip = AbstractCommandLineParameters.HelpTextFilter300;
            Logging.ToolTip = AbstractCommandLineParameters.HelpTextVerbose;

            // Initialize values of elements
            foreach (var value in Enum.GetNames(typeof (IntervalSize)))
            {
                ComboInterval.Items.Add(value);
            }

            foreach (var value in Enum.GetNames(typeof (TopTypes)))
            {
                ComboTop.Items.Add(value);
            }

            var cm = ConfigurationManager.GetInstance();

            ComboInterval.SelectedItem = Interval.GetIntervalFromMinutes(cm.GetValueAsInteger(Constants.Interval)).ToString();
            ComboTop.SelectedItem = Interval.GetTopTypeFromTopIntValue(cm.GetValueAsInteger(Constants.Top)).ToString();
            TxtInputFile.Text = cm.GetValueAsString(Constants.InputFile);
            TxtOutputFile.Text = cm.GetValueAsString(Constants.OutputFile);
            TxtFormatExample.Content = Constants.LineFormatExample;
            TxtLineFormat.Text = cm.GetValueAsString(Constants.LineFormat);
            TxtFilterFileName.Text = cm.GetValueAsString(Constants.FilterFileName);

            LogHTTP500ListCheck.IsChecked = cm.GetValueAsBool(Constants.LogHttp500);
            LogHTTP400ListCheck.IsChecked = cm.GetValueAsBool(Constants.LogHttp400);
            HideEmptyIntervalsCheck.IsChecked = cm.GetValueAsBool(Constants.HideEmptyIntervals);
            FilterStaticRequests.IsChecked = cm.GetValueAsBool(Constants.FilterStaticRequests);
            Logging.IsChecked = cm.GetValueAsBool(Constants.Verbose);
            Filter300.IsChecked = cm.GetValueAsBool(Constants.Filter300);

            TxtFiltersLoaded.Content = "Filters loaded: " + URLFilterSingleton.GetInstance().FiltersLoaded();

            TxtFilterFileName.LostFocus += (sender, args) =>
                {
                    try
                    {
                        URLFilterSingleton.GetInstance().UpdateFilter(TxtFilterFileName.Text);
                        TxtFiltersLoaded.Content = "Filters loaded: " + URLFilterSingleton.GetInstance().FiltersLoaded();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };

            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                // WorkerSupportsCancellation = true,
            };

            _worker.DoWork += ProcessFileAndSaveResults;
            _worker.ProgressChanged += UpdateProgressStatus;
            _worker.RunWorkerCompleted += ProcessCompleted;
        }

        private void ProcessAccessLog(object sender, RoutedEventArgs e)
        {
            /*
             * Quiero tener el top 'x' de cada 'y' intervalo de tiempo
             * El top indicará quien hizo el pedido, cuanto demoró, la URL, y el momento exacto del pedido
             */
            var intervaloDefinido = GetIntervaloSelectedByUser();
            var top = GetTopSelectedByUser();

            var logFileName = TxtInputFile.Text;
            var resultFileName = TxtOutputFile.Text;

            var hideEmptyIntervals = HideEmptyIntervalsCheck.IsChecked != null && HideEmptyIntervalsCheck.IsChecked.Value;
            var logHTTP500List = LogHTTP500ListCheck.IsChecked != null && LogHTTP500ListCheck.IsChecked.Value;
            var logHTTP400List = LogHTTP400ListCheck.IsChecked != null && LogHTTP400ListCheck.IsChecked.Value;
            var filterStaticReqs = FilterStaticRequests.IsChecked != null && FilterStaticRequests.IsChecked.Value;
            var verbose = Logging.IsChecked != null && Logging.IsChecked.Value;
            var filter300 = Filter300.IsChecked != null && Filter300.IsChecked.Value;

            var format = TxtLineFormat.Text;

            var parameters = new GuiParameters
                {
                    IntervaloDefinido = intervaloDefinido,
                    Top = top,
                    LogFileName = logFileName,
                    ResultFileName = resultFileName,
                    HideEmptyIntervals = hideEmptyIntervals,
                    LogHTTP500List = logHTTP500List,
                    LogHTTP400List = logHTTP400List,
                    FilterStaticReqs = filterStaticReqs,
                    Verbose = verbose,
                    Filter300 = filter300,
                    Format = format,
                };

            if (!_worker.IsBusy)
            {
                // Start the asynchronous operation.
                _worker.RunWorkerAsync(parameters);
            }
            else
            {
                MessageBox.Show("A file is already been processing. Wait until it finishes", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // This event handler is where the time-consuming work is done. 
        private static void ProcessFileAndSaveResults(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var p = e.Argument as GuiParameters;

            if (p == null || worker == null)
            {
                MessageBox.Show(@"Worker can't run because parameters is null", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            try
            {
                var result = Procesor.ProcessAccessLog(worker, p);
                if (result == null)
                {
                    return;
                }

                if (result.Count == 0)
                {
                    MessageBox.Show(@"There are NO (zero) matching lines acording to format.", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    Procesor.SaveResultToFile(result, p);
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateProgressStatus(object sender, ProgressChangedEventArgs e)
        {
            ProcessButton.Content = e.ProgressPercentage + "%";
        }

        private void ProcessCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProcessButton.Content = "Start Process";
        }

        private IntervalSize GetIntervaloSelectedByUser()
        {
            return Interval.GetIntervalSelectedByUser((string)ComboInterval.SelectedItem);
        }

        private TopTypes GetTopSelectedByUser()
        {
            return Interval.GetTopSelectedByUser((string)ComboTop.SelectedItem);
        }

        private void RefreshResultFile(object sender, TextChangedEventArgs e)
        {
            TxtOutputFile.Text = CommandLineParameterAux.CreateResultFileName(TxtInputFile.Text);
        }
    }
}
