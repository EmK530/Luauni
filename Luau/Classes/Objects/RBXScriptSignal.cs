using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBXScriptSignal
{
    public List<RBXScriptConnection> listeners = new List<RBXScriptConnection>();

    public IEnumerator connect(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        listeners.Add(new RBXScriptConnection((Closure)inp[1], false));
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }

    public IEnumerator wait(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        dat.closure.yielded = true;
        dat.closure.type = YieldType.Any;
        dat.closure.resumeAt = -1d;
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }

    public void _fire(object[] args)
    {
        foreach(RBXScriptConnection con in listeners)
        {
            if(con.Connected)
            {
                Misc.SummonClosure(con.Connection, args);
            }
        }
    }

    public RBXScriptSignal()
    {
        listeners = new List<RBXScriptConnection>();
    }

    public static bool isObject = false;
}