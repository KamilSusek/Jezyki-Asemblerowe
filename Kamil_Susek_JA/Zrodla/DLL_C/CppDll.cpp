// CppDll.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "CppDll.h"
#include<iostream>
#include<cmath>
#include "nmmintrin.h" // for SSE4.2
#include"cstdint"

// This is an example of an exported variable
CPPDLL_API int nCppDll = 0;

// This is an example of an exported function.
CPPDLL_API int fnCppDll(void)
{
	return 42;
}

// This is the constructor of a class that has been exported.
// see CppDll.h for the class definition
CCppDll::CCppDll()
{
	return;
}
// CppDll.cpp : Defines the exported functions for the DLL application.
//


extern "C"  void  __declspec(dllexport)  operateOnPixelsCpp(unsigned char *pixels, unsigned char *copy1, unsigned char *copy2)
{
	//vector a and b	
	__m128i a, b;	
	// loading 128 bits of tab copy1 and copy 2
	a = _mm_loadu_si128((__m128i*)copy1);	
	b = _mm_loadu_si128((__m128i*)copy2);
	// subtract b from a and get absolute value, next store result in pixels array
	_mm_storeu_si128((__m128i*)pixels, (_mm_abs_epi8(_mm_sub_epi8(a, b)))); 
}

