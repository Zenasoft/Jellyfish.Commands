  
call dnu restore

IF '%Configuration%' == '' (
  call dnu pack src\Jellyfish.Commands --configuration Release
) ELSE (
  call dnu pack src\Jellyfish.Commands --configuration %Configuration%
)

cd test\Jellyfish.Commands.test
call dnx test 

cd ..\..