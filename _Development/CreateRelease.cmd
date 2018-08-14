REM Create a PTM release

IF EXIST ..\Release GOTO NOWINRELEASE
MD ..\Release

:NOWINRELEASE
IF EXIST ..\Release\Latest GOTO NOWINLATEST
MD ..\Release\Latest

:NOWINLATEST
REM Remove previous release
CD ..\Release\Latest
DEL /F /S /Q *

REM Copy release files
MD PTMagic
XCOPY /Y /S ..\..\PTMagic\_defaults\* .\
XCOPY /Y /S ..\..\PTMagic\bin\Release\PublishOutput .\PTMagic
CD .\PTMagic
DEL /F /S /Q _presets
DEL /F /S /Q settings.*
DEL /F /S /Q Monitor\appsettings.json
