
:: \S any character that isn't a white space
:: \d any number character (digit)
:: ^ line start
:: $ line end


..\Abstracta.ReplaceAll_NET4.0.exe -i accessLog_web1_anonimizado.log -x -r "(^\S+) \[(.*) -0300\] (\S+) (\d+) (\d+$)" -w "\t"
..\Abstracta.ReplaceAll_NET4.0.exe -i accessLog_web1_anonimizado.log -r "/Dec/" -w "/12/"
..\Abstracta.ReplaceAll_NET4.0.exe -i accessLog_web1_anonimizado.log -r "/Jan/" -w "/01/"
..\Abstracta.ReplaceAll_NET4.0.exe -i accessLog_web1_anonimizado.log -x -r "(^\S+\t\S+):(\d\d:\d\d:\d\d\t\S+\t\d+\t\d+$)" -w " "


..\..\Abstracta.AccessLogAnalyzer_NET4.0_UI.exe -i "accessLog_web1_anonimizado.log" -s -t 5 -v 10 -l "HOST TIME URL RCODE RTIME" -f "filterURLs.txt" --filterStaticRequests -o "conFiltro.log" -j AccessLogFormat
..\..\Abstracta.AccessLogAnalyzer_NET4.0_UI.exe -i "accessLog_web1_anonimizado.log" -s -t 5 -v 10 -l "HOST TIME URL RCODE RTIME" -f "filterURLs.txt" -o "sinFiltro.log" -j AccessLogFormat

pause
