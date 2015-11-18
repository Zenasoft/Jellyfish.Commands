dnvm use 1.0.0-beta8  
dnu restore
if not "%errorlevel%"=="0" goto failure

IF '%Configuration%' == '' (
  dnu pack src\Jellyfish.Commands --configuration Release
  dnu pack src\Jellyfish.Commands.vnext --configuration Release
) ELSE (
  dnu pack src\Jellyfish.Commands --configuration %Configuration%
  dnu pack src\Jellyfish.Commands.vnext --configuration %Configuration%
)
if not "%errorlevel%"=="0" goto failure

cd test\Jellyfish.Commands.test
dnx test 
if not "%errorlevel%"=="0" goto failure
cd ..\..

:success
exit 0

:failure
exit -1