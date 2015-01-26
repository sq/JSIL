@del /Q *.emjs
@call emcc -Os -O2 --closure 1 -o common.js common/common.cpp
@rename common.js common.emjs
@del /Q *.mem