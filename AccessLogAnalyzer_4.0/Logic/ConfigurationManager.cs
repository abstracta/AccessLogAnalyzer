using System;
using System.Collections.Generic;

namespace AccessLogAnalyzer.Logic
{
    internal class ConfigurationManager
    {
        private bool _initialized;

        private static volatile ConfigurationManager _instance;

        private static readonly object Lock = new object();

        private readonly Dictionary<int, object> _parameters;

        private ConfigurationManager()
        {
            // instanciate _parameters with default values
            _parameters = new Dictionary<int, object>
                {
                    { Constants.Interval,       Constants.IntervalDefaultValue },
                    { Constants.Top,            Constants.TopDefaultValue },
                    { Constants.LineFormat,     Constants.LineFormatDefaultValue },
                    { Constants.FilterFileName, Constants.FilterFileNameDefaultValue },

                    { Constants.InputFile,      Constants.InputFileDefaultValue },
                    { Constants.OutputFile,     Constants.OutputFileDefaultValue },

                    { Constants.LogHttp400,     Constants.LogHttp400DefaultValue },
                    { Constants.LogHttp500,     Constants.LogHttp500DefaultValue },

                    { Constants.FilterStaticRequests,   Constants.FilterStaticRequestDefaultValue },
                    { Constants.Filter300,              Constants.Filter300DefaultValue },

                    { Constants.HideEmptyIntervals,     Constants.HideEmptyIntervalsDefaultValue },

                    { Constants.Verbose,        Constants.VerboseDefaultValue },
                };
        }

        public static ConfigurationManager GetInstance()
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ConfigurationManager();
                    }
                }
            }

            return _instance;
        }

        /// <summary>
        /// Initialice parameters used in GUI or in non-GUI mode
        /// </summary>
        /// <param name="parameters"></param>
        internal void Initialize(AbstractCommandLineParameters parameters)
        {
            if (_initialized)
            {
                throw new Exception("Configuration Manager already initialized!");
            }

            if (parameters.Interval != 0)
            {
                _parameters[Constants.Interval] = parameters.Interval;
            }

            if (parameters.Top != 0)
            {
                _parameters[Constants.Top] = parameters.Top;
            }

            if (!string.IsNullOrWhiteSpace(parameters.LineFormat))
            {
                _parameters[Constants.LineFormat] = parameters.LineFormat;
            }

            if (!string.IsNullOrWhiteSpace(parameters.FilterFileName))
            {
                _parameters[Constants.FilterFileName] = parameters.FilterFileName;
            }

            if (!string.IsNullOrWhiteSpace(parameters.InputFile))
            {
                _parameters[Constants.InputFile] = parameters.InputFile;
            }

            if (!string.IsNullOrWhiteSpace(parameters.OutputFile))
            {
                _parameters[Constants.OutputFile] = parameters.OutputFile;
            }
            else
            {
                _parameters[Constants.OutputFile] = CommandLineParameterAux.CreateResultFileName(parameters.InputFile);
            }

            if (parameters.LogHttp400)
            {
                _parameters[Constants.LogHttp400] = !(bool)_parameters[Constants.LogHttp400];
            }

            if (parameters.LogHttp500)
            {
                _parameters[Constants.LogHttp500] = !(bool)_parameters[Constants.LogHttp500];
            }

            if (parameters.FilterStaticRequests)
            {
                _parameters[Constants.FilterStaticRequests] = !(bool)_parameters[Constants.FilterStaticRequests];
            }

            if (parameters.HideEmptyIntervals)
            {
                _parameters[Constants.HideEmptyIntervals] = !(bool)_parameters[Constants.HideEmptyIntervals];
            }

            if (parameters.Verbose)
            {
                _parameters[Constants.Verbose] = !(bool)_parameters[Constants.Verbose];
            }

            if (parameters.Filter300)
            {
                _parameters[Constants.Filter300] = !(bool)_parameters[Constants.Filter300];
            }

            _initialized = true;
        }

        public int GetValueAsInteger(int parameterCode)
        {
            return (int)_parameters[parameterCode];
        }

        public string GetValueAsString(int parameterCode)
        {
            return (string)_parameters[parameterCode];
        }

        public bool GetValueAsBool(int parameterCode)
        {
            return (bool)_parameters[parameterCode];
        }

        public void SetValue(int parameterCode, object value)
        {
            if (_parameters.ContainsKey(parameterCode))
            {
                _parameters[parameterCode] = value;
            }
            else
            {
                _parameters.Add(parameterCode, value);
            }
        }
    }
}
