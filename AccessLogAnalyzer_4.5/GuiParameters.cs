using System.Collections.Generic;
using Abstracta.AccessLogAnalyzer.DataExtractors;

namespace Abstracta.AccessLogAnalyzer
{
    public class GuiParameters
    {
        public IntervalSize IntervaloDefinido { get; set; }

        public TopTypes Top { get; set; }

        public bool LogHTTP400List { get; set; }

        public bool LogHTTP500List { get; set; }

        public bool Verbose { get; set; }

        public bool HideEmptyIntervals { get; set; }

        public string ResultFileName { get; set; }

        public List<ServerParameters> Servers { get; set; }
    }

    public class ServerParameters
    {
        public string ServerName { get; set; }

        public ServerType ServerType { get; set; }

        public List<string> LogFileNames { get; set; }
        
        public DataExtractor DataLineExtractor { get; set; }

        public bool Filter300 { get; set; }
        
        public bool FilterStaticReqs { get; set; }
    }
}