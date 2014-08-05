using System;
using System.Collections.Generic;

namespace AccessLogAnalizer.Logic.Configuration
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
                    { Constants.Constants.Interval,       Constants.Constants.IntervalDefaultValue },
                    { Constants.Constants.Top,            Constants.Constants.TopDefaultValue },
                    { Constants.Constants.LineFormat,     Constants.Constants.LineFormatDefaultValue },
                    { Constants.Constants.FilterFileName, Constants.Constants.FilterFileNameDefaultValue },

                    { Constants.Constants.InputFile,      Constants.Constants.InputFileDefaultValue },
                    { Constants.Constants.OutputFile,     Constants.Constants.OutputFileDefaultValue },

                    { Constants.Constants.LogHttp400,     Constants.Constants.LogHttp400DefaultValue },
                    { Constants.Constants.LogHttp500,     Constants.Constants.LogHttp500DefaultValue },

                    { Constants.Constants.FilterStaticRequests,   Constants.Constants.FilterStaticRequestDefaultValue },
                    { Constants.Constants.Filter300,              Constants.Constants.Filter300DefaultValue },

                    { Constants.Constants.HideEmptyIntervals,     Constants.Constants.HideEmptyIntervalsDefaultValue },

                    { Constants.Constants.Verbose,        Constants.Constants.VerboseDefaultValue },
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
                _parameters[Constants.Constants.Interval] = parameters.Interval;
            }

            if (parameters.Top != 0)
            {
                _parameters[Constants.Constants.Top] = parameters.Top;
            }

            if (!string.IsNullOrWhiteSpace(parameters.LineFormat))
            {
                _parameters[Constants.Constants.LineFormat] = parameters.LineFormat;
            }

            if (!string.IsNullOrWhiteSpace(parameters.FilterFileName))
            {
                _parameters[Constants.Constants.FilterFileName] = parameters.FilterFileName;
            }

            if (!string.IsNullOrWhiteSpace(parameters.InputFile))
            {
                _parameters[Constants.Constants.InputFile] = parameters.InputFile;
            }

            if (!string.IsNullOrWhiteSpace(parameters.OutputFile))
            {
                _parameters[Constants.Constants.OutputFile] = parameters.OutputFile;
            }
            else
            {
                _parameters[Constants.Constants.OutputFile] = CommandLineParameterAux.CreateResultFileName(parameters.InputFile);
            }

            if (parameters.LogHttp400)
            {
                _parameters[Constants.Constants.LogHttp400] = !(bool)_parameters[Constants.Constants.LogHttp400];
            }

            if (parameters.LogHttp500)
            {
                _parameters[Constants.Constants.LogHttp500] = !(bool)_parameters[Constants.Constants.LogHttp500];
            }

            if (parameters.FilterStaticRequests)
            {
                _parameters[Constants.Constants.FilterStaticRequests] = !(bool)_parameters[Constants.Constants.FilterStaticRequests];
            }

            if (parameters.HideEmptyIntervals)
            {
                _parameters[Constants.Constants.HideEmptyIntervals] = !(bool)_parameters[Constants.Constants.HideEmptyIntervals];
            }

            if (parameters.Verbose)
            {
                _parameters[Constants.Constants.Verbose] = !(bool)_parameters[Constants.Constants.Verbose];
            }

            if (parameters.Filter300)
            {
                _parameters[Constants.Constants.Filter300] = !(bool)_parameters[Constants.Constants.Filter300];
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
