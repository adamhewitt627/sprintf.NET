#pragma once
#include "StrSafe.h"
using namespace Platform;

#define BUFFER_SIZE 128
#define FORMAT(type) static String^ Format(String^ format, type value) { \
    auto buff = new wchar_t[BUFFER_SIZE]; \
    auto ret = ref new String(buff, swprintf(buff, BUFFER_SIZE, format->Data(), value)); \
    delete[] buff; \
    return ret; \
}

namespace sprintfUWP
{
    public ref class Formatter sealed
    {
    public:
		[Windows::Foundation::Metadata::DefaultOverload]
		FORMAT(int32);
		FORMAT(uint32);
		FORMAT(int64);
		FORMAT(uint64);
		FORMAT(double);
		FORMAT(wchar_t);
    };
}
