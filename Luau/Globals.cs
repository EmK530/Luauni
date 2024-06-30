#pragma warning disable CS8601
#pragma warning disable CS8603
#pragma warning disable CS8625
#pragma warning disable CS8981

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class Globals
{
    public static Dictionary<string, object> list = new Dictionary<string, object>();
    public delegate System.Collections.IEnumerator Standard(CallData data);

    private static bool initialized = false;
    public static bool IsInitialized()
    {
        Logging.Debug($"IsInitialized = {initialized}", "Globals:IsInitialized");
        return initialized;
    }

    public static void Init()
    {
        Logging.Debug($"Initializing...", "Globals:Init");
        IterateClass(typeof(GC), list, "", true);
        ESS.np.gameObject.SetActive(false);
        list["game"] = DataModel.instance.GetType();
        list["workspace"] = Workspace.instance.GetType();
        list["Enum"] = typeof(Enum);
        list["Vector2"] = typeof(Vector2);
        list["Vector3"] = typeof(Vector3);
        list["Color3"] = typeof(Color3);
        list["BrickColor"] = typeof(BrickColor);
        list["CFrame"] = typeof(CoordinateFrame);
        list["UDim2"] = typeof(UDim2);
        list["string"] = typeof(String);
        list["math"] = typeof(math);
        initialized = true;
    }

    public static void IterateClass(Type i, Dictionary<string, object> contain, string path, bool top)
    {
        foreach (Type t in i.GetNestedTypes())
        {
            Logging.Debug($"Binding global class '{path+(top?"":".")+t.Name}'", "Globals:IterateClass");
            contain[t.Name] = new Dictionary<string, object>();
            string path2 = path;
            if (!top)
            {
                path2 += ".";
            }
            path2 += t.Name;
            IterateClass(t, (Dictionary<string, object>)contain[t.Name], path2, false);
        }
        foreach (FieldInfo t in i.GetFields())
        {
            Logging.Debug($"Binding global field: {path+(top?"":".")+t.Name}", "Globals:IterateClass");
            contain[t.Name] = t.GetValue(i);
        }
        foreach (MethodInfo t in i.GetMethods(BindingFlags.Static | BindingFlags.Public).Cast<MethodInfo>())
        {
            Logging.Debug($"Binding global function '{path+(top?"":".")+t.Name}'", "Globals:IterateClass");
            contain[t.Name] = (Standard)t.CreateDelegate(typeof(Standard));
        }
    }
    public static object Get(string name)
    {
        Logging.Debug($"Get global '{name}'", "Globals:Get");
        if (list.TryGetValue(name, out var value))
        {
            if (value != null && value.GetType() == typeof(Dictionary<string, object>))
            {
                return new NamedDict()
                {
                    name = name,
                    dict = (Dictionary<string, object>)value
                };
            } else
            {
                return value;
            }
        }
        else
        {
            Logging.Debug($"Global name '{name}' is not registered.", "Globals:Get");
            return null;
        }
    }
    public static object NativeGet(string name)
    {
        if (list.TryGetValue(name, out var value))
        {
            return value;
        }
        else
        {
            return "null";
        }
    }
    public static void Set(string name, object value)
    {
        list[name] = value;
    }
}

public static class GC
{
    public static System.Collections.IEnumerator wait(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        double delay = inp.Length != 0 ? (double)inp[0] : 0d;
        /*
        Luau.returnToProto(ref dat, new object[1] { (double)Mathf.Max(1f / 30f, Convert.ToSingle(delay)) });
        dat.closure.yielded = true;
        dat.closure.type = YieldType.Hybrid;
        dat.closure.resumeAt = Time.realtimeSinceStartupAsDouble + delay;
        */
        Misc.YieldClosure(ref dat.closure, YieldType.Hybrid, delay, ref dat);
        yield break;
    }
    public static System.Collections.IEnumerator spawn(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        if (inp[0] is Closure cl)
        {
            object[] misc = new object[inp.Length - 1];
            for(int i = 0; i < misc.Length; i++)
            {
                misc[i] = inp[i + 1];
            }
            Misc.SummonClosureHybrid(cl, misc);
        } else
        {
            Logging.Error($"Internal error: Cannot spawn type {inp[0]}", "Luauni:Globals:spawn"); dat.initiator.globalErrored = true; yield break;
        }
        yield break;
    }
    public static System.Collections.IEnumerator print(CallData dat)
    {
        string output = "";
        bool first = true;
        object[] inp = Luau.getAllArgs(ref dat);
        foreach (object arg in inp)
        {
            if (!first)
            {
                output += " ";
            }
            output += Luau.accurate_tostring(arg);
            first = false;
        }
        UnityEngine.Debug.Log("[Bytecode] " + output);
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }
    public static System.Collections.IEnumerator warn(CallData dat)
    {
        string output = "";
        bool first = true;
        object[] inp = Luau.getAllArgs(ref dat);
        foreach (object arg in inp)
        {
            if (!first)
            {
                output += " ";
            }
            output += Luau.accurate_tostring(arg);
            first = false;
        }
        UnityEngine.Debug.LogWarning("[Bytecode] " + output);
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }
    public static System.Collections.IEnumerator tick(CallData dat)
    {
        Luau.returnToProto(ref dat, new object[1] { Time.realtimeSinceStartupAsDouble });
        yield break;
    }
    public static System.Collections.IEnumerator tonumber(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Type tp = inp[0].GetType();
        if (tp == typeof(double))
        {
            Luau.returnToProto(ref dat, new object[1] { inp[0] });
        } else if (tp == typeof(string) && double.TryParse((string)inp[0], out double val))
        {
            Luau.returnToProto(ref dat, new object[1] { val });
        } else
        {
            Luau.returnToProto(ref dat, new object[1] { null });
        }
        yield break;
    }
    public static System.Collections.IEnumerator tostring(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Luau.returnToProto(ref dat, new object[1] { (inp[0] == null ? "nil" : inp[0].ToString()) });
        yield break;
    }
    public static System.Collections.IEnumerator pairs(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        if (inp[0].GetType() == typeof(object[]))
        {
            var iter = new TableIterator((object[])inp[0]);
            Luau.returnToProto(ref dat, new object[3] { iter, inp[0], null });
        } else
        {
            var iter = new ArrayIterator((Dictionary<string, object>)inp[0]);
            Luau.returnToProto(ref dat, new object[3] { iter, inp[0], null });
        }
        yield break;
    }
    public static Dictionary<string, object> _G = new Dictionary<string, object>();
}