@call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\Common7\Tools\VsDevCmd.bat"
@if errorlevel 0 goto ok
@call "%ProgramFiles(x86)%\Microsoft Visual Studio 13.0\Common7\Tools\VsDevCmd.bat"
@if errorlevel 0 goto ok
@call "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\Common7\Tools\VsDevCmd.bat"
@if errorlevel 0 goto ok
@call "%ProgramFiles(x86)%\Microsoft Visual Studio 11.0\Common7\Tools\VsDevCmd.bat"
@if errorlevel 0 goto ok
@goto fail

:ok
@del /Q *.dll
@call cl /D_USRDLL /D_WINDLL exclude_common/common.cpp /link /DLL /OUT:common.dll
@if errorlevel 1 goto fail

@del /Q *.exp
@del /Q *.lib
@del /Q *.obj

@goto batch_end

:fail
@echo FAILED. Could not find visual studio environment.
@exit /b 1234

:batch_end