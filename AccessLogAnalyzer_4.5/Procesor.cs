using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Abstracta.AccessLogAnalyzer.DataExtractors;

namespace Abstracta.AccessLogAnalyzer
{
    public class Procesor
    {
        public static List<Interval> ProcessAccessLog(BackgroundWorker worker, GuiParameters p)
        {
            return ProcessAccessLog(worker, p.IntervaloDefinido, p.Top, p.LogFileName, p.DataLineExtractor, p.LogHTTP500List, p.LogHTTP400List, p.FilterStaticReqs, p.Filter300, p.Verbose);
        }

        public static List<Interval> ProcessAccessLog(BackgroundWorker worker, IntervalSize intervalSizeDefined, TopTypes top, string logFileName, DataExtractor format, bool logHTTP500List, bool logHTTP400List, bool filterStaticRequests, bool filter300, bool verbose)
        {
            Logger.GetInstance().Verbose = verbose;

            if (!File.Exists(logFileName))
            {
                throw new Exception(@"File doesn't exists: " + logFileName);
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

            // Create memory structures
            var initialSize = Interval.CalculateInitialSize(intervalSizeDefined);
            var intervals = new List<Interval>(initialSize);
            var minutes = Interval.GetMinutesFromInterval(intervalSizeDefined);

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

                    intervals = AddAccessLog(accessLog, intervals, minutes, intervalSizeDefined, top, logHTTP500List, logHTTP400List);

                    linesProcessed++;
                    if (reportProcessStatus && linesProcessed % onePercent == 0)
                    {  
                        worker.ReportProgress(linesProcessed / onePercent);
                    }
                }
            }

            return intervals;
        }

        public static void SaveResultToFile(List<Interval> result, GuiParameters p)
        {
            SaveResultToFile(result, p.ResultFileName, p.Top, p.IntervaloDefinido, p.HideEmptyIntervals, p.LogHTTP500List, p.LogHTTP400List);
        }

        public static void SaveResultToFile(List<Interval> result, string fileName, TopTypes top, IntervalSize intervalSize, bool hideEmptyIntervals, bool logHTTP500List, bool logHTTP400List)
        {
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
                file.WriteLine(Interval.ToStringHeader);

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
                
                // Write line
                file.WriteLine(string.Empty);
                file.WriteLine("TOP most slow URLs");
                file.WriteLine("------------------------------------------------------------------------------------------");

                // write top of each interval
                var urls = new Dictionary<string, int>();
                foreach (var interval in result)
                {
                    foreach (var accessLog in interval.TopOfInterval)
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

                // sorting urls
                var sortedUrls = new Dictionary<string, int>(urls.Count);
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
                    sortedUrls.Add(url, count);
                }

                file.WriteLine("URLs that are in the top list of all intervals, and the count of them (just in the top list)");
                file.WriteLine("------------------------------------------------------------------------------------------");

                file.WriteLine(String.Join("\t", sortedUrls.Keys));
                file.WriteLine(String.Join("\t", sortedUrls.Values));

                if (logHTTP500List)
                {

                    file.WriteLine(string.Empty);
                    file.WriteLine("All URLS with response code HTTP 500");
                    file.WriteLine(
                        "------------------------------------------------------------------------------------------");

                    // Write all HTTP 500
                    foreach (var accessLog in result.SelectMany(
                        interval => (from accessLog in interval.LogsHTTP500OfInterval
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
                        interval => (from accessLog in interval.LogsHTTP400OfInterval
                                     let strAccessLog = accessLog.ToString()
                                     where strAccessLog != string.Empty
                                     select accessLog)))
                    {
                        file.WriteLine(accessLog);
                    }
                }
            }

            Logger.GetInstance().SaveLogsToFile(fileName + "-LOG.txt");
        }

        private static List<Interval> AddAccessLog(AccessLog accessLog, List<Interval> intervals, int minutes, IntervalSize intervalSizeDefined, TopTypes top, bool logHTTP500List, bool logHTTP400List)
        {
            var intervalOfRequest = FindIntervalOfRequest(intervals, accessLog, minutes);

            // if the interval doesn't exist, create all the invervals needed
            if (intervalOfRequest == null)
            {
                if (intervals.Count == 0)
                {
                    var iniTime = accessLog.Time;
                    intervals.Add(new Interval(iniTime, intervalSizeDefined, top, logHTTP500List, logHTTP400List));
                    intervalOfRequest = intervals[0];
                }
                else
                {
                    if (accessLog.Time < intervals[0].StartInterval)
                    {
                        intervals = AddIntervalsToBackwards(intervals, accessLog, intervalSizeDefined, top, logHTTP500List, logHTTP400List);
                    }
                    else if (accessLog.Time >= intervals[intervals.Count - 1].EndInterval)
                    {
                        intervals = AddIntervalsToForwards(intervals, accessLog, intervalSizeDefined, top, logHTTP500List, logHTTP400List);
                    }
                    else
                    {
                        throw new Exception("Intervals corrupted, can't add point: " + accessLog);
                    }

                    intervalOfRequest = FindIntervalOfRequest(intervals, accessLog, minutes);
                }
            }

            intervalOfRequest.Add(accessLog);

            return intervals;
        }

        private static List<Interval> AddIntervalsToBackwards(IList<Interval> intervals, AccessLog accessLog, IntervalSize intervalSizeDefined, TopTypes top, bool logHTTP500List, bool logHTTP400List)
        {
            // todo test this method
            var result = new List<Interval>();

            var intervalSize = Interval.GetMinutesFromInterval(intervalSizeDefined);

            var intervalsToAdd = (((intervals[0].StartInterval - accessLog.Time).TotalMinutes) / intervalSize) + 1;
            for (var i = 0; i < intervalsToAdd; i++)
            {
                var iniTime = accessLog.Time.AddMinutes(i * intervalSize);
                result.Add(new Interval(iniTime, intervalSizeDefined, top, logHTTP500List, logHTTP400List));
            }

            result.AddRange(intervals);

            return result;
        }

        private static List<Interval> AddIntervalsToForwards(List<Interval> intervals, AccessLog accessLog, IntervalSize intervalSizeDefined, TopTypes top, bool logHTTP500List, bool logHTTP400List)
        {
            var intervalSize = Interval.GetMinutesFromInterval(intervalSizeDefined);

            var lastInterval = intervals[intervals.Count - 1];

            var intervalsToAdd = (((accessLog.Time - lastInterval.EndInterval).TotalMinutes) / intervalSize) + 1;
            for (var i = 1; i <= intervalsToAdd; i++)
            {
                var iniTime = lastInterval.StartInterval.AddMinutes(i * intervalSize);
                intervals.Add(new Interval(iniTime, intervalSizeDefined, top, logHTTP500List, logHTTP400List));
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
