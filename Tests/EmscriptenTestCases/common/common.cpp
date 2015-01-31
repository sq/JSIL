#ifdef __EMSCRIPTEN__
#include <emscripten.h>
#define export(T) extern "C" T EMSCRIPTEN_KEEPALIVE 
#else
#define export(T) extern "C" __declspec(dllexport) T
#endif

#include <stdlib.h>
#include <string.h>

struct TestStruct {
    int I;
    float F;
};

export(void) WriteInt (const int value, int * result) {
    *result = value;
}

export(int) ReadInt (const int * source) {
    return *source;
}

export(void) WriteStruct (const int i, const float f, TestStruct * result) {
    result->I = i;
    result->F = f;
}

export(TestStruct) ReturnStruct (const int i, const float f) {
    TestStruct result;
    result.I = i;
    result.F = f;
    return result;
}

export(TestStruct) ReturnStructArgument (const TestStruct arg) {
    return arg;
}

export(void) MutateStringArgument (char * buf, const int capacity) {
    // #%(*#@%OJIJ#LW% i hate clang
    // strcat_s(buf, capacity, " world");
    strcat(buf, " world");
}

export(int) CopyStringArgument (char * dst, const int capacity, const char * src) {
    int length = strlen(src);
    memset(dst, 0, capacity);
    strcpy(dst, src);
    return length;
}

export(int) WriteStringIntoBuffer (unsigned char * dst, const int capacity) {
    const char * str = "hello world";
    memset(dst, 0, capacity);
    strcpy((char *)dst, str);
    return strlen(str);
}

typedef int (*TPWriteStringIntoBuffer) (unsigned char *, const int);

export(TPWriteStringIntoBuffer) ReturnFunctionPointer () {
    return WriteStringIntoBuffer;
};