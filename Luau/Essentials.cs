#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable CS8981

global using Instruction = System.UInt32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

public struct CommonHeader
{
    public byte tt;
    public byte marked;
    public byte memcat;
}

public struct Proto
{
    public CommonHeader hdr;
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
    public Instruction[] code;
    public Instruction[] codeentry;
    public object[] k; // danger
    public object?[] registers; // custom
    public uint callReg;
    public int expectedReturns;
    public object[] lastReturn;
}

public enum CallStatus
{
    Finished = 0,
    Yielded = 1,
    Errored = 2
}

public struct NamedDict
{
    public string name;
    public Dictionary<string, object> dict;
}

public ref struct CallData
{
    public ref Proto initiator;
    public uint funcRegister;
    public int args;
    public int returns;
}

public struct CallResults
{
    public CallStatus status;
    public float yieldDuration;

    public CallResults()
    {
        status = CallStatus.Finished;
        yieldDuration = 0f;
    }
}

public static class Luau
{
    public static uint INSN_OP(Instruction insn) => insn & 0xFF;
    public static uint INSN_A(Instruction insn) => (insn >> 8) & 0xFF;
    public static uint INSN_B(Instruction insn) => (insn >> 16) & 0xFF;
    public static uint INSN_C(Instruction insn) => (insn >> 24) & 0xFF;
    public static int INSN_D(Instruction insn) => (int)((int)insn >> 16);
    public static int INSN_E(Instruction insn) => (int)(insn >> 8);
    public static bool EQUAL(object v1, object v2)
    {
        Type type = v1.GetType();
        if (type == typeof(double))
        {
            return (double)v1 == (double)v2;
        }
        else if (type == typeof(string))
        {
            return (string)v1 == (string)v2;
        }
        else
        {
            return v1 == v2;
        }
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
        Proto copy = p.initiator;
        copy.lastReturn = new object[tern];
        for (int i = 0; i < tern; i++)
        {
            object tern2 = i < args.Length ? args[i] : null;
            copy.registers[p.funcRegister + i] = tern2;
            copy.lastReturn[i] = tern2;
        }
        p.initiator = copy;
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

        for (int j = 0; j < p.sizek; ++j)
        {
            LuauBytecodeTag read = (LuauBytecodeTag)br.ReadByte();
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
                    string? rd = id <= 0 ? null : tbl[id - 1];
                    if (rd == null)
                        Logging.Warn("Invalid string constant ID: " + id, "Luauni:Parse:PP");
                    p.k[j] = rd;
                    break;
                case LuauBytecodeTag.LBC_CONSTANT_TABLE:
                    Logging.Warn("Found unimplemented table constant.", "Luauni:Parse:PP");
                    //TODO: understand what the hell this is and don't skip
                    int keys = br.ReadVariableLen();
                    for (int k = 0; k < keys; k++)
                    {
                        br.ReadVariableLen();
                    }
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