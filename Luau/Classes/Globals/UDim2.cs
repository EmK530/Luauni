using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UICoords
{
    public double x;
    public double y;
}

public class UDim2
{
    public readonly string ClassName = "UDim2";

    public UICoords Scale;
    public UICoords Offset;

    public UDim2(double xScale = 0, double xOffset = 0, double yScale = 0, double yOffset = 0)
    {
        Scale = new UICoords()
        {
            x = xScale,
            y = yScale
        };
        Offset = new UICoords()
        {
            x = xOffset,
            y = yOffset
        };
    }

    public static IEnumerator @new(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        switch(inp.Length)
        {
            case 0:
                Luau.returnToProto(ref dat, new object[1] { new UDim2() });
                break;
            case 1:
                Luau.returnToProto(ref dat, new object[1] { new UDim2((double)inp[0]) });
                break;
            case 2:
                Luau.returnToProto(ref dat, new object[1] { new UDim2((double)inp[0], (double)inp[1]) });
                break;
            case 3:
                Luau.returnToProto(ref dat, new object[1] { new UDim2((double)inp[0], (double)inp[1], (double)inp[2]) });
                break;
            default:
                Luau.returnToProto(ref dat, new object[1] { new UDim2((double)inp[0], (double)inp[1], (double)inp[2], (double)inp[3]) });
                break;
        }
        yield break;
    }

    public static IEnumerator fromScale(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        switch (inp.Length)
        {
            case 0:
                Luau.returnToProto(ref dat, new object[1] { new UDim2() });
                break;
            case 1:
                Luau.returnToProto(ref dat, new object[1] { new UDim2((double)inp[0]) });
                break;
            default:
                Luau.returnToProto(ref dat, new object[1] { new UDim2((double)inp[0], 0d, (double)inp[1]) });
                break;
        }
        yield break;
    }

    public static bool isObject = false;
}
