@del /Q *.emjs
@call emcc -Os -O2 --closure 1 --memory-init-file 0 -o common.js common/common.cpp
@rename common.js common.emjs
@del /Q *.mem