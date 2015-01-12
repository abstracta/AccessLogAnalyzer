using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Abstracta.AccessLogAnalyzer.DataExtractors;

namespace Abstracta.AccessLogAnalyzer
{
    public class ConfigurationManager
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
                    // general configuration parameters
                    { Constants.FilterFileName, Constants.FilterFileNameDefaultValue },
                    { Constants.OutputFile,     Constants.OutputFileDefaultValue },
                    
                    { Constants.Interval,       Constants.IntervalDefaultValue },
                    { Constants.Top,            Constants.TopDefaultValue },
                    { Constants.LogHttp400,     Constants.LogHttp400DefaultValue },
                    { Constants.LogHttp500,     Constants.LogHttp500DefaultValue },
                    { Constants.HideEmptyIntervals,     Constants.HideEmptyIntervalsDefaultValue },
                    { Constants.Verbose,                Constants.VerboseDefaultValue },

                    { Constants.Servers,                null },

                    ////// server configuration parameters
                    ////{ Constants.ServerType,     Constants.ServerTypeDefaultValue },
                    ////{ Constants.LineFormat,     Constants.LineFormatDefaultValue },
                    ////{ Constants.InputFile,      Constants.InputFileDefaultValue },
                    
                    ////{ Constants.FilterStaticRequests,   Constants.FilterStaticRequestDefaultValue },
                    ////{ Constants.Filter300,              Constants.Filter300DefaultValue },
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
        public void Initialize(AbstractCommandLineParameters parameters)
        {
            if (_initialized)
            {
                throw new Exception("Configuration Manager already initialized!");
            }

            if (!string.IsNullOrEmpty(parameters.ConfigFileName))
            {
                LoadXMLConfigFile(parameters.ConfigFileName);
            }
            else
            {
                LoadParameters(parameters);
            }

            _initialized = true;
        }

        private void LoadParameters(AbstractCommandLineParameters parameters)
        {
            if (parameters.Interval != 0)
            {
                _parameters[Constants.Interval] = parameters.Interval;
            }

            if (parameters.Top != 0)
            {
                _parameters[Constants.Top] = parameters.Top;
            }

            if (!string.IsNullOrWhiteSpace(parameters.FilterFileName))
            {
                _parameters[Constants.FilterFileName] = parameters.FilterFileName;
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

            if (parameters.HideEmptyIntervals)
            {
                _parameters[Constants.HideEmptyIntervals] = !(bool)_parameters[Constants.HideEmptyIntervals];
            }

            if (parameters.Verbose)
            {
                _parameters[Constants.Verbose] = !(bool)_parameters[Constants.Verbose];
            }

            // using parameters just one server can be configured
            var filter300 = parameters.Filter300;
            var filterStaticRequests = parameters.FilterStaticRequests;
            var format = string.IsNullOrEmpty(parameters.LineFormat)
                             ? Constants.LineFormatDefaultValue
                             : parameters.LineFormat;
            var serverType = parameters.ServerType;

            DataExtractor dateLineExtractor = null;
            try
            {
                dateLineExtractor = DataExtractor.CreateDataExtractor(serverType, format);
            }
            catch (Exception)
            {
                Logger.GetInstance().AddLog("Can't create dataExtractor for serverType(" + serverType + ") from line format: " + format);
            }

            var serverDefinition = new ServerParameters
            {
                DataLineExtractor = dateLineExtractor,
                Filter300 = filter300,
                FilterStaticReqs = filterStaticRequests,
                LogFileNames = new List<string> { parameters.InputFile },
                ServerType = serverType,
                ServerName = serverType == ServerType.Apache
                                 ? "APACHE"
                                 : serverType == ServerType.Tomcat
                                       ? "TOMCAT"
                                       : serverType == ServerType.IIS ? "IIS" : "SERVER",
            };

            _parameters[Constants.Servers] = new List<ServerParameters> { serverDefinition };
        }

        private void LoadXMLConfigFile(string configFileName)
        {
            if (!File.Exists(configFileName))
            {
                throw new Exception("Configuration File doesn't exists: " + configFileName);
            }

            var doc = new XmlDocument();
            doc.Load(configFileName);
            var configTag = doc.GetElementsByTagName("configuration")[0];

            if (configTag == null)
            {
                throw new Exception("Configuration File doesn't have 'configuration' tag: " + configFileName);
            }

            if (configTag.Attributes == null)
            {
                throw new Exception("'configuration' tag doesn't have attributes: " + "<configuration startNow=\"\" top=\"\" interval=\"\" outFile=\"\" logHTTP500List=\"\" logHTTP400List=\"\" hideEmptyIntervals=\"\" verbose=\"\">");
            }

            LoadIntValueFromAttributeList(configTag.Attributes, "top", Constants.Top);
            LoadIntValueFromAttributeList(configTag.Attributes, "interval", Constants.Interval);
            LoadBoolValueFromAttributeList(configTag.Attributes, "logHTTP500List", Constants.LogHttp500);
            LoadBoolValueFromAttributeList(configTag.Attributes, "logHTTP400List", Constants.LogHttp400);
            LoadBoolValueFromAttributeList(configTag.Attributes, "hideEmptyIntervals", Constants.HideEmptyIntervals);
            LoadBoolValueFromAttributeList(configTag.Attributes, "verbose", Constants.Verbose);

            LoadStringValueFromAttributeList(configTag.Attributes, "outFile", Constants.OutputFile);
            LoadStringValueFromAttributeList(configTag.Attributes, "filtersFile", Constants.FilterFileName);

            var serversDef = new List<ServerParameters>();

            var servers = doc.GetElementsByTagName("server");
            foreach (var server in servers)
            {
                var srvNode = server as XmlNode;

                if (srvNode == null) continue;

                var name = GetValueFromAttributeList(srvNode.Attributes, "name");
                var serverType = GetValueFromAttributeList(srvNode.Attributes, "serverType");
                var formatLine = GetValueFromAttributeList(srvNode.Attributes, "formatLine");

                var filter300 = GetValueFromAttributeList(srvNode.Attributes, "filter300").ToLower() == "true";
                var filterStaticRequest = GetValueFromAttributeList(srvNode.Attributes, "filterStaticRequest").ToLower() == "true";

                var fileNames = new List<string>();

                foreach (var childNode in srvNode.ChildNodes)
                {
                    var fileDef = childNode as XmlNode;
                    if (fileDef == null) continue;

                    if (fileDef.Name.ToLower() != "server.logfile") continue;

                    var logFilePath = GetValueFromAttributeList(fileDef.Attributes, "path");
                    var logFileName = GetValueFromAttributeList(fileDef.Attributes, "fileName");

                    if (!(logFilePath.EndsWith("\\") || logFilePath.EndsWith("/")))
                    {
                        logFilePath += "/";
                    }

                    fileNames.Add(logFilePath + logFileName);
                }

                var serverDefinition = new ServerParameters
                    {
                        ServerName = name,
                        ServerType = DataExtractor.GetServerTypeFromString(serverType),
                        DataLineExtractor = DataExtractor.CreateDataExtractor(serverType, formatLine),
                        Filter300 = filter300,
                        FilterStaticReqs = filterStaticRequest,
                        LogFileNames = fileNames,
                    };

                serversDef.Add(serverDefinition);
            }

            _parameters[Constants.Servers] = serversDef;
        }

        private static string GetValueFromAttributeList(XmlAttributeCollection attributes, string attrName)
        {
            if (attributes == null)
            {
                throw new Exception("Tag doesn't have attributes: ");
            }

            if (attributes[attrName] == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(attributes[attrName].Value))
            {
                return null;
            }

            return attributes[attrName].Value;
        }

        private void LoadIntValueFromAttributeList(XmlAttributeCollection attributes, string attrName, int arrayIndex)
        {
            var value = GetValueFromAttributeList(attributes, attrName);
            if (value == null)
            {
                return;
            }

            try
            {
                _parameters[arrayIndex] = int.Parse(value);
            }
            catch (Exception e)
            {
                throw new Exception("'" + attrName + "' attribute isn't an integer value: " + attributes[attrName].Value, e);
            }
        }

        private void LoadBoolValueFromAttributeList(XmlAttributeCollection attributes, string attrName, int arrayIndex)
        {
            var value = GetValueFromAttributeList(attributes, attrName);
            if (value == null)
            {
                return;
            }

            try
            {
                _parameters[arrayIndex] = value.ToLower() == "true";
            }
            catch (Exception e)
            {
                throw new Exception("'" + attrName + "' attribute isn't a boolean value: " + attributes[attrName].Value, e);
            }
        }

        private void LoadStringValueFromAttributeList(XmlAttributeCollection attributes, string attrName, int arrayIndex)
        {
            var value = GetValueFromAttributeList(attributes, attrName);
            if (value == null)
            {
                return;
            }

            _parameters[arrayIndex] = attributes[attrName].Value;
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

        public ServerType GetValueAsServerType(int parameterCode)
        {
            return (ServerType)_parameters[parameterCode];
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

        public string GetLineFormat(string serverName)
        {
            var serverType = GetServerTypeOfServer(serverName);
            return DataExtractor.ParametersOfServerType(serverType);
        }

        private ServerType GetServerTypeOfServer(string serverName)
        {
            var servers = (List<ServerParameters>)_parameters[Constants.Servers];
            foreach (var serverParameterse in servers.Where(serverParameterse => serverParameterse.ServerName == serverName))
            {
                return serverParameterse.ServerType;
            }

            throw new Exception("Unknown server: " + serverName);
        }

        public List<ServerParameters> GetListOfServerDefinitions()
        {
            return (List<ServerParameters>) _parameters[Constants.Servers];
        }

        public bool ServerTypeNeedsParameters(ServerType serverType)
        {
            return DataExtractor.ServerTypeNeedsParameters(serverType);
        }
    }
}
