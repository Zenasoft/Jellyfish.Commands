call dnvm use 1.0.0-beta8  
call dnu restore
if not "%errorlevel%"=="0" goto failure

IF '%Configuration%' == '' (
  call dnu pack src\Jellyfish.Commands --configuration Release
  call dnu pack src\Jellyfish.Commands.vnext --configuration Release
) ELSE (
  call dnu pack src\Jellyfish.Commands --configuration %Configuration%
  call dnu pack src\Jellyfish.Commands.vnext --configuration %Configuration%
)
if not "%errorlevel%"=="0" goto failure

cd test\Jellyfish.Commands.test
call dnx test 
cd ../..
if not "%errorlevel%"=="0" goto failure

:success
exit 0

:failure
exit -1