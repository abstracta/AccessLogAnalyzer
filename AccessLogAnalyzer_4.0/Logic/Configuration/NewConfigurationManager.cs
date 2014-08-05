using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace AccessLogAnalizer.Logic.Configuration
{
    internal class NewConfigurationManager
    {
        internal int Interval { get; set; }

        internal int Top { get; set; }

        internal string OutputFileName { get; set; }

        internal List<Server> Servers { get; set; }

        internal NewConfigurationManager(string configFileName)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(configFileName);

            Interval = GetIntValueFromXML(xmlDocument, "interval", Constants.Constants.IntervalDefaultValue);
            Top = GetIntValueFromXML(xmlDocument, "top", Constants.Constants.TopDefaultValue);
            OutputFileName = GetStringValueFromXML(xmlDocument, "outputFile", Constants.Constants.OutputFileDefaultValue);

            Servers = new List<Server>();
            var servers = xmlDocument.SelectNodes("/accessLogAnalyzer/server");
            if (servers == null)
            {
                throw new Exception("XML configuration file: Need a 'server' tag to process its logs.");
            }

            var serversName = (from object server in servers
                               let xmlNode = server as XmlNode
                               where xmlNode != null
                               let xmlAttributeCollection = xmlNode.Attributes
                               where xmlAttributeCollection != null
                               select xmlAttributeCollection["name"].Value).ToList();

            foreach (var serverName in serversName)
            {
                var server = new Server();
                Servers.Add(server);

                var xPath = "/accessLogAnalyzer/server[@name='" + serverName + "']";
            
                var selectSingleNode = xmlDocument.SelectSingleNode(xPath + "/logFiles");
                if (selectSingleNode == null)
                {
                    throw new Exception("XML configuration file: Need the 'logFiles' tag for server: " + serverName); 
                }

                server.Name = serverName;
                server.LogFiles = selectSingleNode.InnerText;

                selectSingleNode = xmlDocument.SelectSingleNode(xPath + "/filtersFileName");
                if (selectSingleNode != null)
                {
                    var filtesFileName = selectSingleNode.InnerText;
                    if (!string.IsNullOrWhiteSpace(filtesFileName))
                    {
                        server.FiltersFileName = filtesFileName;
                    }
                }

                selectSingleNode = xmlDocument.SelectSingleNode(xPath + "/lineFormat");
                if (selectSingleNode == null)
                {
                    throw new Exception("XML configuration file: Need the 'lineFormat' tag for server: " + serverName);
                }

                server.LineFormat = selectSingleNode.InnerText;

                // todo: keep from here
            }
        }

        private static int GetIntValueFromXML(XmlDocument xmlDocument, string tagName, int defaultValue = -1)
        {
            int result;
            try
            {
                var interval = xmlDocument.GetElementsByTagName(tagName)[0];
                result = int.Parse(interval.InnerText);
            }
            catch (Exception)
            {
                result = defaultValue;
            }

            return result;
        }

        private static string GetStringValueFromXML(XmlDocument xmlDocument, string tagName, string defaultValue = "")
        {
            string result;
            try
            {
                result = xmlDocument.GetElementsByTagName(tagName)[0].ToString();
            }
            catch (Exception)
            {
                result = defaultValue;
            }

            return result;
        }
    }

    internal class Server
    {
        internal string Name { get; set; }

        internal string LogFiles { get; set; }

        internal string FiltersFileName { get; set; }

        internal string LineFormat { get; set; }

        internal List<LineProcesor> LineProcesors { get; set; }
    }

    internal class LineProcesor
    {
        internal string Name { get; set; }

        internal string LogFiles { get; set; }
    }
}
