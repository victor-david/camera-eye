echo Post Build
rem Args are $(ProjectDir) $(TargetDir) $(PlatformName) $(ConfigurationName)
setlocal
set PROJ=%1
set TARGET=%2
set PLATFORM=%3
set CONFIG=%4
if (%PLATFORM%) == (AnyCPU) set PLATFORM=x86

echo Proj is %PROJ%
echo Target is %TARGET%
echo Platform is %PLATFORM%
echo Config is %CONFIG%
echo Nuget is %NUGET_PACKAGES%

echo Copy FFMPEG dlls to %TARGET%
copy %PROJ%Ffmpeg\%PLATFORM%\*.dll %TARGET%

md %TARGET%Plugins

echo Copy plugins to %TARGET%Plugins\
copy %PROJ%..\Restless.Plugin.Amcrest\bin\%CONFIG%\netstandard2.1\*.dll %TARGET%Plugins\
copy %PROJ%..\Restless.Plugin.Axis\bin\%CONFIG%\netstandard2.1\*.dll %TARGET%Plugins\
copy %PROJ%..\Restless.Plugin.Foscam\bin\%CONFIG%\netstandard2.1\*.dll %TARGET%Plugins\
copy %PROJ%..\Restless.Plugin.Framework\bin\%CONFIG%\netstandard2.1\*.dll %TARGET%Plugins\
copy %NUGET_PACKAGES%\rtspclientsharp\1.3.3\lib\netstandard2.0\RtspClientSharp.dll %TARGET%Plugins\
endlocal