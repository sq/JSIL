@call "%VS120COMNTOOLS%\VsDevCmd.bat"
@if errorlevel 1 goto fail

@del /Q *.dll
@call cl /D_USRDLL /D_WINDLL common/common.cpp /link /DLL /OUT:common.dll
@if errorlevel 1 goto fail

@del /Q *.exp
@del /Q *.lib
@del /Q *.obj

@goto batch_end

:fail
@echo FAILED.
@exit /b 1234

:batch_end