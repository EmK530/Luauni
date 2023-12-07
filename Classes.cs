global using Instruction = System.UInt32;
using System.Diagnostics;

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
    public Instruction[] code;
    public Instruction[] codeentry;
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
    public static uint INSN_OP(Instruction insn) => insn & 0xFF;
    public static uint INSN_A(Instruction insn) => (insn >> 8) & 0xFF;
    public static uint INSN_B(Instruction insn) => (insn >> 16) & 0xFF;
    public static uint INSN_C(Instruction insn) => (insn >> 24) & 0xFF;
    public static int INSN_D(Instruction insn) => (int)((int)insn >> 16);
    public static int INSN_E(Instruction insn) => (int)(insn >> 8);
    public static bool EQUAL(object v1, object v2)
    {
        Type type = v1.GetType();
        Type type2 = v2.GetType();
        if (type == typeof(double) || type == typeof(int))
        {
            if (type == typeof(int))
            {
                v1 = Convert.ToDouble((int)v1);
            }
            if (type2 == typeof(int))
            {
                v2 = Convert.ToDouble((int)v2);
            }
            return (double)v1 == (double)v2;
        } else if (type == typeof(string)) {
            return (string)v1 == (string)v2;
        }
        else
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
