using System;
using UnityEngine;

public static class task
{
    public static System.Collections.IEnumerator wait(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        double delay = inp.Length != 0 ? (double)inp[0] : 0d;
        Misc.YieldClosure(ref dat.closure, YieldType.Task, delay, ref dat);
        yield break;
    }
    public static System.Collections.IEnumerator spawn(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        if (inp[0] is Closure cl)
        {
            object[] misc = new object[inp.Length - 1];
            for (int i = 0; i < misc.Length; i++)
            {
                misc[i] = inp[i + 1];
            }
            Misc.SummonClosureTask(cl, misc);
        }
        else
        {
            Logging.Error($"Internal error: Cannot spawn type {inp[0]}", "Luauni:Globals:task.spawn"); dat.initiator.globalErrored = true; yield break;
        }
        yield break;
    }
    public static System.Collections.IEnumerator delay(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        double duration = (double)inp[0];
        if (inp[1] is Closure cl)
        {
            object[] misc = new object[inp.Length - 2];
            for (int i = 0; i < misc.Length; i++)
            {
                misc[i] = inp[i + 2];
            }
            Misc.DelayClosureTask(cl, misc, duration);
        }
        else
        {
            Logging.Error($"Internal error: Cannot delay type {inp[0]}", "Luauni:Globals:task.delay"); dat.initiator.globalErrored = true; yield break;
        }
        yield break;
    }

    public static bool isObject = false;
}