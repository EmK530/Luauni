#pragma warning disable CS8903
#pragma warning disable CS8981

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

public static class Globals
{
    public static Dictionary<string, object> list = new Dictionary<string, object>();
    public delegate CallResults Standard(ref CallData data);

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
        initialized = true;
    }
    private static void IterateClass(Type i, Dictionary<string, object> contain, string path, bool top)
    {
        foreach (Type t in i.GetNestedTypes())
        {
            contain[t.Name] = new Dictionary<string, object>();
            string path2 = path;
            if (!top)
            {
                path2 += ".";
            }
            path2 += t.Name;
            IterateClass(t, (Dictionary<string, object>)contain[t.Name], path2, false);
        }
        foreach (MethodInfo t in i.GetMethods(BindingFlags.Static | BindingFlags.Public).Cast<MethodInfo>())
        {
            Logging.Debug($"Binding global '{path+(top?"":".")+t.Name}'", "Globals:IterateClass");
            contain[t.Name] = (Standard)t.CreateDelegate(typeof(Standard));
        }
    }
    public static object Get(string name)
    {
        if(list.TryGetValue(name, out var value))
        {
            return value;
        }
        else
        {
            Logging.Error($"Global name '{name}' is not registered.", "Globals:Get");
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
    public static CallResults print(ref CallData dat)
    {
        string output = "";
        bool first = true;
        for(int i = 0; i < dat.args; i++)
        {
            var a = dat.initiator.registers[dat.funcRegister + i + 1];
            if (!first)
            {
                output += " ";
            }
            output += a.ToString();
            first = false;
        }
        Console.WriteLine("\x1b[7;30;47m[Bytecode] " + output + "\x1b[0m ");
        return new CallResults();
    }
    public static class math
    {
        public static CallResults random(ref CallData dat)
        {
            object[] inp = Luau.getAllArgs(ref dat);
            switch (dat.args)
            {
                case 0:
                    {
                        Luau.returnToProto(ref dat, new object[1] { NMath.ldexp(Luau.pcg32_random() | ((ulong)Luau.pcg32_random() << 32), -64) });
                        return new CallResults();
                    }
                case 1:
                    {
                        Luau.returnToProto(ref dat, new object[1] { (double)(1 + ((Convert.ToUInt64(inp[0]) * Luau.pcg32_random()) >> 32)) });
                        return new CallResults();
                    }
                case 2:
                    {
                        int l = Convert.ToInt32((double)inp[0]);
                        int u = Convert.ToInt32((double)inp[1]);
                        Luau.returnToProto(ref dat, new object[1] { (double)(l + (int)((((uint)u - (uint)l + 1UL) * Luau.pcg32_random()) >> 32)) });
                        return new CallResults();
                    }
                default:
                    Logging.Error("Unsupported number of parameters! Returning 0.", "Globals:math.random");
                    Luau.returnToProto(ref dat, new object[1] { 0d });
                    return new CallResults();
            }
        }
        public static CallResults randomseed(ref CallData dat)
        {
            object[] inp = Luau.getAllArgs(ref dat);
            Luau.pcg32_seed(Convert.ToUInt64((double)inp[0]));
            return new CallResults();
        }
    }
}