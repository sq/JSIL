#ifdef __EMSCRIPTEN__
#include <emscripten.h>
#define export(T) extern "C" T EMSCRIPTEN_KEEPALIVE 
#else
#define export(T) extern "C" __declspec(dllexport) T
#endif

#include <stdlib.h>
#include <string.h>
#include <stdint.h>

struct TestStruct {
    int I;
    float F;
};

struct CallbackStruct {
	unsigned short us;
	void(*callback)(int i);
};

struct FlagStruct {
	uint32_t flags;
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

export(int)Add(int a, int b) {
    return a + b;
}

export(void *)Alloc(int size) {
    return malloc(size);
}

export(void)Free(void * ptr) {
    return free(ptr);
}


typedef int (*TPWriteStringIntoBuffer) (unsigned char *, const int);
typedef TestStruct (*TPReturnStructArgument) (const TestStruct);
typedef int (*TPBinaryOperator) (int, int);

export(TPWriteStringIntoBuffer) ReturnWriteStringIntoBuffer () {
    return WriteStringIntoBuffer;
};

export(TPReturnStructArgument) ReturnReturnStructArgument () {
    return ReturnStructArgument;
};

export(TPBinaryOperator) ReturnAdd () {
    return Add;
};

export(int) CallBinaryOperator (TPBinaryOperator op, int a, int b) {
    return op(a, b);
}

export(TestStruct) CallReturnStructArgument (TPReturnStructArgument rsa, TestStruct arg) {
    return rsa(arg);
}

export(void) CallFunctionInStruct(CallbackStruct s, int i) {
	return s.callback(i);
}

export(FlagStruct) PassFlagInStruct(FlagStruct s) {
	FlagStruct s2;
	s2.flags = s.flags;
	return s2;
}

export(void) FillUshortArray(uint16_t *ramp) {
	ramp[0] = 192;
}