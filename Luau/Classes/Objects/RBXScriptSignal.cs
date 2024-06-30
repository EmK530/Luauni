using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBXScriptSignal
{
    public List<RBXScriptConnection> listeners = new List<RBXScriptConnection>();
    public List<RBXScriptConnection> waiting = new List<RBXScriptConnection>();

    public IEnumerator connect(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        listeners.Add(new RBXScriptConnection((Closure)inp[1], false, ref dat));
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }

    public IEnumerator wait(CallData dat)
    {
        Misc.YieldClosureForever(ref dat.closure, 0d, ref dat);
        waiting.Add(new RBXScriptConnection(dat.closure.source, false, ref dat));
        yield break;
    }

    public IEnumerator _fire(object[] args)
    {
        foreach(RBXScriptConnection con in listeners)
        {
            if(con.Connected)
            {
                yield return Misc.ExecuteCoroutine(Misc.SummonClosure(con.Connection, args));
            }
        }
        RBXScriptConnection[] list = waiting.ToArray();
        waiting.Clear();
        foreach (RBXScriptConnection con in list)
        {
            Luau.returnToProto(ref con.Data, args);
            con.Data.closure.yielded = false;
            TaskScheduler.instance.lastRanCount++;
            yield return Misc.ExecuteCoroutine(Luauni.Execute(con.Data.closure));
        }
        yield break;
    }

    public RBXScriptSignal()
    {
        listeners = new List<RBXScriptConnection>();
    }

    public static bool isObject = false;
}