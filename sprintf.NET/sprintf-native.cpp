#pragma once
#include "StrSafe.h"
#include "stdint.h"

// StringCchPrintf(buffer, length, format, value);
#define FORMAT(type, suffix) extern "C" __declspec(dllexport) size_t Format##suffix(wchar_t* buffer, size_t length, wchar_t* format, type value) { \
    return 0;\
}

FORMAT(int32_t, Int32);
FORMAT(uint32_t, UInt32);
FORMAT(int64_t, Int64);
FORMAT(uint64_t, UInt64);
FORMAT(double, Double);
FORMAT(wchar_t, Char);
