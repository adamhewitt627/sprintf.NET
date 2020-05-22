using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
#if UAP
using static sprintfUWP.Formatter;
#endif

namespace SprintfNET
{
    public static class StringFormatter
    {
        private const string PARAMETER = "parameter";
        private const string FLAGS = "flags";
        private const string WIDTH = "width";
        private const string PRECISION = "precision";
        private const string LENGTH = "length";
        private const string TYPE = "type";

        private static readonly Regex STRING_FORMAT_REGEX = new Regex($@"\%
            ((?<{PARAMETER}>\d*)\$)?
            (?<{FLAGS}>[\'\#\-\+ 0]+)?
            (?<{WIDTH}>\d+)?
            (\.(?<{PRECISION}>\d+))?
            (?<{LENGTH}>[hl]l?)?
            (?<{TYPE}>[dioxXucCsfeEgGpn%@])
        ", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        public static string PrintF(string format, params object[] args)
        {
            _ = format ?? throw new ArgumentNullException(nameof(format));

            var argIndex = 0;
            return STRING_FORMAT_REGEX.Replace(format, match =>
            {
                switch (match.Value)
                {
                    case "%%":
                        return "%";

                    default:
                        var @param = match.Groups[PARAMETER];
                        var index = @param.Success
                            ? int.Parse(@param.Value) - 1
                            : argIndex++;

                        //Format string with the parameter stripped
                        var fmt = string.Join(string.Empty, "%",
                            match.Groups[FLAGS],
                            match.Groups[WIDTH],
                            match.Groups[PRECISION].Success ? "." + match.Groups[PRECISION] : string.Empty,
                            match.Groups[LENGTH],
                            match.Groups[TYPE]);

                        return swprintf(fmt, args[index]); //?? "";
                }
            });
        }

        private static unsafe string swprintf(string format, object arg)
        {
            if (arg is string s) return s;
            if (string.Equals(format, "%@")) return string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0}", arg);

            int res = 0;
            int size = 8;
            const int maxSize = 256;

            fixed (char* pointer = format)
            {
                do
                {
                    char* buffer = stackalloc char[size];
                    res = arg switch
                    {
                        int i => FormatInt32(buffer, size, pointer, i),
                        uint i => FormatUInt32(buffer, size, pointer, i),
                        long i => FormatInt64(buffer, size, pointer, i),
                        ulong i => FormatUInt64(buffer, size, pointer, i),
                        double i => FormatDouble(buffer, size, pointer, i),
                        float i => FormatDouble(buffer, size, pointer, i),
                        char i => FormatChar(buffer, size, pointer, i),
                        _ => throw new ArgumentException($"Unsupported format argument: {arg} - Type: {arg?.GetType()}"),
                    };

                    if (res >= 0)
                    {
                        var result = new string(buffer, 0, res);
                        return result;
                    }

                } while (res < 0 && (size *= 2) <= maxSize);
            }

            return string.Empty;
        }


        private const string NativeLib = "swprintf";
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int FormatInt32(char* result, int maxLength, char* format, int value);
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int FormatUInt32(char* result, int maxLength, char* format, uint value);
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int FormatInt64(char* result, int maxLength, char* format, long value);
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int FormatUInt64(char* result, int maxLength, char* format, ulong value);
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int FormatDouble(char* result, int maxLength, char* format, double value);
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int FormatChar(char* result, int maxLength, char* format, char value);
    }
}
