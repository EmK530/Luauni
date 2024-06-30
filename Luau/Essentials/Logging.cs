using System;
using UnityEngine;

public static class Logging
{
    public static bool ShowDebug = false;

    public static string LastError = "N/A";

    public static void Debug(object input, string src = "Main") { if (ShowDebug) { UnityEngine.Debug.Log($"[{src}:DEBUG] {input}"); } }
    public static void Print(object input, string src = "Main") {
        UnityEngine.Debug.Log($"[{src}:PRINT] {input}");
        DebugConsole.console.Add(DateTime.Now.ToString("HH:mm:ss") + " -- " + input + " -- " + src);
    }
    public static void Warn(object input, string src = "Main") {
        UnityEngine.Debug.LogWarning($"[{src}:WARN] {input}");
        DebugConsole.console.Add("<color=#ffff00>" + DateTime.Now.ToString("HH:mm:ss") + " -- " + input + " -- " + src + "</color>");
    }
    public static void Error(object input, string src = "Main") {
        LastError = input.ToString();
        UnityEngine.Debug.LogError($"[{src}:ERROR] {input}");
        DebugConsole.console.Add("<color=#ff0000>" + DateTime.Now.ToString("HH:mm:ss") + " -- " + input + " -- " + src + "</color>");
    }
}