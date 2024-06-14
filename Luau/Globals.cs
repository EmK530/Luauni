#pragma warning disable CS8601
#pragma warning disable CS8603
#pragma warning disable CS8625
#pragma warning disable CS8981

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

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
        list["game"] = DataModel.instance.GetType();
        list["workspace"] = Workspace.instance.GetType();
        list["Enum"] = typeof(Enum);
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
            if (value.GetType() == typeof(Dictionary<string, object>))
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
            Logging.Warn($"Global name '{name}' is not registered.", "Globals:Get");
            return null;
        }
    }
    public static void Set(string name, object value)
    {
        list[name] = value;
    }
}

public static class GC
{
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
    public static class math
    {
        //missing abs
        //missing acos
        //missing asin
        //missing atan
        //missing atan2
        //missing ceil
        //missing clamp
        //missing cos
        //missing deg
        //missing exp
        //missing floor
        //missing fmod
        //missing frexp
        public static double huge = double.PositiveInfinity;
        //missing ldexp
        //missing log
        //missing log10
        //missing max
        //missing min
        //missing modf
        //missing noise
        public static double pi = 3.1415926535897931;
        //missing pow
        //missing rad
        public static System.Collections.IEnumerator random(CallData dat)
        {
            object[] inp = Luau.getAllArgs(ref dat);
            switch (dat.args)
            {
                case 0:
                    {
                        Luau.returnToProto(ref dat, new object[1] { NMath.ldexp(Luau.pcg32_random() | ((ulong)Luau.pcg32_random() << 32), -64) });
                        yield break;
                    }
                case 1:
                    {
                        Luau.returnToProto(ref dat, new object[1] { (double)(1 + ((Convert.ToUInt64(inp[0]) * Luau.pcg32_random()) >> 32)) });
                        yield break;
                    }
                case 2:
                    {
                        int l = Convert.ToInt32((double)inp[0]);
                        int u = Convert.ToInt32((double)inp[1]);
                        Luau.returnToProto(ref dat, new object[1] { (double)(l + (int)((((uint)u - (uint)l + 1UL) * Luau.pcg32_random()) >> 32)) });
                        yield break;
                    }
                default:
                    Logging.Error("Unsupported number of parameters! Returning 0.", "Globals:math.random");
                    Luau.returnToProto(ref dat, new object[1] { 0d });
                    yield break;
            }
        }
        public static System.Collections.IEnumerator randomseed(CallData dat)
        {
            object[] inp = Luau.getAllArgs(ref dat);
            Luau.pcg32_seed(Convert.ToUInt64((double)inp[0]));
            yield break;
        }
        public static System.Collections.IEnumerator round(CallData dat)
        {
            object[] inp = Luau.getAllArgs(ref dat);
            Luau.returnToProto(ref dat, new object[1] { Math.Round(Luau.safeNum(inp[0])) });
            yield break;
        }
        //missing sign
        //missing sin
        //missing sinh
        public static System.Collections.IEnumerator sqrt(CallData dat)
        {
            object[] inp = Luau.getAllArgs(ref dat);
            Luau.returnToProto(ref dat, new object[1] { Math.Sqrt(Luau.safeNum(inp[0])) });
            yield break;
        }
        public static System.Collections.IEnumerator tan(CallData dat)
        {
            object[] inp = Luau.getAllArgs(ref dat);
            Luau.returnToProto(ref dat, new object[1] { Math.Tan(Luau.safeNum(inp[0])) });
            yield break;
        }
        //missing tanh
    }
}