using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class Essentials
{
    public static void system(string cmd)
    {
        Process process = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = "/C " + cmd;
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }
}

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
    public uint[] code;
    public uint[] codeentry;
    public object[] k; // danger
    public object?[] registers; // custom
    public int instpos; // custom
    public object? lastReturn; // custom
    public int lastReturnCount; // custom
}

public struct Closure
{
    public CommonHeader hdr;
    public byte isC;
    public byte nupvalues;
    public byte stacksize;
    public byte preload;
    //fuck the gclist
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
        Type type = v1.GetType();
        if (type == typeof(double))
        {
            return (double)v1 == (double)v2;
        } else if (type == typeof(string)) {
            return (string)v1 == (string)v2;
        } else
        {
            return v1 == v2;
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
    public static Instance NewInstance(GameObject inp)
    {
        Instance i = new Instance();
        i.src = inp;
        return i;
    }
}

public class Instance
{
    public GameObject src;
    public object[] Clone(params object[] args)
    {
        GameObject test = GameObject.Instantiate(src, CGNI.InstLocation);
        test.name = src.name;
        return new object[1] { test };
    }
    public object[] GetMouse(params object[] args)
    {
        return new object[1] { CGNI.ms };
    }
    public object[] GetService(params object[] args)
    {
        return new object[1] { Luau.NewInstance(CGNI.game.transform.Find((string)args[1]).gameObject) };
    }
}

public static class _G
{
    public static Dictionary<string, object> dict = new Dictionary<string, object>();
}

public static class lfunc
{
    public static Proto luaF_newproto()
    {
        Proto f = new Proto();
        f.maxstacksize = 0;
        f.numparams = 0;
        f.nups = 0;
        f.is_vararg = 0;
        f.k = null;
        f.sizek = 0;
        f.p = null;
        f.sizep = 0;
        f.code = null;
        f.sizecode = 0;
        f.flags = 0;
        f.codeentry = null;
        f.registers = new object[255];
        f.instpos = -1;
        f.lastReturn = null;
        f.lastReturnCount = 0;
        return f;
    }
}
