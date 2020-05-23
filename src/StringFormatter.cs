using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static System.Text.Encoding;

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
                            ? int.Parse(@param.Value, CultureInfo.InvariantCulture) - 1
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
        
        private static readonly PlatformID platform = Environment.OSVersion.Platform;
        private static readonly int CharSize = platform switch
        {
            PlatformID.Unix => sizeof(char) * 2,
            _ => sizeof(char),
        };

        private static unsafe string swprintf(string format, object arg)
        {
            if (arg is string s) return s;
            if (string.Equals(format, "%@", StringComparison.InvariantCulture)) 
                return string.Format(CultureInfo.InvariantCulture.NumberFormat, "{0}", arg);

            int res = 0;
            int size = 8;
            const int maxSize = 256;

            var fmtSpan = platform switch
            {
                PlatformID.Unix => UTF32.GetBytes(format),
                _ => MemoryMarshal.AsBytes(format.AsSpan()),
            };

            do
            {
                Span<byte> buffer = stackalloc byte[size * CharSize];

                fixed (byte* pbBuffer = &MemoryMarshal.GetReference(buffer))
                fixed (byte* pbFormat = &MemoryMarshal.GetReference(fmtSpan))
                {
                    res = arg switch
                    {
                        int i => FormatInt32(pbBuffer, size, pbFormat, i),
                        uint i => FormatUInt32(pbBuffer, size, pbFormat, i),
                        long i => FormatInt64(pbBuffer, size, pbFormat, i),
                        ulong i => FormatUInt64(pbBuffer, size, pbFormat, i),
                        double i => FormatDouble(pbBuffer, size, pbFormat, i),
                        float i => FormatDouble(pbBuffer, size, pbFormat, i),
                        char i => FormatChar(pbBuffer, size, pbFormat, i),
                        _ => throw new ArgumentException($"Unsupported format argument: {arg} - Type: {arg?.GetType()}"),
                    };
                }


                if (res >= 0)
                {
                    var slice = buffer.Slice(0, res * CharSize);
                    var result = platform switch
                    {
                        PlatformID.Unix => UTF32.GetString(slice),
                        _ => Unicode.GetString(slice),
                    };
                    return result;
                }

            } while (res < 0 && (size *= 2) <= maxSize);

            return string.Empty;
        }


        private const string NativeLib = "swprintf";
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int FormatInt32(byte* result, int maxLength, byte* format, int value);
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int FormatUInt32(byte* result, int maxLength, byte* format, uint value);
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int FormatInt64(byte* result, int maxLength, byte* format, long value);
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int FormatUInt64(byte* result, int maxLength, byte* format, ulong value);
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int FormatDouble(byte* result, int maxLength, byte* format, double value);
        [DllImport(NativeLib, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int FormatChar(byte* result, int maxLength, byte* format, char value);
    }
}
