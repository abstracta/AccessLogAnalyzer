using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Abstracta.AccessLogAnalyzer
{
    public class Procesor
    {
        public static List<Interval> ProcessAccessLog(BackgroundWorker worker, GuiParameters p, LoggerType logType = LoggerType.File)
        {
            ServersManager.Reset();

            Logger.GetInstance().Verbose = p.Verbose;

            var top = p.Top;
            var logHTTP500List = p.LogHTTP500List;
            var logHTTP400List = p.LogHTTP400List;
            var intervalSizeDefined = p.IntervaloDefinido;

            var serverNames = p.Servers.Select(server => server.ServerName).ToList();

            // Create memory structures
            var initialSize = Interval.CalculateInitialSize(intervalSizeDefined);
            var intervals = new List<Interval>(initialSize);
            var minutes = Interval.GetMinutesFromInterval(intervalSizeDefined);

            // for each server, process its files
            foreach (var serverParameters in p.Servers)
            {
                Logger.GetInstance().AddLog(@"Start Process server info: " + serverParameters.ServerName);

                var serverName = serverParameters.ServerName;
                var serverIndex = ServersManager.AddServer(serverName);
                var format = serverParameters.DataLineExtractor;

                if (format == null)
                {
                    Logger.GetInstance().AddLog("Server hasn't a DataLineExtractor correctly defined");
                    continue;
                }

                var filter300 = serverParameters.Filter300;
                var filterStaticRequests = serverParameters.FilterStaticReqs;

                foreach (var fName in serverParameters.LogFileNames)
                {
                    var logFileName = fName;

                    if (!File.Exists(logFileName))
                    {
                        var currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                        logFileName = currentPath + "\\" + logFileName;
                    }

                    if (!File.Exists(logFileName))
                    {
                        Logger.GetInstance().AddLog(@"File doesn't exists: " + logFileName);
                        continue;
                    }

                    // to give feedback to the user about the processing state
                    var onePercent = int.MaxValue;
                    var reportProcessStatus = false;
                    if (worker != null)
                    {
                        var totalLines = File.ReadLines(logFileName).Count();
                        reportProcessStatus = totalLines > 100000;
                        onePercent = totalLines/100;
                    }

                    Logger.GetInstance().AddLog("Start procesing file: " + logFileName);

                    // read the access log file and group all requests by interval
                    using (var file = new StreamReader(logFileName))
                    {
                        // create 'n' threads, pass two objetcs: GetLine() and AddAccessLog(), both are sinc. methods
                        // each thread gets a line and creates an AccessLog, then saves it
                        var linesProcessed = 0;
                        string line;
                        while ((line = file.ReadLine()) != null)
                        {
                            format.SetLine(line);
                            var accessLog = AccessLog.CreateFromLine(format, filterStaticRequests, filter300);
                            if (accessLog == null)
                            {
                                // if format of line is unknown, then the line is discarded
                                // if url is filtered, then the line is discarded
                                continue;
                            }

                            intervals = AddAccessLog(serverIndex, serverNames, accessLog, intervals, minutes,
                                                     intervalSizeDefined, top, logHTTP500List, logHTTP400List);

                            linesProcessed++;
                            if (reportProcessStatus && linesProcessed%onePercent == 0)
                            {
                                worker.ReportProgress(linesProcessed/onePercent);
                            }
                        }
                    }

                    Logger.GetInstance().AddLog("File processed: " + logFileName);
                }

                Logger.GetInstance().AddLog(@"Server info processed: " + serverParameters.ServerName);
            }

            Logger.GetInstance().SaveLogsToFile();

            return intervals;
        }

        public static void SaveResultToFile(List<Interval> result, GuiParameters p)
        {
            var top = p.Top;
            var fileName = p.ResultFileName;
            var intervalSize = p.IntervaloDefinido;
            var hideEmptyIntervals = p.HideEmptyIntervals;
            var logHTTP500List = p.LogHTTP500List;
            var logHTTP400List = p.LogHTTP400List;

            var serverNames = p.Servers.Select(server => server.ServerName).ToList();

            if (result == null)
            {
                return;
            }

            using (var file = new StreamWriter(fileName))
            {
                // remove last empty values
                var i = result.Count - 1;
                while (result[i].IsEmpty())
                {
                    result.RemoveAt(i);
                    i--;
                }

                // write info about the log file
                file.WriteLine("Interval Size: {0}", intervalSize);
                file.WriteLine("Top: {0}", top);
                file.WriteLine("Intervals: {0}", result.Count);

                file.WriteLine();
                file.WriteLine("Filters:");
                file.WriteLine("------------------------------------------------------------------------------------------");
                foreach (var filter in URLFilterSingleton.GetInstance().GetOnlyFilters())
                {
                    file.WriteLine(URLFilterSingleton.OnlyPrefix + filter);
                }

                foreach (var filter in URLFilterSingleton.GetInstance().GetDiscardFilters())
                {
                    file.WriteLine(URLFilterSingleton.DiscardPrefix + filter);
                }

                file.WriteLine();
                file.WriteLine("Stadistical Information");
                file.WriteLine("------------------------------------------------------------------------------------------");

                // write headers of resume of intervals
                file.WriteLine(Interval.GetStringHeader(serverNames));

                // write resume of intervals
                if (hideEmptyIntervals)
                {
                    foreach (var interval in result.Where(interval => !interval.IsEmpty()))
                    {
                        file.WriteLine(interval);
                    }
                }
                else
                {
                    foreach (var interval in result)
                    {
                        file.WriteLine(interval);
                    }
                }

                foreach (var serverName in serverNames)
                {
                    // write top of each interval
                    var urls = new Dictionary<string, int>();

                    file.WriteLine(string.Empty);
                    file.WriteLine("SERVER: " + serverName);
                    file.WriteLine("---------------------------------------");

                    // Write line
                    file.WriteLine(string.Empty);
                    file.WriteLine("TOP most slow URLs");
                    file.WriteLine("------------------------------------------------------------------------------------------");

                    var serverIndex = ServersManager.GetServerIndex(serverName);

                    foreach (var interval in result)
                    {
                        foreach (var accessLog in interval.GetTopOfInterval(serverIndex))
                        {
                            var strAccessLog = accessLog.ToString();
                            if (strAccessLog != string.Empty)
                            {
                                file.WriteLine(accessLog);
                            }

                            if (urls.ContainsKey(accessLog.URL))
                            {
                                urls[accessLog.URL]++;
                            }
                            else
                            {
                                urls.Add(accessLog.URL, 1);
                            }
                        }
                    }

                    // Write the list of the TOP URLs, and the total count of them
                    file.WriteLine(string.Empty);

                    var urlNames = new StringBuilder();
                    var counters = new StringBuilder();
                    while (urls.Count > 0)
                    {
                        var url = string.Empty;
                        var count = 0;

                        foreach (var key in urls.Keys)
                        {
                            if (urls[key] <= count) continue;

                            url = key;
                            count = urls[key];
                        }

                        urls.Remove(url);
                        
                        urlNames.Append(url + "\t");
                        counters.Append(count + "\t");
                    }

                    file.WriteLine("URLs that are in the top list of all intervals, and the count of them (just in the top list)");
                    file.WriteLine("------------------------------------------------------------------------------------------");

                    file.WriteLine(urlNames);
                    file.WriteLine(counters);

                    if (logHTTP500List)
                    {
                        file.WriteLine(string.Empty);
                        file.WriteLine("All URLS with response code HTTP 500");
                        file.WriteLine("------------------------------------------------------------------------------------------");

                        // Write all HTTP 500
                        foreach (var accessLog in result.SelectMany(
                            interval => (from accessLog in interval.GetLogsHTTP500OfInterval(serverIndex)
                                         let strAccessLog = accessLog.ToString()
                                         where strAccessLog != string.Empty
                                         select accessLog)))
                        {
                            file.WriteLine(accessLog);
                        }
                    }

                    if (logHTTP400List)
                    {
                        file.WriteLine(string.Empty);
                        file.WriteLine("All URLS with response code HTTP 400");
                        file.WriteLine(
                            "------------------------------------------------------------------------------------------");

                        // Write all HTTP 400
                        foreach (var accessLog in result.SelectMany(
                            interval => (from accessLog in interval.GetLogsHTTP400OfInterval(serverIndex)
                                         let strAccessLog = accessLog.ToString()
                                         where strAccessLog != string.Empty
                                         select accessLog)))
                        {
                            file.WriteLine(accessLog);
                        }
                    }
                }
            }

            Logger.GetInstance().SaveLogsToFile();
        }

        private static List<Interval> AddAccessLog(int serverIndex, List<string> serverNames, AccessLog accessLog, List<Interval> intervals, int minutes, IntervalSize intervalSizeDefined, TopTypes top, bool logHTTP500List, bool logHTTP400List)
        {
            var intervalOfRequest = FindIntervalOfRequest(intervals, accessLog, minutes);

            // if the interval doesn't exist, create all the invervals needed
            if (intervalOfRequest == null)
            {
                if (intervals.Count == 0)
                {
                    var iniTime = accessLog.Time;
                    intervals.Add(new Interval(serverNames, iniTime, intervalSizeDefined, top, logHTTP500List, logHTTP400List));
                    intervalOfRequest = intervals[0];
                }
                else
                {
                    if (accessLog.Time < intervals[0].StartInterval)
                    {
                        intervals = AddIntervalsToBackwards(serverNames, intervals, accessLog, intervalSizeDefined, top, logHTTP500List, logHTTP400List);
                    }
                    else if (accessLog.Time >= intervals[intervals.Count - 1].EndInterval)
                    {
                        intervals = AddIntervalsToForwards(serverNames, intervals, accessLog, intervalSizeDefined, top, logHTTP500List, logHTTP400List);
                    }
                    else
                    {
                        throw new Exception("Intervals corrupted, can't add point: " + accessLog);
                    }

                    intervalOfRequest = FindIntervalOfRequest(intervals, accessLog, minutes);
                }
            }

            intervalOfRequest.Add(serverIndex, accessLog);

            return intervals;
        }

        private static List<Interval> AddIntervalsToBackwards(List<string> serverNames, IList<Interval> intervals, AccessLog accessLog, IntervalSize intervalSizeDefined, TopTypes top, bool logHTTP500List, bool logHTTP400List)
        {
            var result = new List<Interval>();

            var intervalSize = Interval.GetMinutesFromInterval(intervalSizeDefined);

            var intervalsToAdd = (((intervals[0].StartInterval - accessLog.Time).TotalMinutes) / intervalSize) + 1;
            for (var i = 0; i < intervalsToAdd; i++)
            {
                var iniTime = accessLog.Time.AddMinutes(i * intervalSize);
                result.Add(new Interval(serverNames, iniTime, intervalSizeDefined, top, logHTTP500List, logHTTP400List));
            }

            result.AddRange(intervals);

            return result;
        }

        private static List<Interval> AddIntervalsToForwards(List<string> serverNames, List<Interval> intervals, AccessLog accessLog, IntervalSize intervalSizeDefined, TopTypes top, bool logHTTP500List, bool logHTTP400List)
        {
            var intervalSize = Interval.GetMinutesFromInterval(intervalSizeDefined);

            var lastInterval = intervals[intervals.Count - 1];

            var intervalsToAdd = (((accessLog.Time - lastInterval.EndInterval).TotalMinutes) / intervalSize) + 1;
            for (var i = 1; i <= intervalsToAdd; i++)
            {
                var iniTime = lastInterval.StartInterval.AddMinutes(i * intervalSize);
                intervals.Add(new Interval(serverNames, iniTime, intervalSizeDefined, top, logHTTP500List, logHTTP400List));
            }

            return intervals;
        }

        private static Interval FindIntervalOfRequest(IList<Interval> intervals, AccessLog accessLog, int minutesOfInterval)
        {
            // if there are no intervals
            if (intervals.Count == 0)
            {
                return null;
            }

            // if the point is out of the whole range
            if (accessLog.Time < intervals[0].StartInterval || accessLog.Time >= intervals[intervals.Count-1].EndInterval)
            {
                return null;
            }

            // find the interval inside the whole range
            var index = Convert.ToInt32(Math.Truncate((accessLog.Time - intervals[0].StartInterval).TotalMinutes)) / minutesOfInterval;
            return intervals[index];
        }
    }
}
