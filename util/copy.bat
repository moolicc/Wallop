@ECHO OFF

REM Copies the contents of the output directory of a plugin to the output directory of the main application.
REM The following command line arguments are used.
REM [source directory (output of plugin project)] [configuration (debug/release)] [plugin folder name]


REM Gather variables

SET sourcedir=%1
SET configuration=%2
SET pluginfolder=%3
SET destdir="..\..\Wallop\bin\%configuration%\net6.0\plugins\%pluginfolder%\"

SET sourcePattern=".\bin\%configuration%\net6.0\*.dll"

echo Source pattern:
echo %sourcePattern%

echo Destination directory:
echo %destdir%


REM Perform copy

IF NOT EXIST %destdir% mkdir %destdir%
copy %sourcePattern% %destdir%


REM Perform trim of common dependencies

SET stddir="..\..\Wallop\bin\%configuration%\net6.0\"
SET /A trimcount=0
FOR %%A IN (%stddir%*.dll) DO (
    IF EXIST %destdir%\%%~nxA DEL /Q /F %destdir%\%%~nxA
    echo Trimming dependency %%~nxA
    SET /A trimcount += 1
)

echo Trimmed %trimcount% dependencies.