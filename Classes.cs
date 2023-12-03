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
    public static int INSN_D(Instruction insn) => (int)(insn >> 16);
    public static int INSN_E(Instruction insn) => (int)(insn >> 8);
}