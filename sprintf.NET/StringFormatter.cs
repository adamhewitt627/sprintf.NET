using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

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

        private static readonly Regex STRING_FORMAT_REGEX = new Regex(STRING_REPLACE_PATTERN, RegexOptions.ExplicitCapture);

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

#if UAP
        private static string swprintf(string format, object arg)
        {
            switch (arg)
            {
                case int value: return Formatter.Format(format, value);
                case uint value: return Formatter.Format(format, value);
                case long value: return Formatter.Format(format, value);
                case ulong value: return Formatter.Format(format, value);
                case float value: return Formatter.Format(format, value);
                case double value: return Formatter.Format(format, value);
                case char value: return Formatter.Format(format, value);
                default: throw new NotImplementedException($"Not implemented: {arg?.GetType()}");
            }
        }

#else

        [DllImport("msvcrt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int swprintf_s(string result, int maxLength, string format, int value);
        [DllImport("msvcrt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int swprintf_s(string result, int maxLength, string format, uint value);
        [DllImport("msvcrt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int swprintf_s(string result, int maxLength, string format, long value);
        [DllImport("msvcrt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int swprintf_s(string result, int maxLength, string format, ulong value);
        [DllImport("msvcrt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int _snwprintf_s(string result, int maxLength, int count, string format, double value);
        [DllImport("msvcrt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int swprintf_s(string result, int maxLength, string format, double value);
        private static readonly Lazy<Func<string, int, string, double, int>> @double = new Lazy<Func<string, int, string, double, int>>(() => {
            try
            {
                var i = _snwprintf_s(new string('\0', 8), 8, 8, "%f", 0);
                return (r, l, f, a) => _snwprintf_s(r, l, l, f, a);
            }
            catch { return swprintf_s; }
        });
        [DllImport("msvcrt", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern int swprintf_s(string result, int maxLength, string format, char value);

        private static string swprintf(string format, object arg)
        {
            if (arg is string s) return s;
            if (format == "%@") return string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0}", arg);

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
                    case int i: return () => swprintf_s(buffer, size, format, i);
                    case uint i: return () => swprintf_s(buffer, size, format, i);
                    case long i: return () => swprintf_s(buffer, size, format, i);
                    case ulong i: return () => swprintf_s(buffer, size, format, i);
                    case double i: return () => swprintf_s(buffer, size, format, i);
                    case float i: return () => @double.Value(buffer, size, format, i);
                    case char i: return () => swprintf_s(buffer, size, format, i);
                    default: throw new ArgumentException($"Unsupported format argument: {arg} - Type: {arg?.GetType()}");
                }
            }
        }
#endif
    }
}
