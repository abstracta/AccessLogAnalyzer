set exePath=..

set fileName="access_log.txt"

rem 10.82.8.95 - - [02/Nov/2013:21:00:00 -0300] "HEAD / HTTP/1.1" 200 - 617
"%exePath%\Abstracta.ReplaceAll_NET4.0.exe" -i "%fileName%" -x -r "(^\S+) \S+ \S+ \[(\S+):(\d\d.\d\d.\d\d) -\d+] \"(\S+ \S+) HTTP\S+ (\S+) (\S+) (\S+$)" -t -w "$1\t$2 $3\t$4\t$5\t$6\t$7"

rem 10.82.8.95	02/Nov/2013 21:00:00	HEAD /	200	-	617
"%exePath%\Abstracta.ReplaceAll_NET4.0.exe" -i "%fileName%" -r "/Nov/" -w "/11/"

pause