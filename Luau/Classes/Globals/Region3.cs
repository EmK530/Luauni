using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region3
{
    public readonly string ClassName = "Region3";

    public Vector3 min;
    public Vector3 max;

    public Region3(Vector3 min, Vector3 max)
    {
        this.min = min;
        this.max = max;
    }

    public static IEnumerator @new(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        switch(inp.Length)
        {
            case 2:
                Luau.returnToProto(ref dat, new object[1] { new Region3((Vector3)inp[0], (Vector3)inp[1]) });
                break;
            default:
                Logging.Error($"No constructor found for CFrame with argument count {inp.Length}", "Luauni:CFrame");
                dat.initiator.globalErrored = true;
                yield break;
        }
        yield break;
    }

    public static bool isObject = false;
}
