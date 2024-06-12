#pragma warning disable CS8632

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.InputManagerEntry;

public class Proto
{
    public byte nups;
    public byte numparams;
    public byte is_vararg;
    public byte maxstacksize;
    public byte flags;
    public int bytecodeid;
    public int sizecode;
    public int sizek;
    public int sizep;
    public Proto[] p;
    public uint[] code;
    public uint[] codeentry;
    public object[] k; // danger
    public object?[] registers; // custom
    public uint callReg;
    public int expectedReturns;
    public object[] lastReturn;
    public object recentNameCalledRegister;
    public bool globalErrored = false;
}

public class Closure
{
    public Proto p;
    public object[] upvals;
    public int loadedUps = 0;
}

public class UpvalREF
{
    public Proto src;
    public uint register;
}

public class NamedDict
{
    public string name;
    public Dictionary<string, object> dict;
}

public class CallData
{
    public Proto initiator;
    public uint funcRegister;
    public int args;
    public int returns;
}

public class TableIterator
{
    private object[] table;
    private int index = 0;

    public TableIterator(object[] table)
    {
        this.table = table;
        this.index = -1;
    }

    public (bool, object[]) Get()
    {
        this.index++;
        if (this.index < this.table.Length)
        {
            return (true, new object[2] { this.index + 1, this.table[this.index] });
        } else
        {
            return (false, new object[0]);
        }
    }
}

public class ArrayIterator
{
    private Dictionary<string, object> array;
    private Dictionary<string, object>.Enumerator e;

    public ArrayIterator(Dictionary<string, object> array)
    {
        this.array = array;
        e = array.GetEnumerator();
    }

    public (bool, object[]) Get()
    {
        if (e.MoveNext())
        {
            return (true, new object[2] { e.Current.Key, e.Current.Value });
        }
        else
        {
            return (false, new object[0]);
        }
    }
}

public static class Luau
{
    public static uint INSN_OP(uint insn) => insn & 0xFF;
    public static uint INSN_A(uint insn) => (insn >> 8) & 0xFF;
    public static uint INSN_B(uint insn) => (insn >> 16) & 0xFF;
    public static uint INSN_C(uint insn) => (insn >> 24) & 0xFF;
    public static int INSN_D(uint insn) => (int)((int)insn >> 16);
    public static int INSN_E(uint insn) => (int)(insn >> 8);
    public static bool EQUAL(object v1, object v2)
    {
        Type t1 = v1.GetType();
        Type t2 = v2.GetType();
        if (t1 == typeof(double) && t2 == typeof(double))
        {
            return (double)v1 == (double)v2;
        }
        else if (t1 == typeof(string) && t2 == typeof(string))
        {
            return (string)v1 == (string)v2;
        }
        else if (t1 == typeof(double) || t2 == typeof(double))
        {
            double d1 = 0;
            double d2 = 0;
            if (t1 == typeof(double))
            {
                d1 = (double)v1;
                if(double.TryParse((string)v2, out double c2)) { d2 = c2; } else { return false; }
            } else
            {
                d2 = (double)v2;
                if (double.TryParse((string)v1, out double c1)) { d1 = c1; } else { return false; }
            }
            return d1 == d2;
        }
        else
        {
            return v1 == v2;
        }
    }
    public static bool LIKELY(object v1)
    {
        if (v1 == null)
            return false;
        Type t = v1.GetType();
        if (t == typeof(bool)) {
            return (bool)v1;
        } else if (t == typeof(double)){
            return ((double)v1) == 0;
        } else if (t == typeof(string)){
            return ((string)v1) == "";
        }
        return false;
    }
    public static object SAFEINDEX(object v1, string key)
    {
        object inner = v1;
        if (inner == null)
            return null;
        foreach (string k in key.Split("."))
        {
            Dictionary<string, object> c1 = (Dictionary<string, object>)inner;
            if (c1.TryGetValue(k, out object t1))
            {
                if (t1 == null)
                    return null;
                inner = t1;
            }
            else
            {
                return null;
            }
        }
        return inner;
    }
    public static double safeNum(object inp)
    {
        if (inp == null)
        {
            return 0d;
        }
        else
        {
            return (double)inp;
        }
    }
    public static void returnToProto(ref CallData p, object[] args)
    {
        int tern = p.returns == -1 ? args.Length : p.returns;
        p.initiator.lastReturn = new object[tern];
        for (int i = 0; i < tern; i++)
        {
            object tern2 = i < args.Length ? args[i] : null;
            p.initiator.registers[p.funcRegister + i] = tern2;
            p.initiator.lastReturn[i] = tern2;
        }
    }
    public static object[] getAllArgs(ref CallData d)
    {
        if (d.args != -1)
        {
            object[] buf = new object[d.args];
            for (int i = 0; i < d.args; i++)
            {
                buf[i] = d.initiator.registers[d.funcRegister + i + 1];
            }
            return buf;
        } else
        {
            return d.initiator.lastReturn;
        }
    }
    public static string accurate_tostring(object arg)
    {
        if (arg == null)
            return "nil";
        Type tp = arg.GetType();
        if (tp == typeof(bool))
            return (bool)arg ? "true" : "false";
        if (tp == typeof(double))
        {
            if (double.IsNaN((double)arg))
                return "nan";
            return arg.ToString().Replace(",", ".");
        }
        return arg.ToString();
    }
    public static ulong randstate = 0;
    public static uint pcg32_random()
    {
        ulong oldstate = randstate;
        randstate = oldstate * 6364136223846793005UL + (105 | 1);
        uint xorshifted = (uint)(((oldstate >> 18) ^ oldstate) >> 27);
        uint rot = (uint)(oldstate >> 59);
        return (xorshifted >> (int)rot) | (xorshifted << (-(int)rot & 31));
    }
    public static void pcg32_seed(ulong seed)
    {
        randstate = 0;
        pcg32_random();
        randstate += seed;
        pcg32_random();
    }
}

public static class ParseEssentials
{
    public static Proto PrepareProto(ByteReader br, int i, string[] tbl)
    {
        Proto p = new Proto();
        p.p = null;
        p.sizep = 0;
        p.flags = 0;
        p.codeentry = null;
        p.registers = new object[255];

        p.bytecodeid = i;
        p.maxstacksize = br.ReadByte();
        p.numparams = br.ReadByte();
        p.nups = br.ReadByte();
        p.is_vararg = br.ReadByte();
        br.Skip(1); br.Skip(br.ReadVariableLen());
        p.sizecode = br.ReadVariableLen();
        Logging.Debug($"Loading {p.sizecode * 4} bytes of bytecode into Proto {i}", "Luauni:Parse:PP");
        p.code = new uint[p.sizecode];
        for (int j = 0; j < p.sizecode; j++)
        {
            p.code[j] = br.ReadUInt32(Endian.Little);
        }
        p.sizek = br.ReadVariableLen();
        Logging.Debug($"Loading {p.sizek} constants for Proto {i}", "Luauni:Parse:PP");
        p.k = new object[p.sizek];

        int tables = 0;
        for (int j = 0; j < p.sizek; ++j)
        {
            LuauBytecodeTag read = (LuauBytecodeTag)br.ReadByte();
            Logging.Debug("j: " + j + " ("+read+")");
            switch (read)
            {
                case LuauBytecodeTag.LBC_CONSTANT_NIL:
                    p.k[j] = null;
                    break;
                case LuauBytecodeTag.LBC_CONSTANT_BOOLEAN:
                    p.k[j] = (br.ReadByte() == 1);
                    break;
                case LuauBytecodeTag.LBC_CONSTANT_NUMBER:
                    p.k[j] = BitConverter.ToDouble(br.ReadRange(8));
                    break;
                case LuauBytecodeTag.LBC_CONSTANT_VECTOR:
                    Logging.Warn("Found unimplemented vector constant.", "Luauni:Parse:PP");
                    br.Skip(16); //TODO: Implement vectors when migrating to Unity.
                    break;
                case LuauBytecodeTag.LBC_CONSTANT_STRING:
                    int id = br.ReadVariableLen();
                    string? rd = id == 0 ? null : tbl[id - 1];
                    if (rd == null)
                        Logging.Warn("Invalid string constant ID: " + id, "Luauni:Parse:PP");
                    p.k[j] = rd;
                    break;
                case LuauBytecodeTag.LBC_CONSTANT_TABLE:
                    Dictionary<string, object> kt = new Dictionary<string, object>(); // key table
                    int keys = br.ReadVariableLen();
                    Logging.Warn(keys);
                    for (int k = 0; k < keys; ++k)
                    {
                        int temp = br.ReadVariableLen();
                        Logging.Debug(temp);
                        Logging.Debug(tbl[temp - tables]);
                        kt.Add(tbl[temp - tables],null); // hacky solution because there was a bug
                    }
                    tables++;
                    p.k[j] = kt;
                    break;
                case LuauBytecodeTag.LBC_CONSTANT_IMPORT:
                    Logging.Warn("Found unimplemented import constant.", "Luauni:Parse:PP");
                    //TODO: understand what the hell this is and don't skip
                    br.Skip(4);
                    break;
                case LuauBytecodeTag.LBC_CONSTANT_CLOSURE:
                    Logging.Warn("Found unimplemented closure constant.", "Luauni:Parse:PP");
                    //TODO: understand what the hell this is and don't skip
                    int fid = br.ReadVariableLen();
                    //that's it lol for now
                    break;
                default:
                    Logging.Warn("Unknown bytecode tag " + read, "Luauni:Parse:PP");
                    break;
            }
        }
        p.sizep = br.ReadVariableLen();
        p.p = new Proto[p.sizep];
        return p;
    }
}

public static class Misc
{
    public static object TryGetType(Transform v2)
    {
        Type type = GetTypeByName(v2.tag);
        if (type != null)
        {
            var component = v2.gameObject.GetComponent(type);
            if (component != null)
            {
                Logging.Warn("Returning component!!!");
                return component;
            }
        }
        Logging.Warn($"Returned GameObject with no known class: {v2.name}", "Luauni:Misc:TGRV");
        return v2.gameObject;
    }
    public static (bool,Component) TryGetTypeStrict(Transform v2)
    {
        Type type = GetTypeByName(v2.tag);
        if (type != null)
        {
            var component = v2.gameObject.GetComponent(type);
            if (component != null)
            {
                return (true, component);
            } else
            {
                return (false, null);
            }
        }
        else
        {
            return (false,null);
        }
    }
    public static Type GetTypeByName(string name)
    {
        return Type.GetType(name);
    }
    public static Type SafeType(object input)
    {
        if (input is Type)
        {
            Logging.Debug("Safe type");
            return (Type)input;
        }
        else
        {
            return input.GetType();
        }
    }
    public static GameObject SafeGameObjectFromClass(object input)
    {
        Type t = SafeType(input);
        if (t == typeof(GameObject))
        {
            return (GameObject)input;
        }
        else if (t == typeof(Component))
        {
            return ((Component)input).gameObject;
        }
        else
        {
            Type cast = (Type)t;
            FieldInfo src = cast.GetField("source");
            if(src != null)
            {
                return (GameObject)(cast.GetField("source").GetValue(cast));
            }
        }
        return null;
    }
}