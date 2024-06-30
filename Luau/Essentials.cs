#pragma warning disable CS8632

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class RegisterManager
{
    private object[] _registers;
    public byte stacktop = 0;

    public RegisterManager(int size)
    {
        _registers = new object[size];
    }

    public object this[long index]
    {
        get
        {
            if (index > stacktop)
            {
                stacktop = (byte)index;
            }
            return _registers[index];
        }
        set
        {
            _registers[index] = value;
            if (index > stacktop)
            {
                stacktop = (byte)index;
            }
        }
    }
}

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
    public Dictionary<string, object> imports;
    public object[] k;
    public RegisterManager registers;
    public uint callReg;
    public uint expectedReturns;
    public byte stacktop
    {
        get { return registers.stacktop; }
    }
    public object recentNameCalledRegister;
    public bool globalErrored = false;
}

public enum YieldType
{
    WaitingForSignal = 0,
    Hybrid = 1,
    Task = 2
}

public class SClosure
{
    public Closure source;
    public Closure lCC;
    public int cEL = 0;
    public List<int> iP = new List<int>();
    public List<Proto> pL = new List<Proto>();
    public List<Closure?> cL = new List<Closure?>();
    public object[] args;
    public bool initiated = false;
    public bool complete = false;

    public bool yielded = false;
    public YieldType type;
    public double yieldStart = 0;
    public double resumeAt = 0;
    public CallData yieldReturnTo;

    public uint nextInst()
    {
        iP[cEL]++;
        return pL[cEL].code[iP[cEL]];
    }

    public void jumpSteps(int steps)
    {
        iP[cEL] += steps;
    }
}

public class Closure
{
    public Proto p;
    public object[] upvals;
    public int loadedUps = 0;

    //custom
    public Luauni owner;
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
    public SClosure closure;
    public uint funcRegister;
    public uint args;
    public uint returns;
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
        if(v1 == null || v2 == null)
        {
            return v1 == null && v2 == null;
        }
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
        }
        return true;
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
        uint tern = p.returns == 0 ? (uint)args.Length : p.returns - 1;
        for (uint i = 0; i < tern; i++)
        {
            object tern2 = (i <= p.initiator.expectedReturns && i < args.Length) ? args[i] : null;
            p.initiator.registers[p.funcRegister + i] = tern2;
        }
    }
    public static object[] getAllArgs(ref CallData d)
    {
        object[] buf = new object[(d.args != 0 ? d.args - 1 : d.initiator.stacktop - d.funcRegister)];
        for (int i = 0; i < buf.Length; i++)
        {
            buf[i] = d.initiator.registers[d.funcRegister + i + 1];
        }
        return buf;
    }
    public static string accurate_tostring(object arg, bool quotes = false)
    {
        if (arg == null)
            return "nil";
        Type tp = arg.GetType();
        if (tp == typeof(bool))
            return (bool)arg ? "true" : "false";
        if (tp == typeof(string))
            return quotes ? '"'+(string)arg+'"' : (string)arg;
        if (tp == typeof(object[]))
        {
            object[] col = (object[])arg;
            string build = "{";
            bool first = true;
            foreach(object o in col)
            {
                if (!first)
                {
                    build += ", ";
                }
                build += accurate_tostring(o, true);
                first = false;
            }
            build += "}";
            return build;
        }
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
    public static Proto PrepareProto(ByteReader br, int i, string[] tbl, Transform src, Proto[] protos, Luauni owner)
    {
        Proto p = new Proto();
        p.p = null;
        p.sizep = 0;
        p.flags = 0;
        p.codeentry = null;
        p.registers = new RegisterManager(256);
        p.imports = new Dictionary<string, object>();
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
        BindingFlags search = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
        int tables = 0;
        for (int j = 0; j < p.sizek; ++j)
        {
            LuauBytecodeTag read = (LuauBytecodeTag)br.ReadByte();
            //Logging.Debug("j: " + j + " ("+read+")");
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
                    for (int k = 0; k < keys; ++k)
                    {
                        int temp = br.ReadVariableLen();
                        kt.Add(tbl[temp - tables],null); // hacky solution because there was a bug
                    }
                    tables++;
                    p.k[j] = kt;
                    break;
                case LuauBytecodeTag.LBC_CONSTANT_IMPORT:
                    {
                        uint aux = br.ReadUInt32(Endian.Little);
                        int pathLength = (int)(aux >> 30);
                        List<string> importPathParts = new List<string>();
                        for (int a = 0; a < pathLength; a++)
                        {
                            int shiftAmount = 10 * a;
                            int index = (int)((aux >> (20 - shiftAmount)) & 1023);
                            if (index >= p.k.Length)
                            {
                                Logging.Error($"Invalid constant index {index}.", "Luauni:Parse:PP");
                            }
                            else
                            {
                                importPathParts.Add(p.k[index].ToString());
                            }
                        }
                        string importPath = string.Join('.', importPathParts);
                        Logging.Debug($"Resolving import constant: {importPath}", "Luauni:Parse:PP");
                        bool first = true;
                        object current = null;
                        string last = "";
                        foreach(string index in importPathParts)
                        {
                            last = index;
                            if (first)
                            {
                                if (index == "script")
                                {
                                    current = src.gameObject;
                                }
                                else
                                {
                                    current = Globals.Get(index);
                                }
                                first = false;
                            }
                            else
                            {
                                if (current == null)
                                {
                                    break;
                                }
                                else
                                {
                                    Type t = Misc.SafeType(current);
                                    if (t == typeof(Dictionary<string, object>))
                                    {
                                        Dictionary<string, object> arr = (Dictionary<string, object>)current;
                                        current = arr[index];
                                    }
                                    else if (t == typeof(GameObject) || index == "camera")
                                    {
                                        GameObject obj = (GameObject)(t == typeof(GameObject) ? current : Misc.SafeGameObjectFromClass(current));
                                        Transform find = obj.transform.Find(index);
                                        if (find != null)
                                        {
                                            current = Misc.TryGetType(find);
                                        }
                                        else
                                        {
                                            Logging.Debug($"{index} is not a valid member of {t.Name}", "Luauni:Parse:PP"); current = null; break;
                                        }
                                    }
                                    else if (t == typeof(NamedDict))
                                    {
                                        NamedDict nd = (NamedDict)current;
                                        if (nd.dict.TryGetValue(index, out object val))
                                        {
                                            current = val;
                                        }
                                        else
                                        {
                                            Logging.Debug($"{index} is not a valid member of {t.Name}", "Luauni:Parse:PP"); current = null; break;
                                        }
                                    }
                                    else
                                    {
                                        FieldInfo test = t.GetField(index, search);
                                        Type test2 = t.GetNestedType(index, search);
                                        MethodInfo test3 = t.GetMethod(index, search);
                                        PropertyInfo test4 = t.GetProperty(index, search);
                                        if (test != null)
                                        {
                                            object send = test.GetValue(test.IsStatic ? null : current);
                                            current = send;
                                        }
                                        else if (test2 != null)
                                        {
                                            current = test2;
                                        }
                                        else if (test3 != null)
                                        {
                                            current = (Globals.Standard)Delegate.CreateDelegate(typeof(Globals.Standard), test3.IsStatic ? null : current, test3);
                                        }
                                        else if (test4 != null)
                                        {
                                            object send = test4.GetValue(current);
                                            current = send;
                                        }
                                        else
                                        {
                                            FieldInfo isObject = t.GetField("isObject");
                                            if (isObject == null)
                                            {
                                                Logging.Error($"Internal error: isObject not part of class {t.Name}", "Luauni:Parse:PP"); current = null; break;
                                            }
                                            bool indexable = (bool)isObject.GetValue(t);
                                            if (indexable)
                                            {
                                                FieldInfo f1 = t.GetField("source", search);
                                                GameObject obj;
                                                if (f1 != null)
                                                {
                                                    obj = (GameObject)(f1.GetValue(current));
                                                }
                                                else
                                                {
                                                    obj = ((Component)current).gameObject;
                                                }
                                                Transform find = obj.transform.Find(index);
                                                if (find != null)
                                                {
                                                    current = Misc.TryGetType(find);
                                                }
                                                else
                                                {
                                                    Logging.Debug($"{index} is not a valid member of {t.Name}", "Luauni:Parse:PP"); current = null; break;
                                                }
                                            }
                                            else
                                            {
                                                Logging.Debug($"{index} is not a valid member of {t.Name}", "Luauni:Parse:PP"); current = null; break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if(current == null)
                        {
                            Logging.Warn("Could not resolve import path: " + importPath, "Luauni:Parse:PP");
                        }
                        p.imports.Add(importPath, current);
                        p.k[j] = current;
                        //Logging.Print($"Resolved to: {current}", "Luauni:Parse:PP");
                        break;
                    }
                case LuauBytecodeTag.LBC_CONSTANT_CLOSURE:
                    //Logging.Warn("Found unimplemented closure constant.", "Luauni:Parse:PP");
                    int fid = br.ReadVariableLen();
                    Closure newclosure = new Closure()
                    {
                        p = protos[fid],
                        upvals = new object[protos[fid].nups],
                        owner = owner
                    };
                    p.k[j] = newclosure;
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
    public static IEnumerator ExecuteCoroutine(IEnumerator coroutine)
    {
        Stack<IEnumerator> stack = new Stack<IEnumerator>();
        stack.Push(coroutine);

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            bool moveNext = current.MoveNext();

            if (moveNext)
            {
                if (current.Current is IEnumerator next)
                {
                    stack.Push(next);
                }
                else
                {
                    yield return current.Current;
                }
            }
            else
            {
                stack.Pop();
            }
        }
    }

    public static IEnumerator SummonClosure(Closure cl, object[] args)
    {
        SClosure create = new SClosure()
        {
            source = cl,
            args = args
        };
        create.iP.Add(-1);
        create.pL.Add(cl.p);
        create.cL.Add(null);
        yield return ExecuteCoroutine(TaskScheduler.instance.Spawn(create));
        yield break;
    }
    public static void SummonClosureHybrid(Closure cl, object[] args)
    {
        SClosure create = new SClosure()
        {
            source = cl,
            args = args
        };
        create.iP.Add(-1);
        create.pL.Add(cl.p);
        create.cL.Add(null);
        TaskScheduler.instance.SpawnHybrid(create);
    }
    public static void SummonClosureTask(Closure cl, object[] args)
    {
        SClosure create = new SClosure()
        {
            source = cl,
            args = args
        };
        create.iP.Add(-1);
        create.pL.Add(cl.p);
        create.cL.Add(null);
        TaskScheduler.instance.SpawnTask(create);
    }
    public static void YieldClosure(ref SClosure closure, YieldType type, double duration, ref CallData returns)
    {
        closure.yielded = true;
        closure.type = type;
        closure.yieldStart = Time.realtimeSinceStartupAsDouble;
        closure.resumeAt = Time.realtimeSinceStartupAsDouble + duration;
        closure.yieldReturnTo = returns;
    }
    public static void YieldClosureForever(ref SClosure closure, double duration, ref CallData returns)
    {
        closure.yielded = true;
        closure.type = YieldType.WaitingForSignal;
        closure.yieldStart = Time.realtimeSinceStartupAsDouble;
        closure.resumeAt = Time.realtimeSinceStartupAsDouble + duration;
        closure.yieldReturnTo = returns;
    }
    public static string GetTypeName(object value)
    {
        if(value == null)
        {
            return "nil";
        }
        Type t = value.GetType();
        if(t == typeof(object[]) || t == typeof(NamedDict) || t == typeof(Dictionary<string, object>))
        {
            return "table";
        }
        return t.ToString();
    }
    public static object TryGetType(Transform v2)
    {
        Type type = GetTypeByName(v2.tag);
        if (type != null)
        {
            var component = v2.gameObject.GetComponent(type);
            if (component != null)
            {
                return component;
            }
        }
        if (Logging.ShowDebug)
            Logging.Warn($"Returned GameObject with no known class: {v2.name}", "Luauni:Misc:TGT");
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
        else
        {
            BindingFlags search = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
            FieldInfo src = t.GetField("source", search);
            if(src != null)
            {
                return (GameObject)(t.GetField("source").GetValue(input));
            } else
            {
                return ((Component)input).gameObject;
            }
        }
    }
}