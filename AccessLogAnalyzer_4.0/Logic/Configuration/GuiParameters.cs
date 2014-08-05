using AccessLogAnalizer.Logic.Constants;

namespace AccessLogAnalizer.Logic.Configuration
{
    internal class GuiParameters
    {
        public IntervalSize IntervaloDefinido { get; set; }

        public string LogFileName { get; set; }

        public bool FilterStaticReqs { get; set; }

        public bool LogHTTP400List { get; set; }

        public TopTypes Top { get; set; }

        public string ResultFileName { get; set; }

        public bool HideEmptyIntervals { get; set; }

        public bool LogHTTP500List { get; set; }

        public bool Verbose { get; set; }

        public string Format { get; set; }

        public bool Filter300 { get; set; }
    }
}