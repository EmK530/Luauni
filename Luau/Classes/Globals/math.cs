using System;
using UnityEngine;

public static class math
{
    public static System.Collections.IEnumerator abs(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Luau.returnToProto(ref dat, new object[1] { (double)Mathf.Abs(Convert.ToSingle(inp[0])) });
        yield break;
    }
    //missing acos
    //missing asin
    //missing atan
    //missing atan2
    //missing ceil
    //missing clamp
    public static System.Collections.IEnumerator cos(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Luau.returnToProto(ref dat, new object[1] { (double)Mathf.Cos(Convert.ToSingle(inp[0])) });
        yield break;
    }
    //missing deg
    //missing exp
    public static System.Collections.IEnumerator floor(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Luau.returnToProto(ref dat, new object[1] { (double)Mathf.Floor(Convert.ToSingle(inp[0])) });
        yield break;
    }
    //missing fmod
    //missing frexp
    public static double huge = double.PositiveInfinity;
    //missing ldexp
    //missing log
    //missing log10
    public static System.Collections.IEnumerator max(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Luau.returnToProto(ref dat, new object[1] { (double)Mathf.Max(Convert.ToSingle(inp[0]), Convert.ToSingle(inp[1])) });
        yield break;
    }
    public static System.Collections.IEnumerator min(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Luau.returnToProto(ref dat, new object[1] { (double)Mathf.Min(Convert.ToSingle(inp[0]), Convert.ToSingle(inp[1])) });
        yield break;
    }
    //missing modf
    //missing noise
    public static double pi = 3.1415926535897931;
    //missing pow
    public static System.Collections.IEnumerator rad(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Luau.returnToProto(ref dat, new object[1] { (double)inp[0] * (double)Mathf.Deg2Rad });
        yield break;
    }
    public static System.Collections.IEnumerator random(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        switch (inp.Length)
        {
            case 0:
                {
                    Luau.returnToProto(ref dat, new object[1] { NMath.ldexp(Luau.pcg32_random() | ((ulong)Luau.pcg32_random() << 32), -64) });
                    yield break;
                }
            case 1:
                {
                    Luau.returnToProto(ref dat, new object[1] { (double)(1 + ((Convert.ToUInt64(inp[0]) * Luau.pcg32_random()) >> 32)) });
                    yield break;
                }
            case 2:
                {
                    int l = Convert.ToInt32((double)inp[0]);
                    int u = Convert.ToInt32((double)inp[1]);
                    Luau.returnToProto(ref dat, new object[1] { (double)(l + (int)((((uint)u - (uint)l + 1UL) * Luau.pcg32_random()) >> 32)) });
                    yield break;
                }
            default:
                Logging.Error("Unsupported number of parameters! Returning 0.", "Globals:math.random");
                Luau.returnToProto(ref dat, new object[1] { 0d });
                yield break;
        }
    }
    public static System.Collections.IEnumerator randomseed(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Luau.pcg32_seed(Convert.ToUInt64((double)inp[0]));
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }
    public static System.Collections.IEnumerator round(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Luau.returnToProto(ref dat, new object[1] { Math.Round(Luau.safeNum(inp[0])) });
        yield break;
    }
    //missing sign
    public static System.Collections.IEnumerator sin(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Luau.returnToProto(ref dat, new object[1] { (double)Mathf.Sin(Convert.ToSingle(inp[0])) });
        yield break;
    }
    //missing sinh
    public static System.Collections.IEnumerator sqrt(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Luau.returnToProto(ref dat, new object[1] { Math.Sqrt(Luau.safeNum(inp[0])) });
        yield break;
    }
    public static System.Collections.IEnumerator tan(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Luau.returnToProto(ref dat, new object[1] { Math.Tan(Luau.safeNum(inp[0])) });
        yield break;
    }
    //missing tanh

    public static bool isObject = false;
}