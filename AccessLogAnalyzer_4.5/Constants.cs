﻿using Abstracta.AccessLogAnalyzer.DataExtractors;

namespace Abstracta.AccessLogAnalyzer
{
    public static class Constants
    {
        // --- Property names --- 

        public const int Interval = 0;

        public const int Top = 1;

        //// public const int LineFormat = 2;

        //// public const int InputFile = 3;

        public const int OutputFile = 4;

        public const int HideEmptyIntervals = 5;

        public const int LogHttp400 = 6;

        public const int LogHttp500 = 7;

        public const int FilterFileName = 8;

        //// public const int FilterStaticRequests = 9;

        public const int Verbose = 10;

        //// public const int Filter300 = 11;

        //// public const int ServerType = 12;

        public const int Servers = 13;

        // --- Default values --- 

        public const int IntervalDefaultValue = 10;

        public const int TopDefaultValue = 5;

        public const string FilterFileNameDefaultValue = "";

        public const string LineFormatDefaultValue = "";
        
        public const string InputFileDefaultValue = "";

        public const string OutputFileDefaultValue = "_reportResult.txt";

        public const bool HideEmptyIntervalsDefaultValue = false;

        public const bool LogHttp400DefaultValue = false;

        public const bool LogHttp500DefaultValue = true;

        public const bool FilterStaticRequestDefaultValue = false;

        public const bool VerboseDefaultValue = false;

        public const bool Filter300DefaultValue = false;

        public const ServerType ServerTypeDefaultValue = ServerType.None;

        public const TimeUnitType UnitTypeDefaultValue = TimeUnitType.Milliseconds;
    }
}
