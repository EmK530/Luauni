#pragma warning disable CS8601
#pragma warning disable CS8625
#pragma warning disable CS8981

global using Instruction = System.UInt32;
using System;

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
}

public static class ParseEssentials
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
        return f;
    }
    public static Proto PrepareProto(ByteReader br, int i, string[] tbl)
    {
        Proto p = new Proto();
        p.p = null;
        p.sizep = 0;
        p.flags = 0;
        p.codeentry = null;
        p.registers = new object[255];
        p.instpos = -1;

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