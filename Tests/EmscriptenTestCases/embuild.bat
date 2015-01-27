@del /Q *.emjs
@del /Q *.js.map

@if exist "%ProgramW6432%\Emscripten\emsdk_env.bat" (
    @call "%ProgramW6432%\Emscripten\emsdk_env.bat" 
) else (
    @call "%PROGRAMFILES%\Emscripten\emsdk_env.bat"
)
@if errorlevel 1 goto fail

@call emcc -O0 -g4 --memory-init-file 0 -o common.js common/common.cpp
@if errorlevel 1 goto fail

@rem @call emcc -Os -O2 --memory-init-file 0 -o common.js common/common.cpp
@if errorlevel 1 goto fail

@echo Compiled common.emjs

@rename common.js common.emjs
@del /Q *.mem

@goto batch_end

:fail
@echo FAILED.
@exit /b 1234

:batch_end