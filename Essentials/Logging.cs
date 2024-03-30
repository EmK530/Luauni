using System;
using UnityEngine;

public static class Logging
{
    public static bool ShowDebug = true;

    public static string LastError = "N/A";

    public static void Debug(object input, string src = "Main") { if (ShowDebug) { UnityEngine.Debug.Log($"[{src}:DEBUG] {input}"); } }
    public static void Print(object input, string src = "Main") { UnityEngine.Debug.Log($"[{src}:PRINT] {input}"); }
    public static void Warn(object input, string src = "Main") { UnityEngine.Debug.LogWarning($"[{src}:WARN] {input}"); }
    public static void Error(object input, string src = "Main") { LastError = input.ToString(); UnityEngine.Debug.LogError($"[{src}:ERROR] {input}"); }
}