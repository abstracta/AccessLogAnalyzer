
AccessLogAnalizer is a tool to parse an AccessLog file in a specific format (use ReplaceAll.exe to format the file) 
to get the 'n' most slow HTTP responses ('n' = top) by interval (specified in minutes). 

It runs in .NET 4.0 and .NET 4.5, and has a Graphic user interface, and a command line interface. 

Future work: 
----------------------------------------------------------------------------
 
 - Graph results in the GUI mode, allow to export to images. 
 * Create charts in new windows.
 - Add multi thread support
 - Add feedback to the user in CLI Mode (lines proceced, etc.)
 - Support Unzip InputFile
 - Support multiple inputfiles (e.g. "FolderPath\*.log")
 - Support analysis of multiple servers
 - Add regular expressions for preprosesing the log lines 
 - Add XML config file to load config. files
 
----------------------------------------------------------------------------


Working: Version 2014.04.16
 - Change RTime from long to double

----------------------------------------------------------------------------

Working: Version 2014.04.16
 - Process file with a BackgroundWorker
 - Add feedback to the user in GUI Mode (lines proceced, etc.)

----------------------------------------------------------------------------

Version 2014.04.14
 - Add support for ResponseEndTime to calculate the ResponseTime

----------------------------------------------------------------------------

 Version 2014.04.02
- Tooltip in GUI mode, for each checkbox, using CLI help text
* Refactor CommandLineParameters class
- If verbose -> create logs, otherwise -> don't
* Refactor Logger class
* Improve memory usage

----------------------------------------------------------------------------

 Version 2014.03.14
- Add filter "Only analize requests that contains.."
* Add "ONLY:" and "DISCARD:" prefix to filters file
* Change URLFilterSingleton class

----------------------------------------------------------------------------

 Version 2014.03.06
- Change key type from string to int in dictionary to improve performance
- Add Filter HTTP RCode 3?? feature
* Add GUI parameter and CLI parameter to configure this

----------------------------------------------------------------------------

  Version 2014.03.05
 - Add option to filter static requests of each interval 
 * When filtering static requests is enable, HTTP requests to images, css and js are discarded from accessLog file.
 - Refactor CommandLineParameters.cs
 - Fix bug when initializing parameters in CLI mode - bool parameters didn't change internal configuration in CLI mode
 - Add verbose CLI parameter to allow logging to file internal events of AccessLogAnalizer

----------------------------------------------------------------------------

 ! MAJOR CHANGE
 ! CLI API changed -> 'f' now is 'l', 'f' means 'filter' and 'l' means 'formatLine'

  Version 2014.02.26 (2)
 - Add filterList of requests
 *  AccessLog CreateFromLine() changed. It returns null when url is filtered.
 
----------------------------------------------------------------------------

  Version 2014.02.26
  - Add statistical info about requests
  * Count requests under { 2, 4, 6, 10, 15, 20, 40, MAX } segs of response time
  * Add 'Time Unit Type' as information in format of Line

  - Improve performance 
  * Change: format split 'line by line' by: format split only once at begin

 ----------------------------------------------------------------------------

Version 2014.02.14
- Add CLI properties to set LogHTTP500, and LogHTTP400 options

 ----------------------------------------------------------------------------

Version 2014.02.13
- Memory usage grows a lot when log file is big
* Change ProcessAccessLog() and SaveResultToFile() methods
* ProcessAccesLog method now receives Top value, to save just the worst AccessLogs of each inteval instead of all of them
* SaveResultToFile method saves all accessLogs of each interval, it doesn't need the Top information

- Add log of each request that response = HTTP 5??

- Add log of each request that response = HTTP 4??
* Add option to add this log or not. By default, process doesn't save this to result file

- Improve A LOT the performance by improving method FindIntervalOfRequest() in Procesor class
* Method finds the interval in order(1) instead of in order(n)

----------------------------------------------------------------------------

Version 2014.02.12
- Fix bug: now TOP worst-times includes responses HTTP 500

----------------------------------------------------------------------------

Version 2014.02.06
- Add log count of HTTP 500, count HTTP 400, and HTTP 200 by interval
* New ToString(), add ToStringHeader string constant
- Refactor Interval: now "LogsOfInterval" is private

----------------------------------------------------------------------------

Version 2014.01.31
- Add support for logs greater than a day. Now it can process logs of any time span size.
* Add AddIntervalsToBackwards, and AddIntervalsToForwards methods
* Improve performance of method FindIntervalOfRequest 
- Save Urls of the TOP list and count of Urls in result file

----------------------------------------------------------------------------

Version 2014.01.28
- Fix border case bug in FindIntervalOfRequest method
- Save Urls list and count of Urls in result file
- 'Remove last empty' values in SaveResultToFile method

----------------------------------------------------------------------------

Version 2014.01.16
- Add HideEmptyIntervals parameter in GUI and CommandLine

----------------------------------------------------------------------------

Version 2014.01.15
- Fix command line behavior. 
* Checks required parameters when Non-GUI mode.
* Fix --help parameter
- Add "\n" between intervals in "SaveResultToFile" method

----------------------------------------------------------------------------

Version 2014.01.13
- Add command line suport

----------------------------------------------------------------------------