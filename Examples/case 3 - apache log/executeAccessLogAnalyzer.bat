
:: \S any character that isn't a white space
:: \d any number character (digit)
:: ^ line start
:: $ line end

set fileName="apache-accessLog.log"

..\Abstracta.ReplaceAll_NET4.0.exe -i %fileName% -x -r "(^\S+) \[(.*) -0300\] (\S+) (\d+) (\d+$)" -w "\t"
..\Abstracta.ReplaceAll_NET4.0.exe -i %fileName% -r "/Dec/" -w "/12/"
..\Abstracta.ReplaceAll_NET4.0.exe -i %fileName% -r "/Jan/" -w "/01/"
..\Abstracta.ReplaceAll_NET4.0.exe -i %fileName% -x -r "(^\S+\t\S+):(\d\d:\d\d:\d\d\t\S+\t\d+\t\d+$)" -w " "


..\..\Abstracta.AccessLogAnalyzer_NET4.0_UI.exe -i %fileName% -t 5 -v 1 -l "HOST TIME URL RCODE RTIME RSIZE MICROSECONDS" -j AccessLogFormat

pause