using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberSequenceKeypoint
{
    public readonly string ClassName = "NumberSequenceKeypoint";

    public double Time;
    public double Value;

    public NumberSequenceKeypoint(double time, double value)
    {
        this.Time = time;
        this.Value = value;
    }

    public static IEnumerator @new(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        switch(inp.Length)
        {
            case 2:
                double location = (double)inp[0];
                if(location < 0 || location > 1)
                {
                    Logging.Error($"NumberSequenceKeypoint time must be between 0 and 1", "Luauni:NumberSequenceKeypoint");
                    dat.initiator.globalErrored = true;
                    yield break;
                }
                Luau.returnToProto(ref dat, new object[1] { new NumberSequenceKeypoint(location, (double)inp[1]) });
                break;
            default:
                Logging.Error($"No constructor found for NumberSequenceKeypoint with argument count {inp.Length}", "Luauni:NumberSequenceKeypoint");
                dat.initiator.globalErrored = true;
                yield break;
        }
        yield break;
    }

    public static bool isObject = false;
}
