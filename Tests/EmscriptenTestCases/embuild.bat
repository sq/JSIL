@del /Q *.emjs
@del /Q *.js.map

@if exist "%ProgramW6432%\Emscripten\emsdk_env.bat" (
    @pushd "%ProgramW6432%\Emscripten" 
    @call "emsdk_env.bat" 
    @popd
) else (
    @pushd "%ProgramFiles%\Emscripten" 
    @call "emsdk_env.bat" 
    @popd
)
@if errorlevel 1 goto fail

@set EMARGS=--memory-init-file 0 -s MODULARIZE=1 -s EXPORT_FUNCTION_TABLES=1 -s RESERVED_FUNCTION_POINTERS=8 -o common.js common/common.cpp

@call emcc -O0 -g4 %EMARGS%
@if errorlevel 1 goto fail

@rem @call emcc -Os -O2 %EMARGS%
@if errorlevel 1 goto fail

@echo Compiled common.emjs

@rename common.js common.emjs
@del /Q *.mem

@goto batch_end

:fail
@echo FAILED.
@exit /b 1234

:batch_end