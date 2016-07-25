::Installer for mIRC Auto-Downloader

@ECHO off
SETLOCAL ENABLEEXTENSIONS ENABLEDELAYEDEXPANSION
SET me=%~n0
SET parent=%~dp0
SET files=%parent%\Files
SET images=%parent%\Images

ECHO ###########################################
ECHO ##### Welcome to the AutoDL Installer #####
ECHO ###########################################
ECHO ###########################################
ECHO Please take a moment to read this to ensure a successful install.
ECHO.
ECHO You will be asked a couple install questions during this installation.
ECHO If this is your first time using DOS this is a quick description of what to do.
ECHO When asked to enter something, you simply type your answer and press ^<ENTER^>.
ECHO If you wish to leave it blank (for answers that accept blank), simply press 
ECHO ^<ENTER^>.
ECHO.
ECHO Questions that will be asked and their descriptions:
ECHO.
ECHO Script Folder Location: The location your mIRC script folder is located.
ECHO If you used default mIRC installation, this will be located in %%APPDATA%%\mIRC 
ECHO and you may simply press ^<ENTER^> when prompted to use the default.
ECHO.
ECHO mIRC Folder Location:  The location your mIRC program folder is located.
ECHO If you used the default mIRC installation, this will be located in 
ECHO %%ProgramFiles(x86)%%\mIRC and you may simply press ^<ENTER^> when prompted to 
ECHO use the default.
ECHO.
ECHO *Optional* mIRC Options Setup:  This will edit your mirc.ini file for settings
ECHO required for AutoDL to function properly.  If you would like to complete this 
ECHO step manually follow the instructions at github.com/Rinsho/mIRCAutoDL/wiki.
ECHO.
ECHO - **IMPORTANT** This has only been tested with mIRC 7.32 though it appears new 
ECHO - settings are simply added to the ini, thus maintaining the order of previous 
ECHO - settings. This may change in a future mIRC version. Your old mirc.ini will be
ECHO - renamed to mirc.ini.BAK so you can restore previous settings if this occurs.
ECHO.
ECHO ***** Scroll Up or expand DOS window if beginning is cut off.  Thanks. ******
ECHO Press ^<ENTER^> to continue...
PAUSE >NUL
CLS

ECHO Setup:
ECHO.
SET /P scriptfolder=Script folder location (leave blank to use default): 
SET /P mircfolder=mIRC folder location (leave blank to use default): 
SET /P optsetup=Setup mirc.ini settings (Y/N)? 

IF /I "%scriptfolder%"=="" (SET scriptfolder="%APPDATA%\mIRC")
IF /I "%mircfolder%"=="" (SET mircfolder="%ProgramFiles(x86)%\mIRC")

ECHO.
ECHO ###########################################
ECHO.
ECHO Setting Up mIRC Remote Script File...

FOR /F "usebackq tokens=3 delims=:" %%G IN (`FIND /N /V /C "" "%scriptfolder:"=%\scripts\remote.ini"`) DO (
  SET lines=%%G
  SET /A lines=!lines:~1!-1
)

FOR /F "usebackq delims=" %%G IN (`MORE +!lines! ^< "%scriptfolder:"=%\scripts\remote.ini"`) DO (
  FOR /F "tokens=1* delims==" %%A IN ("%%G") DO (
    SET lastline="%%B"
    IF /I "!lastline:~6,5!" NEQ "START" (
      (
	IF /I "!lastline!" NEQ "" (
	  @ECHO.
	)
        @ECHO n!lines!=
        SET /A lines=!lines!+1
        @ECHO n!lines!=on *:START:/load -rs scripts\AutoDL.mrc
      ) >> "%scriptfolder:"=%\scripts\remote.ini"
    )
  )
)

ECHO Remote Script File Updated.
ECHO.
ECHO ###########################################
ECHO.
IF /I "%optsetup%"=="Y" (
  ECHO MIRC.INI Setup Started.
  SET /P "autoget=Choose Auto-Get type (1 - Resume, 2 - Overwrite): "
  mIRCOptionsEditor.exe "%scriptfolder:"=%\mirc.ini" "!autoget!"
  ECHO MIRC.INI Setup Completed.
) ELSE (
  ECHO MIRC.INI Setup Skipped.
)
ECHO.
ECHO ###########################################
ECHO.
ECHO Copying Files...

ROBOCOPY "%images%" "%mircfolder:"=%\AutoDL\Images" Delete.png /xo /r:5 /w:1 /ns /nc /ndl /njh /njs
IF %ERRORLEVEL% EQU 0 (ECHO File Copy: Newer or current version of Delete.png already exists)
IF %ERRORLEVEL% GTR 1 (ECHO File Copy: Error copying file Delete.png)

ROBOCOPY "%files%" "%scriptfolder:"=%\scripts" AutoDL.mrc /xo /r:5 /w:1 /ns /nc /ndl /njh /njs
IF %ERRORLEVEL% EQU 0 (ECHO File Copy: Newer or current version of AutoDL.mrc already exists)
IF %ERRORLEVEL% GTR 1 (ECHO File Copy: Error copying file AutoDL.mrc)

ROBOCOPY "%files%" "%mircfolder:"=%\AutoDL\Service" Core.dll /xo /r:5 /w:1 /ns /nc /ndl /njh /njs
IF %ERRORLEVEL% EQU 0 (ECHO File Copy: Newer or current version of Core.dll already exists)
IF %ERRORLEVEL% GTR 1 (ECHO File Copy: Error copying file Core.dll)

ROBOCOPY "%files%" "%mircfolder:"=%\AutoDL\Service" ServiceDataContracts.dll /xo /r:5 /w:1 /ns /nc /ndl /njh /njs
IF %ERRORLEVEL% EQU 0 (ECHO File Copy: Newer or current version of ServiceDataContracts.dll already exists)
IF %ERRORLEVEL% GTR 1 (ECHO File Copy: Error copying file ServiceDataContracts.dll)

ROBOCOPY "%files%" "%mircfolder:"=%\AutoDL\Service" ServiceContracts.dll /xo /r:5 /w:1 /ns /nc /ndl /njh /njs
IF %ERRORLEVEL% EQU 0 (ECHO File Copy: Newer or current version of ServiceContracts.dll already exists)
IF %ERRORLEVEL% GTR 1 (ECHO File Copy: Error copying file ServiceContracts.dll)

ROBOCOPY "%files%" "%mircfolder:"=%\AutoDL\Client" ServiceClient.dll /xo /r:5 /w:1 /ns /nc /ndl /njh /njs
IF %ERRORLEVEL% EQU 0 (ECHO File Copy: Newer or current version of ServiceClient.dll already exists)
IF %ERRORLEVEL% GTR 1 (ECHO File Copy: Error copying file ServiceClient.dll)

ROBOCOPY "%files%" "%mircfolder:"=%\AutoDL\Service" UpdateServiceProvider.dll /xo /r:5 /w:1 /ns /nc /ndl /njh /njs
IF %ERRORLEVEL% EQU 0 (ECHO File Copy: Newer or current version of UpdateServiceProvider.dll already exists)
IF %ERRORLEVEL% GTR 1 (ECHO File Copy: Error copying file UpdateServiceProvider.dll)

ROBOCOPY "%files%" "%mircfolder:"=%\AutoDL\Client" UpdateServiceSubscriberClient.dll /xo /r:5 /w:1 /ns /nc /ndl /njh /njs
IF %ERRORLEVEL% EQU 0 (ECHO File Copy: Newer or current version of UpdateServiceSubscriberClient.dll already exists)
IF %ERRORLEVEL% GTR 1 (ECHO File Copy: Error copying file UpdateServiceSubscriberClient.dll)

ROBOCOPY "%files%" "%mircfolder:"=%\AutoDL\Service" UpdateServiceSubscriberContract.dll /xo /r:5 /w:1 /ns /nc /ndl /njh /njs
IF %ERRORLEVEL% EQU 0 (ECHO File Copy: Newer or current version of UpdateServiceSubscriberContract.dll already exists)
IF %ERRORLEVEL% GTR 1 (ECHO File Copy: Error copying file UpdateServiceSubscriberContract.dll)

ROBOCOPY "%files%" "%mircfolder:"=%\AutoDL\Client" mIRCClient.dll /xo /r:5 /w:1 /ns /nc /ndl /njh /njs
IF %ERRORLEVEL% EQU 0 (ECHO File Copy: Newer or current version of mIRCClient.dll already exists)
IF %ERRORLEVEL% GTR 1 (ECHO File Copy: Error copying file mIRCClient.dll)

ROBOCOPY "%files%" "%mircfolder:"=%\AutoDL\Service" mIRCWrapper.dll /xo /r:5 /w:1 /ns /nc /ndl /njh /njs
IF %ERRORLEVEL% EQU 0 (ECHO File Copy: Newer or current version of mIRCWrapper.dll already exists)
IF %ERRORLEVEL% GTR 1 (ECHO File Copy: Error copying file mIRCWrapper.dll)

ECHO File Copy Completed.  Press Enter to exit.
PAUSE >NUL
ENDLOCAL
EXIT