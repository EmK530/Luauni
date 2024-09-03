using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InheritedByAll
{
    public static IEnumerator Clone(CallData dat)
    {
        GameObject toclone = Misc.SafeGameObjectFromClass(dat.initiator.recentNameCalledRegister);
        if(toclone != null)
        {
            GameObject src = ESS.Clone(toclone);
            Luau.returnToProto(ref dat, new object[1] { Misc.TryGetType(src.transform) });
        } else
        {
            Logging.Error($"Internal error: Cannot perform Clone action on {dat.initiator.recentNameCalledRegister}", "Luauni:Step"); dat.initiator.globalErrored = true; yield break;
        }
        yield break;
    }
    public static IEnumerator Destroy(CallData dat)
    {
        GameObject.DestroyImmediate(Misc.SafeGameObjectFromClass(dat.initiator.recentNameCalledRegister));
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }
    public static IEnumerator FindFirstChild(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        GameObject tosearch = Misc.SafeGameObjectFromClass(dat.initiator.recentNameCalledRegister);
        string key = (string)inp[1];
        if (tosearch == null)
        {
            Logging.Error($"Internal error: Cannot perform FindFirstChild action on {dat.initiator.recentNameCalledRegister}", "Luauni:Step"); dat.initiator.globalErrored = true; yield break;
        }
        Transform find = tosearch.transform.Find(key);
        Luau.returnToProto(ref dat, new object[1] { find != null ? Misc.TryGetType(find) : null });
        yield break;
    }
    public static IEnumerator GetChildren(CallData dat)
    {
        GameObject tosearch = Misc.SafeGameObjectFromClass(dat.initiator.recentNameCalledRegister);
        List<object> children = new List<object>();
        foreach (Transform child in tosearch.transform)
        {
            children.Add(Misc.TryGetType(child));
        }
        object[] sendback = children.ToArray();
        children.Clear();
        Luau.returnToProto(ref dat, new object[1] { sendback });
        yield break;
    }
    private static List<object> DescendantsCache = new List<object>();
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
        GameObject tosearch = Misc.SafeGameObjectFromClass(dat.initiator.recentNameCalledRegister);
        foreach(Transform child in tosearch.transform)
        {
            IterRecursive(child);
        }
        object[] sendback = DescendantsCache.ToArray();
        DescendantsCache.Clear();
        Luau.returnToProto(ref dat, new object[1] { sendback });
        yield break;
    }
    public static IEnumerator WaitForChild(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        GameObject tosearch = Misc.SafeGameObjectFromClass(dat.initiator.recentNameCalledRegister);
        string key = (string)inp[1];
        if(tosearch == null)
        {
            Logging.Error($"Internal error: Cannot perform WaitForChild action on {dat.initiator.recentNameCalledRegister}", "Luauni:Step"); dat.initiator.globalErrored = true; yield break;
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
        Luau.returnToProto(ref dat, new object[1] { Misc.TryGetType(find) });
        yield break;
    }
}
