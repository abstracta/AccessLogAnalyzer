namespace Abstracta.AccessLogAnalyzer
{
    public static class CommandLineParameterAux
    {
        public static string CreateResultFileName(string logFileName)
        {
            const string topStr = "-top.log";

            if (logFileName == null)
            {
                return topStr;
            }

            return logFileName + topStr;
        }
    }
}