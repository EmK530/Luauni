using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class String
{
    public static IEnumerator gmatch(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        if (inp.Length != 2)
        {
            Logging.Error("Invalid argument count: "+inp.Length, "String:gmatch"); dat.initiator.globalErrored = true; yield break;
        }

        string s = (string)inp[0];
        string pattern = (string)inp[1];

        List<string> matches = new List<string>();
        Regex regex = new Regex(pattern);
        Match match = regex.Match(s);
        while (match.Success)
        {
            matches.Add(match.Value);
            match = match.NextMatch();
        }
        TableIterator it = new TableIterator(matches.ToArray());
        matches.Clear();

        Luau.returnToProto(ref dat, new object[] { it });
        yield break;
    }

    public static IEnumerator sub(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        switch(inp.Length)
        {
            case 1:
                Luau.returnToProto(ref dat, new object[1] { ((string)inp[0]).Substring(1) });
                break;
            case 2:
                Luau.returnToProto(ref dat, new object[1] { ((string)inp[0]).Substring(Convert.ToInt32(inp[1])) });
                break;
            default:
                int v1 = Convert.ToInt32(inp[1]);
                Luau.returnToProto(ref dat, new object[1] { ((string)inp[0]).Substring(v1, Convert.ToInt32(inp[2])-v1) });
                break;
        }
        yield break;
    }

    public static bool isObject = false;
}