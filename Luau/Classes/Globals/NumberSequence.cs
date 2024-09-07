using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberSequence
{
    public readonly string ClassName = "NumberSequence";

    public object[] Keypoints = new object[0];

    public NumberSequence(double n)
    {
        Keypoints = new object[2] { new NumberSequenceKeypoint(0d, n), new NumberSequenceKeypoint(1d, n) };
    }
    public NumberSequence(double n0, double n1)
    {
        Keypoints = new object[2] { new NumberSequenceKeypoint(0d, n0), new NumberSequenceKeypoint(1d, n1) };
    }
    public NumberSequence(object[] keypoints)
    {
        double lastPos = -1d;
        foreach(object i in keypoints)
        {
            if(i == null)
            {
                throw new ArgumentException("NumberSequence constructor received table containing a nil value.");
            }
            Type t = i.GetType();
            if(t != typeof(NumberSequenceKeypoint))
            {
                throw new ArgumentException("NumberSequence constructor received table containing a non-NumberSequenceKeypoint value.");
            }
            NumberSequenceKeypoint conv = (NumberSequenceKeypoint)i;
            if(conv.Time < lastPos)
            {
                throw new ArgumentException("table of NumberSequenceKeypoints are not in order.");
            }
            lastPos = conv.Time;
        }
        Keypoints = keypoints;
    }

    public Gradient ToAlphaGradient()
    {
        Gradient gradient = new Gradient();
        List<GradientAlphaKey> colors = new List<GradientAlphaKey>();
        foreach(NumberSequenceKeypoint i in Keypoints)
        {
            colors.Add(new GradientAlphaKey(1f-(float)i.Value, (float)i.Time));
        }
        gradient.alphaKeys = colors.ToArray();
        return gradient;
    }

    public static IEnumerator @new(CallData dat)
    {
        dynamic[] inp = Luau.getAllArgs(ref dat);
        switch(inp.Length)
        {
            case 1:
                Luau.returnToProto(ref dat, new object[1] { new NumberSequence(inp[0]) });
                break;
            case 2:
                Luau.returnToProto(ref dat, new object[1] { new NumberSequence(inp[0], inp[1]) });
                break;
            default:
                Logging.Error($"No constructor found for NumberSequence with argument count {inp.Length}", "Luauni:Ray");
                dat.initiator.globalErrored = true;
                yield break;
        }
        yield break;
    }

    public static bool isObject = false;
}
