dnvm use 1.0.0-beta8  
CMD /C dnu restore
if NOT ERRORLEVEL 0 EXIT /B 1

IF '%Configuration%' == '' (
  CMD /C dnu pack src\Jellyfish.Commands --configuration Release
  CMD /C dnu pack src\Jellyfish.Commands.vnext --configuration Release
) ELSE (
  CMD /C dnu pack src\Jellyfish.Commands --configuration %Configuration%
  CMD /C dnu pack src\Jellyfish.Commands.vnext --configuration %Configuration%
)
if NOT ERRORLEVEL 0 EXIT /B 1

cd test\Jellyfish.Commands.test
CMD /C dnx test 

cd ..\..

if NOT ERRORLEVEL 0 EXIT /B 1