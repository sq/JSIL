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

struct AlignmentStruct {
	unsigned short us;
	uint32_t choices;
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

export(const char *) ReturnString(const char *s) {
	return s;
}

export(int) ReturnInt(int i) {
	return i;
}

export(const unsigned char *) ReturnStaticString() {
	return (const unsigned char *)"fuzzy pickles";
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


export(int) CopySecondStringFromArray(char * dst, const int capacity, const char ** src) {
	int length = strlen(src[1]);
	memset(dst, 0, capacity);
	strcpy(dst, src[1]);
	return length;
}

export(int) WriteStringIntoBuffer (unsigned char * dst, const int capacity) {
    const char * str = "hello world";
    memset(dst, 0, capacity);
    strcpy((char *)dst, str);
    return strlen(str);
}

export(void) FillStructBuffer(TestStruct *buffer, const int capacity) {
	memset(buffer, 0, capacity*sizeof(TestStruct));

	for (int i = 0; i < capacity; i++) {
		buffer[i].I = 1;
		buffer[i].F = 2.0;
	}
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

export(void *) ReturnNullPtr() {
	return NULL;
}

export(float) AddFloat (float a, float b) {
	return a + b;
}

typedef int (*TPWriteStringIntoBuffer) (unsigned char *, const int);
typedef TestStruct (*TPReturnStructArgument) (const TestStruct);
typedef int (*TPBinaryOperator) (int, int);
typedef const char * (*TPReturnString) (const char *);
typedef float(*TPAddFloat) (float, float);

export(TPWriteStringIntoBuffer) ReturnWriteStringIntoBuffer () {
    return WriteStringIntoBuffer;
};

export(TPReturnStructArgument) ReturnReturnStructArgument () {
    return ReturnStructArgument;
};

export(TPBinaryOperator) ReturnAdd () {
    return Add;
};

export(TPReturnString) ReturnReturnString () {
    return ReturnString;
};

export(TPAddFloat) ReturnAddFloat () {
	return AddFloat;
}

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

export(uint16_t) FirstElementOfUshortArray(uint16_t *arr) {
	return arr[0];
}

export(AlignmentStruct) TestAlignment(const AlignmentStruct arg) {
	return arg;
}

export(int) ReadStringLength(const char **s) {
	return strlen(*s);
}

export(int) ReturnSecondIntFromArray(int *ints) {
	return ints[1];
}