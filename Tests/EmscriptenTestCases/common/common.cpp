#ifdef __EMSCRIPTEN__
#include <emscripten.h>
#define export(T) extern "C" T EMSCRIPTEN_KEEPALIVE 
#else
#define export(T) extern "C" __declspec(dllexport) T
#endif

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

export(void) MutateStringArgument (char * buf, int capacity) {
    // #%(*#@%OJIJ#LW% i hate clang
    // strcat_s(buf, capacity, " world");
    strcat(buf, " world");
}