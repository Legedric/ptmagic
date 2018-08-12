REM Create a PTM release

IF EXIST ..\Release GOTO NOWINRELEASE
MD ..\Release

:NOWINRELEASE
IF EXIST ..\Release\Latest GOTO NOWINLATEST
MD ..\Release\Latest

:NOWINLATEST
REM Remove previous release
CD ..\Release\Latest
DEL /F /S /Y *

REM Copy release files
MKDIR PTMagic
XCOPY /Y /S ..\..\PTMagic\_defaults\_default* .\
XCOPY /Y /S ..\..\PTMagic\bin\Release\PublishOutput .\PTMagic
