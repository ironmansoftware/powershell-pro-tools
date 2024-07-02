set VsVersion=%1
if "%1" == "" set VsVersion=12.0

msbuild "%POSHTOOLROOT%\PowerShellTools.sln" /m /filelogger /verbosity:normal /p:Configuration=Release;VisualStudioVersion=%VsVersion%

