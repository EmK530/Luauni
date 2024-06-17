using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Color3
{
    public readonly string ClassName = "Color3";

    public readonly float r, g, b;

    public Color3(float r = 0, float g = 0, float b = 0)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }

    public static IEnumerator @new(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        switch(inp.Length)
        {
            case 0:
                Luau.returnToProto(ref dat, new object[1] { new Color3() });
                break;
            case 1:
                Luau.returnToProto(ref dat, new object[1] { new Color3(Convert.ToSingle(inp[0])) });
                break;
            case 2:
                Luau.returnToProto(ref dat, new object[1] { new Color3(Convert.ToSingle(inp[0]), Convert.ToSingle(inp[1])) });
                break;
            default:
                Luau.returnToProto(ref dat, new object[1] { new Color3(Convert.ToSingle(inp[0]), Convert.ToSingle(inp[1]), Convert.ToSingle(inp[2])) });
                break;
        }
        yield break;
    }

    public static IEnumerator fromRGB(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        switch (inp.Length)
        {
            case 0:
                Luau.returnToProto(ref dat, new object[1] { new Color3() });
                break;
            case 1:
                Luau.returnToProto(ref dat, new object[1] { new Color3((float)inp[0] / 255f) });
                break;
            case 2:
                Luau.returnToProto(ref dat, new object[1] { new Color3((float)inp[0] / 255f, (float)inp[1] / 255f) });
                break;
            default:
                Luau.returnToProto(ref dat, new object[1] { new Color3((float)inp[0] / 255f, (float)inp[1] / 255f, (float)inp[2] / 255f) });
                break;
        }
        yield break;
    }

    public static bool isObject = false;
}
