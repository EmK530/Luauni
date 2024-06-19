using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickColor
{
    public readonly string ClassName = "BrickColor";

    public static IEnumerator Gray(CallData dat)
    {
        Luau.returnToProto(ref dat, new object[1] { new Color3(0.640f, 0.636f, 0.648f) });
        yield break;
    }

    public static bool isObject = false;
}
