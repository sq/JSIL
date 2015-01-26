@del /Q *.dll
@call cl /D_USRDLL /D_WINDLL common/common.cpp /link /DLL /OUT:common.dll
@del /Q *.exp
@del /Q *.lib
@del /Q *.obj