using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Instance
{
    public static IEnumerator Clone(CallData dat)
    {
        dynamic[] inp = Luau.getAllArgs(ref dat);
        GameObject toclone = Misc.SafeGameObjectFromClass(inp[0]);
        if(toclone != null)
        {
            GameObject src = ESS.Clone(toclone);
            Luau.returnToProto(ref dat, new dynamic[1] { Misc.TryGetType(src.transform) });
        } else
        {
            Logging.Error($"Internal error: Cannot perform Clone action on {inp[0]}", "Luauni:Step"); dat.initiator.globalErrored = true; yield break;
        }
        yield break;
    }
    public static IEnumerator Destroy(CallData dat)
    {
        dynamic[] inp = Luau.getAllArgs(ref dat);
        GameObject.DestroyImmediate(Misc.SafeGameObjectFromClass(inp[0]));
        Luau.returnToProto(ref dat, new dynamic[0]);
        yield break;
    }
    public static IEnumerator FindFirstChild(CallData dat)
    {
        dynamic[] inp = Luau.getAllArgs(ref dat);
        GameObject tosearch = Misc.SafeGameObjectFromClass(inp[0]);
        string key = (string)inp[1];
        if (tosearch == null)
        {
            Logging.Error($"Internal error: Cannot perform FindFirstChild action on {inp[0]}", "Luauni:Step"); dat.initiator.globalErrored = true; yield break;
        }
        Transform find = tosearch.transform.Find(key);
        Luau.returnToProto(ref dat, new dynamic[1] { find != null ? Misc.TryGetType(find) : null });
        yield break;
    }
    public static IEnumerator GetChildren(CallData dat)
    {
        dynamic[] inp = Luau.getAllArgs(ref dat);
        GameObject tosearch = Misc.SafeGameObjectFromClass(inp[0]);
        List<dynamic> children = new List<dynamic>();
        foreach (Transform child in tosearch.transform)
        {
            children.Add(Misc.TryGetType(child));
        }
        dynamic[] sendback = children.ToArray();
        children.Clear();
        Luau.returnToProto(ref dat, new dynamic[1] { sendback });
        yield break;
    }
    private static List<dynamic> DescendantsCache = new List<dynamic>();
    private static void IterRecursive(Transform search)
    {
        DescendantsCache.Add(Misc.TryGetType(search));
        foreach (Transform child in search)
        {
            IterRecursive(child);
        }
    }
    public static IEnumerator GetDescendants(CallData dat)
    {
        dynamic[] inp = Luau.getAllArgs(ref dat);
        GameObject tosearch = Misc.SafeGameObjectFromClass(inp[0]);
        foreach(Transform child in tosearch.transform)
        {
            IterRecursive(child);
        }
        dynamic[] sendback = DescendantsCache.ToArray();
        DescendantsCache.Clear();
        Luau.returnToProto(ref dat, new dynamic[1] { sendback });
        yield break;
    }
    public static IEnumerator WaitForChild(CallData dat)
    {
        dynamic[] inp = Luau.getAllArgs(ref dat);
        GameObject tosearch = Misc.SafeGameObjectFromClass(inp[0]);
        string key = (string)inp[1];
        if(tosearch == null)
        {
            Logging.Error($"Internal error: Cannot perform WaitForChild action on {inp[0]}", "Luauni:Step"); dat.initiator.globalErrored = true; yield break;
        }
        Transform find = tosearch.transform.Find(key);
        float st = Time.realtimeSinceStartup;
        bool alerted = false;
        while (find == null)
        {
            yield return null;
            find = tosearch.transform.Find(key);
            if (!alerted && find == null && Time.realtimeSinceStartup - st > 10f)
            {
                Logging.Warn($"Infinite yield possible on '{tosearch.name}:WaitForChild(\"{key}\")'", "Instance:WaitForChild");
                alerted = true;
            }
        }
        Luau.returnToProto(ref dat, new dynamic[1] { Misc.TryGetType(find) });
        yield break;
    }
    public static IEnumerator ClearAllChildren(CallData dat)
    {
        dynamic[] inp = Luau.getAllArgs(ref dat);
        ESS.ClearAllChildren(Misc.SafeGameObjectFromClass(inp[0]));
        Luau.returnToProto(ref dat, new dynamic[0]);
        yield break;
    }
    public static IEnumerator IsA(CallData dat) {
        dynamic[] inp = Luau.getAllArgs(ref dat);
        GameObject obj = Misc.SafeGameObjectFromClass(inp[0]);
        Luau.returnToProto(ref dat, new dynamic[1] { obj.tag == (string)inp[1] });
        yield break;
    }
}
