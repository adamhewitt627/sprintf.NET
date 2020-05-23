# sprintf.NET   ![Continuous](https://github.com/adamhewitt627/sprintf.NET/workflows/Continuous/badge.svg) [![NuGet Status](http://img.shields.io/nuget/v/sprintf.NET.svg?style=flat)](https://www.nuget.org/packages/sprintf.NET/)
For many reasons, a .NET developer *should* use [composite formatting](https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting). (Culture-specific, no P/Invoke cost, string interpolation, etc) However, sometimes it is desirable to have a C-style format string, as when sharing localization strings across platforms.

P/Invoke to `swprintf` is generally sufficient, but:
1. It does not handle `params` correctly. (Values remain boxed as `object` and the format becomes the pointer value)
2. Because `swprintf` is disallowed for Windows Store apps, we must call the `strsafe` methods.

Thus, we use a regular expression to parse the format string, and then call the approriate native API according to the parameter type.



### Extension to `printf`
To improve sharing with iOS code, this includes `%@` as an acceptable format specificer. Its behavior will match the culture-invariant `ToString` of the parameter.
