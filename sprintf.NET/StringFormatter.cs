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
        private const string STRING_REPLACE_PATTERN = @"\%"
                                              + @"((?<" + PARAMETER + @">\d*)\$)?"
                                              + @"(?<" + FLAGS + @">[\'\#\-\+ 0]+)?"
                                              + @"(?<" + WIDTH + @">\d+)?"
                                              + @"(\.(?<" + PRECISION + @">\d+))?"
                                              + @"(?<" + LENGTH + @">[hl]l?)?"
                                              + @"(?<" + TYPE + @">[dioxXucCsfeEgGpn%@])";

        private static readonly Regex STRING_FORMAT_REGEX = new Regex(STRING_REPLACE_PATTERN, RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        public static string PrintF(string format, params object[] args)
        {
            if (format == null) throw new ArgumentNullException(nameof(format));

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

        private static string swprintf(string format, object arg)
        {
            if (arg is string s) return s;
            if (format == "%@") return string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0}", arg);

            // Console.WriteLine($"IntPtr.Size: {IntPtr.Size}");
            int res = 0;
            int size = 8;
            string buffer = null;
            const int maxSize = 256;
            var formatter = buildFunc();

            do
            {
                buffer = new string('\0', size);
                res = formatter();
            } while (res < 0 && (size *= 2) <= maxSize);

            var result = buffer.Substring(0, res);
            return result;

            Func<int> buildFunc()
            {
                switch (arg)
                {
                    case int i: return () => FormatInt32(buffer, size, format, i);
                    case uint i: return () => FormatUInt32(buffer, size, format, i);
                    case long i: return () => FormatInt64(buffer, size, format, i);
                    case ulong i: return () => FormatUInt64(buffer, size, format, i);
                    case double i: return () => FormatDouble(buffer, size, format, i);
                    case float i: return () => FormatDouble(buffer, size, format, i);
                    case char i: return () => FormatChar(buffer, size, format, i);
                    default: throw new ArgumentException($"Unsupported format argument: {arg} - Type: {arg?.GetType()}");
                }
            }
        }


        private const string NativeLib = "sprintf-native";
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FormatInt32(string result, int maxLength, string format, int value);
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FormatUInt32(string result, int maxLength, string format, uint value);
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FormatInt64(string result, int maxLength, string format, long value);
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FormatUInt64(string result, int maxLength, string format, ulong value);
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FormatDouble(string result, int maxLength, string format, double value);
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FormatChar(string result, int maxLength, string format, char value);
    }
}
