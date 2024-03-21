using System;

public static class Logging
{
    public static bool ShowDebug = false;

    public static void Debug(object input, string src = "Main") { if (ShowDebug) { Console.WriteLine($"\x1b[1;30;40m[{src}:DEBUG] {input}\x1b[0m "); } }
    public static void Print(object input, string src = "Main") { Console.WriteLine($"\x1b[7;30;47m[{src}:PRINT] {input}\x1b[0m "); }
    public static void Warn(object input, string src = "Main") { Console.WriteLine($"\x1b[7;30;43m[{src}:WARN] {input}\x1b[0m "); }
    public static void Error(object input, string src = "Main") { Console.WriteLine($"\x1b[7;30;41m[{src}:ERROR] {input}\x1b[0m "); }
}