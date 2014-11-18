
:: \S indica cualquier caracter que no sea "espacio blanco"
:: \d indica cualquier caracter decimal (un dígito numérico)
:: ^ indica principio de línea
:: $ indica fin de línea

set fileName="access_log.txt"

:: ..\Abstracta.ReplaceAll_NET4.0.exe -i %fileName% -x -r "(^\S+) \[(.*) -0300\] (\S+) (\d+) (\d+$)" -w "\t"
:: ..\Abstracta.ReplaceAll_NET4.0.exe -i %fileName% -r "/Dec/" -w "/12/"
:: ..\Abstracta.ReplaceAll_NET4.0.exe -i %fileName% -r "/Jan/" -w "/01/"
:: ..\Abstracta.ReplaceAll_NET4.0.exe -i %fileName% -x -r "(^\S+\t\S+):(\d\d:\d\d:\d\d\t\S+\t\d+\t\d+$)" -w " "


..\..\Abstracta.AccessLogAnalyzer_NET4.0_UI -i %fileName% -t 5 -v 1 -l "HOST TIME URL RCODE RTIME RSIZE MICROSECONDS" -j AccessLogFormat -s

pause
