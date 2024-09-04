#pragma warning disable CS8601
#pragma warning disable CS8603
#pragma warning disable CS8625
#pragma warning disable CS8981

using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public static class Globals
{
    private static Dictionary<string, Dictionary<string, dynamic>> closureList = new Dictionary<string, Dictionary<string, dynamic>>();

    public static Dictionary<string, dynamic> list = new Dictionary<string, dynamic>()
    {
        ["game"] = DataModel.instance.GetType(),
        ["workspace"] = Workspace.instance.GetType(),
        ["Enum"] = typeof(Enum),
        ["Vector2"] = typeof(Vector2),
        ["Vector3"] = typeof(Vector3),
        ["Color3"] = typeof(Color3),
        ["Region3"] = typeof(Region3),
        ["BrickColor"] = typeof(BrickColor),
        ["Ray"] = typeof(Ray),
        ["CFrame"] = typeof(CFrame),
        ["UDim2"] = typeof(UDim2),
        ["string"] = typeof(String),
        ["math"] = typeof(math),
        ["task"] = typeof(task)
    };
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
        initialized = true;
    }

    public static void Register(ref SClosure target)
    {
        string hsh = target.source.hash;
        if(!closureList.ContainsKey(hsh))
        {
            closureList.Add(hsh, list);
        } else
        {
            closureList[hsh] = list;
        }
    }

    public static void IterateClass(Type i, Dictionary<string, dynamic> contain, string path, bool top)
    {
        foreach (Type t in i.GetNestedTypes())
        {
            Logging.Debug($"Binding global class '{path+(top?"":".")+t.Name}'", "Globals:IterateClass");
            contain[t.Name] = new Dictionary<string, dynamic>();
            string path2 = path;
            if (!top)
            {
                path2 += ".";
            }
            path2 += t.Name;
            IterateClass(t, (Dictionary<string, dynamic>)contain[t.Name], path2, false);
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
    private static dynamic GetInternal(Dictionary<string, dynamic> list, string name)
    {
        Logging.Debug($"Get global '{name}'", "Globals:GetDefault");
        if (list.TryGetValue(name, out var value))
        {
            if (value != null && value.GetType() == typeof(Dictionary<string, dynamic>))
            {
                return new NamedDict()
                {
                    name = name,
                    dict = (Dictionary<string, dynamic>)value
                };
            }
            else
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
    public static dynamic Get(ref SClosure target, string name)
    {
        Logging.Debug($"Get global '{name}'", "Globals:Get");
        Dictionary<string, dynamic> lst = closureList[target.source.hash];
        return GetInternal(lst, name);
    }
    public static dynamic Get(string name)
    {
        Logging.Debug($"Get global '{name}'", "Globals:Get");
        return GetInternal(list, name);
    }
    public static dynamic NativeGet(string name)
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
    public static void Set(ref SClosure target, string name, dynamic value)
    {
        closureList[target.source.hash][name] = value;
    }
}

public static class GC
{
    public static System.Collections.IEnumerator wait(CallData dat)
    {
        dynamic[] inp = Luau.getAllArgs(ref dat);
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
        dynamic[] inp = Luau.getAllArgs(ref dat);
        if (inp[0] is Closure cl)
        {
            dynamic[] misc = new dynamic[inp.Length - 1];
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
        dynamic[] inp = Luau.getAllArgs(ref dat);
        foreach (dynamic arg in inp)
        {
            if (!first)
            {
                output += " ";
            }
            output += Luau.accurate_tostring(arg);
            first = false;
        }
        UnityEngine.Debug.Log("[Bytecode] " + output);
        Luau.returnToProto(ref dat, new dynamic[0]);
        yield break;
    }
    public static System.Collections.IEnumerator warn(CallData dat)
    {
        string output = "";
        bool first = true;
        dynamic[] inp = Luau.getAllArgs(ref dat);
        foreach (dynamic arg in inp)
        {
            if (!first)
            {
                output += " ";
            }
            output += Luau.accurate_tostring(arg);
            first = false;
        }
        UnityEngine.Debug.LogWarning("[Bytecode] " + output);
        Luau.returnToProto(ref dat, new dynamic[0]);
        yield break;
    }
    public static System.Collections.IEnumerator tick(CallData dat)
    {
        Luau.returnToProto(ref dat, new dynamic[1] { Time.realtimeSinceStartupAsDouble });
        yield break;
    }
    public static System.Collections.IEnumerator tonumber(CallData dat)
    {
        dynamic[] inp = Luau.getAllArgs(ref dat);
        Type tp = inp[0].GetType();
        if (tp == typeof(double))
        {
            Luau.returnToProto(ref dat, new dynamic[1] { inp[0] });
        } else if (tp == typeof(string) && double.TryParse((string)inp[0], out double val))
        {
            Luau.returnToProto(ref dat, new dynamic[1] { val });
        } else
        {
            Luau.returnToProto(ref dat, new dynamic[1] { null });
        }
        yield break;
    }
    public static System.Collections.IEnumerator tostring(CallData dat)
    {
        dynamic[] inp = Luau.getAllArgs(ref dat);
        Luau.returnToProto(ref dat, new dynamic[1] { (inp[0] == null ? "nil" : inp[0].ToString()) });
        yield break;
    }
    public static System.Collections.IEnumerator pairs(CallData dat)
    {
        dynamic[] inp = Luau.getAllArgs(ref dat);
        dynamic v1 = inp[0];
        if(v1 == null)
        {
            Logging.Error($"invalid argument #1 to 'pairs' (table expected, got nil)", "Luauni:Globals:pairs"); dat.initiator.globalErrored = true; yield break;
        }
        if (v1.GetType() == typeof(dynamic[]))
        {
            var iter = new TableIterator(v1);
            Luau.returnToProto(ref dat, new dynamic[3] { iter, v1, null });
        } else {
            var iter = new ArrayIterator(v1);
            Luau.returnToProto(ref dat, new dynamic[3] { iter, v1, null });
        }
        yield break;
    }
    public static Dictionary<string, dynamic> _G = new Dictionary<string, dynamic>();
}