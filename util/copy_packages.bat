@ECHO OFF

REM The following command line arguments are used.
REM [output directory (output of plugin project)]

REM Gather variables

SET destdir=%1

SET sourcePattern="..\..\modules"

echo Source directory:
echo %sourcePattern%

echo Destination directory:
echo %destdir%


REM Perform copy

IF NOT EXIST %destdir% mkdir %destdir%
ROBOCOPY %sourcePattern% %destdir% /E

EXIT 0