using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ray
{
    public readonly string ClassName = "Ray";

    public Vector3 Origin;
    public Vector3 Direction;

    public Ray(Vector3 Origin, Vector3 Direction)
    {
        this.Origin = Origin;
        this.Direction = Direction;
    }

    public static IEnumerator @new(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        switch(inp.Length)
        {
            case 2:
                Luau.returnToProto(ref dat, new object[1] { new Ray((Vector3)inp[0], (Vector3)inp[1]) });
                break;
            default:
                Logging.Error($"No constructor found for Ray with argument count {inp.Length}", "Luauni:Ray");
                dat.initiator.globalErrored = true;
                yield break;
        }
        yield break;
    }

    public static bool isObject = false;
}
