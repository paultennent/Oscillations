@echo off
if "%1"=="" GOTO :usage
echo Unity folder:%1

echo Building unity web versions to folder %2


Pushd "%~dp0"
start "unitybuild" /high /wait "%1\unity.exe" -quit -batchmode  -projectPath "%~dp0" -executeMethod BuildApk.PerformWebBuilds -targetPath "%2" -logFile .\output.txt

popd
type beep.txt
goto :end

:usage
echo buildweb [path to unity exe] [path to build folder]
echo     [path to unity exe (in editor folder)] e.g. d:\jqm\unity\editor 



:end
