#ifdef __EMSCRIPTEN__
#include <emscripten.h>
#define export(T) extern "C" T EMSCRIPTEN_KEEPALIVE 
#else
#define export(T) extern "C" T __declspec(dllexport) 
#endif

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