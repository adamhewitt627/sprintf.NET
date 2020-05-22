#include <cstddef>
#include <cstdint>
#include <cwchar>

#if defined(_MSC_VER) // Microsoft 
    #define EXPORT __declspec(dllexport)
#elif defined(__GNUC__) // GCC
    #define EXPORT __attribute__((visibility("default")))
#endif

extern "C" EXPORT size_t FormatInt32(wchar_t* buffer, size_t length, wchar_t* format, int32_t value) {
    return swprintf(buffer, length, format, value);
}
extern "C" EXPORT size_t FormatUInt32(wchar_t* buffer, size_t length, wchar_t* format, uint32_t value) {
    return swprintf(buffer, length, format, value);
}
extern "C" EXPORT size_t FormatInt64(wchar_t* buffer, size_t length, wchar_t* format, int64_t value) {
    return swprintf(buffer, length, format, value);
}
extern "C" EXPORT size_t FormatUInt64(wchar_t* buffer, size_t length, wchar_t* format, uint64_t value) {
    return swprintf(buffer, length, format, value);
}
extern "C" EXPORT size_t FormatDouble(wchar_t* buffer, size_t length, wchar_t* format, double value) {
    return swprintf(buffer, length, format, value);
}
extern "C" EXPORT size_t FormatChar(wchar_t* buffer, size_t length, wchar_t* format, wchar_t value) {
    return swprintf(buffer, length, format, value);
}
