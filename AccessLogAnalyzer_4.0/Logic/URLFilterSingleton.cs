using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AccessLogAnalyzer.Logic
{
    internal class URLFilterSingleton
    {
        internal const string OnlyPrefix = "ONLY:";

        internal const string DiscardPrefix = "DISCARD:";
        
        private static volatile URLFilterSingleton _instance;

        private static readonly object Lock = new object();

        private List<string> _onlyTextFilters = new List<string>();

        private List<string> _discardTextFilters = new List<string>();

        private URLFilterSingleton()
        {
            var fileName = ConfigurationManager.GetInstance().GetValueAsString(Constants.FilterFileName);

            UpdateFilter(fileName);
        }

        internal static URLFilterSingleton GetInstance()
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new URLFilterSingleton();
                    }
                }
            }

            return _instance;
        }

        internal static string ClearParametersFromURL(string url)
        {
            if (url == null)
            {
                return null;
            }

            var indexOf = url.IndexOf('?');
            return indexOf < 0 ? url : url.Remove(indexOf);
        }

        internal static bool IsStaticResource(string url)
        {
            var s = url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                    || url.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                    || url.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
                    || url.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
                    || url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);

            return s;

            /*
            if (url.Contains(".css?") || url.Contains(".js?"))
            {
                return true;
            }
            //*/
        }

        internal void UpdateFilter(string newFileName)
        {
            _onlyTextFilters = new List<string>();
            _discardTextFilters = new List<string>();

            var fileName = newFileName;

            if (String.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            if (!File.Exists(fileName))
            {
                throw new Exception("FilterFile doesn't exists");
            }

            using (var file = new StreamReader(fileName))
            {
                string line;

                while ((line = file.ReadLine()) != null)
                {
                    if (line.StartsWith(OnlyPrefix))
                    {
                        _onlyTextFilters.Add(line.Replace(OnlyPrefix, string.Empty).Trim());
                    }
                    else if (line.StartsWith(DiscardPrefix))
                    {
                        _discardTextFilters.Add(line.Replace(DiscardPrefix, string.Empty).Trim());
                    }
                    else
                    {
                        const string message = "Malformed filtering file. Prefix must be used: { " + OnlyPrefix + ", " +
                                               DiscardPrefix + " }";
                        throw new Exception(message);
                    }
                }
            }
        }

        /// <summary>
        /// Returns true when the URL is discarded according filtering rules
        /// First, the URL must match all the ONLY rules. If ONLY rules is empty, any URL passes this filtering rule.
        /// Second, the URL must match all the DISCARD rules. If url contains the text presented in any DISCARD rule, it's discarded.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Returns true when the URL is discarded according filtering rules</returns>
        internal bool DiscardUrlByFilterRules(string url)
        {
            var url1 = ClearParametersFromURL(url);

            var containsAnyOnlyText = _onlyTextFilters.Count == 0;

            if (!containsAnyOnlyText)
            {
                if (_onlyTextFilters.Any(url1.Contains))
                {
                    containsAnyOnlyText = true;
                }
            }

            return !containsAnyOnlyText || _discardTextFilters.Any(url1.Contains);
        }

        internal int FiltersLoaded()
        {
            return _onlyTextFilters.Count + _discardTextFilters.Count;
        }

        /// <summary>
        /// Creates a copy of the list, and returns it
        /// </summary>
        /// <returns></returns>
        internal List<string> GetOnlyFilters()
        {
            return _onlyTextFilters.ToList();
        }

        /// <summary>
        /// Creates a copy of the list, and returns it
        /// </summary>
        /// <returns></returns>
        internal List<string> GetDiscardFilters()
        {
            return _discardTextFilters.ToList();
        }
    }
}
