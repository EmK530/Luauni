using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBXScriptConnection
{
    public bool Connected = true;

    public bool Once = false;
    public Closure Connection = null;
    public CallData Data;

    public IEnumerator Disconnect(CallData dat)
    {
        Connected = false;
        Connection = null;
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }

    public RBXScriptConnection(Closure connection, bool once, ref CallData dat)
    {
        Connection = connection;
        Once = once;
        Data = dat;
    }

    public static bool isObject = false;
}
