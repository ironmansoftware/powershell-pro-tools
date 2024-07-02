REM set msbuild path
set MsbuildBin=%ProgramFiles(x86)%\MSBuild\12.0\Bin
set POSHTOOLROOT=%~dp0

REM msbuild.exe to the command path
set PATH=%MsbuildBin%;%PATH%
