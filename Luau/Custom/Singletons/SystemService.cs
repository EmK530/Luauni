using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class SystemService : MonoBehaviour
{
    public static IEnumerator Exit(CallData dat)
    {
        Application.Quit();
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }

    public static bool Unlocked
    {
        get { return Windows != null; }
    }

    public static WindowsFunctions Windows = null;

    public static SystemService instance;
    public static bool isObject = false;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning($"Destroying duplicate singleton '{name}'");
            DestroyImmediate(gameObject);
        }
    }
}

public class WindowsFunctions
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowText(IntPtr hWnd, string text);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    private static IntPtr window = IntPtr.Zero;

    private static string GetActiveWindowTitle()
    {
        const int nChars = 256;
        StringBuilder Buff = new StringBuilder(nChars);
        IntPtr handle = GetForegroundWindow();

        if (GetWindowText(window, Buff, nChars) > 0)
        {
            return Buff.ToString();
        }
        return null;
    }

    public WindowsFunctions()
    {
        window = GetForegroundWindow();
    }

    public static string WindowTitle
    {
        get {
            return GetActiveWindowTitle();
        }
        set {
            SetWindowText(window, value);
        }
    }

    public static IEnumerator MessageBox(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        if(inp.Length != 3)
        {
            Logging.Error($"Invalid argument count: {inp.Length}, expected 3.","SystemService:MessageBox"); dat.initiator.globalErrored = true; yield break;
        }
        string txt = (string)inp[0];
        string caption = (string)inp[1];
        uint type = Convert.ToUInt32(inp[2]);
        int result = MessageBox(IntPtr.Zero, txt, caption, type);
        Luau.returnToProto(ref dat, new object[1] { (double)result });
        yield break;
    }
}
