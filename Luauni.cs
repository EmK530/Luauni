#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CS8605

using Microsoft.VisualBasic;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.ComponentModel.Design;
using System.Collections.Generic;

public class Luauni
{
    FileStream file;

    private Dictionary<string, object> globals;
    string[] stringtable;
    int[] protopos;
    Proto[] protos;
    object lastreturn;
    int mainprotoId;
    Proto mainproto;
    int instPos = -1;

    public static bool db = false;

    public static void debug(object input) { if (db) { Console.WriteLine("\x1b[1;30;40m" + "[DEBUG] " + input + "\x1b[0m "); } }
    public static void print(object input) { Console.WriteLine("\x1b[7;30;47m" + input + "\x1b[0m "); }
    public static void warn(object input) { Console.WriteLine("\x1b[7;30;43m" + "[WARN] " + input + "\x1b[0m "); }
    public static void error(object input) { Console.WriteLine("\x1b[7;30;41m" + "[ERROR] " + input + "\x1b[0m "); }

    public Luauni(string filepath)
    {
        file = File.OpenRead(filepath);
        globals = new Dictionary<string, object>();
        SetGlobal("print", (Action<object[]>)CG.print);
    }

    private void SetGlobal(string name, object value)
    {
        globals[name] = value;
    }

    private object GetGlobal(string name)
    {
        if (globals.TryGetValue(name, out var value))
        {
            return value;
        }
        else
        {
            error($"Global variable '{name}' not found.");
            return null;
        }
    }

    private string? ReadString()
    {
        int id = ReadVariableLen();
        return id == 0 ? null : stringtable[id - 1];
    }

    private uint ReadUInt32LE()
    {
        //shush
        return (uint)file.ReadByte() + ((uint)file.ReadByte() * 256u) + ((uint)file.ReadByte() * 65536u) + ((uint)file.ReadByte() * 16777216u);
    }

    private uint ReadUInt32BE()
    {
        //shush x2
        return ((uint)file.ReadByte() * 16777216u) + ((uint)file.ReadByte() * 65536u) + ((uint)file.ReadByte() * 256u) + (uint)file.ReadByte();
    }

    private byte[] ReadRange(int count)
    {
        byte[] buf = new byte[count];
        file.Read(buf, 0, count);
        return buf;
    }

    private void Skip(int count)
    {
        file.Seek(count, SeekOrigin.Current);
    }

    private int ReadVariableLen()
    {
        int result = 0;
        int shift = 0;
        while(true)
        {
            byte b = (byte)file.ReadByte();
            result |= (b & 127) << shift;
            shift += 7;
            if ((b & 128) == 0)
            {
                break;
            }
        }
        return result;
    }

    public void Parse()
    {
        print("Perform initial parsing...");
        byte ver = (byte)file.ReadByte();
        byte varver = (byte)file.ReadByte();
        debug("Luau version: "+ver);
        debug("Type version: " + varver);
        if (ver != 5)
        {
            warn("Lua version mismatch! Expect issues!");
        }
        if (varver != 1)
        {
            warn("Type version mismatch! Expect issues!");
        }
        int strings = ReadVariableLen();
        debug("String table entries: " + strings);
        stringtable = new string[strings];
        for(int i = 0; i < strings; i++)
        {
            byte size = (byte)file.ReadByte();
            byte[] str = ReadRange(size);
            stringtable[i] = Encoding.UTF8.GetString(str);
            debug("String table entry: "+stringtable[i]);
        }
        debug("Built string table.");
        int protoCount = ReadVariableLen();
        debug("Proto table entries: " + protoCount);
        protos = new Proto[protoCount];
        protopos = new int[protoCount];
        for (int i = 0; i < protoCount; i++)
        {
            Proto p = lfunc.luaF_newproto();
            protopos[p.bytecodeid] = -1;
            debug("Making proto " + i);
            //wtf is source
            p.bytecodeid = i;
            p.maxstacksize = (byte)file.ReadByte();
            p.numparams = (byte)file.ReadByte();
            p.nups = (byte)file.ReadByte();
            p.is_vararg = (byte)file.ReadByte();
            Skip(1); Skip(ReadVariableLen()); // fuck these flags
            p.sizecode = ReadVariableLen();
            p.code = new uint[p.sizecode];
            for (int j = 0; j < p.sizecode; j++)
            {
                p.code[j] = ReadUInt32LE(); //little endian needed for correct Luau class function behavior
            }
            p.sizek = ReadVariableLen();
            p.k = new object[p.sizek];
            for (int j = 0; j < p.sizek; ++j)
            {
                LuauBytecodeTag read = (LuauBytecodeTag)file.ReadByte();
                switch (read)
                {
                    case LuauBytecodeTag.LBC_CONSTANT_NIL:
                        debug("Found nil constant.");
                        p.k[j] = null;
                        break;
                    case LuauBytecodeTag.LBC_CONSTANT_BOOLEAN:
                        debug("Found boolean constant.");
                        p.k[j] = (file.ReadByte() == 1);
                        break;
                    case LuauBytecodeTag.LBC_CONSTANT_NUMBER:
                        debug("Found number constant.");
                        p.k[j] = BitConverter.ToDouble(ReadRange(8));
                        break;
                    case LuauBytecodeTag.LBC_CONSTANT_VECTOR:
                        debug("Found vector constant.");
                        Skip(16); //TODO: Implement vectors when migrating to Unity.
                        break;
                    case LuauBytecodeTag.LBC_CONSTANT_STRING:
                        string rd = ReadString();
                        debug("Found string constant: " + rd); //retrieves the string ID from string table
                        p.k[j] = rd;
                        break;
                    case LuauBytecodeTag.LBC_CONSTANT_TABLE:
                        debug("Found table constant.");
                        //TODO: understand what the hell this is and don't skip
                        int keys = ReadVariableLen();
                        for(int k = 0; k < keys; k++)
                        {
                            ReadVariableLen();
                        }
                        break;
                    case LuauBytecodeTag.LBC_CONSTANT_IMPORT:
                        debug("Found import constant.");
                        //TODO: understand what the hell this is and don't skip
                        Skip(4);
                        break;
                    case LuauBytecodeTag.LBC_CONSTANT_CLOSURE:
                        //TODO: understand what the hell this is and don't skip
                        int fid = ReadVariableLen();
                        //that's it lol for now
                        break;
                    default:
                        warn("Unknown bytecode tag " + read);
                        break;
                }
            }
            p.sizep = ReadVariableLen();
            p.p = new Proto[p.sizep];
            for (int j = 0; j < p.sizep; ++j)
            {
                int read = ReadVariableLen();
                debug("Added proto " + read + " to sub proto index " + j);
                p.p[j] = protos[read];
            }
            //skip debug bullshit
            ReadVariableLen();
            ReadString(); // debug name
            //debug("Debug name: "+ReadString());
            if (file.ReadByte() == 1)
            {
                error("Line info is enabled, this is not supported!");
                return;
            }
            if (file.ReadByte() == 1)
            {
                error("Debug info is enabled, this is not supported!");
                return;
            }
            protos[i] = p;
        }
        mainprotoId = ReadVariableLen();
        mainproto = protos[mainprotoId];
        debug("Main proto is index " + mainprotoId);
        file.Close();
    }

    public void Execute()
    {
        print("Begin execution...\n");
        Proto main = protos[mainprotoId];
        executeProto(ref main);
    }

    private Instruction getNext(ref Proto use)
    {
        use.instpos += 1;
        return use.code[use.instpos];
    }

    private (object?,int) executeProto(ref Proto p)
    {
        p.instpos = -1;
        while (true)
        {
            Instruction inst = getNext(ref p);
            LuauOpcode opcode = (LuauOpcode)Luau.INSN_OP(inst);
            debug("Proto " + p.bytecodeid + " executing instruction " + opcode);
            switch (opcode)
            {
                case LuauOpcode.LOP_ADD:
                    {
                        double rg1 = (double)p.registers[Luau.INSN_B(inst)];
                        double rg2 = (double)p.registers[Luau.INSN_C(inst)];
                        p.registers[Luau.INSN_A(inst)] = rg1 + rg2;
                    }
                    break;
                case LuauOpcode.LOP_CALL:
                    {
                        uint reg = Luau.INSN_A(inst);
                        int args = ((int)Luau.INSN_B(inst)) - 1;
                        int returns = ((int)Luau.INSN_C(inst)) - 1;
                        if (p.registers[reg].GetType() == typeof(Proto))
                        {
                            debug("We're calling a proto");
                            Proto target = (Proto)p.registers[reg];
                            target.registers = new object[255];
                            switch (args)
                            {
                                case -1:
                                    {
                                        if (p.lastReturnCount == 1)
                                        {
                                            debug("Passing 1 argument from last return");
                                            target.registers[0] = p.lastReturn;
                                        }
                                        else
                                        {
                                            debug("Passing " + p.lastReturnCount + " arguments from last return");
                                            List<object> returnList = (List<object>)p.lastReturn;
                                            for (int i = 0; i < p.lastReturnCount; i++)
                                            {
                                                target.registers[i] = returnList[i];
                                            }
                                        }
                                    }
                                    break;
                                case 0:
                                    break;
                                default:
                                    {
                                        debug("Passing " + args + " arguments");
                                        for (int i = 0; i < args; i++)
                                        {
                                            target.registers[i] = p.registers[reg + i + 1];
                                        }
                                    }
                                    break;
                            }
                            (object?, int) callData = executeProto(ref target);
                            p.lastReturn = callData.Item1;
                            p.lastReturnCount = callData.Item2;
                            debug("Written lastreturn");
                            if (args != 0 && p.lastReturnCount != 0)
                            {
                                int tern = (args == -1 ? p.lastReturnCount : args);
                                if (tern == 1)
                                {
                                    debug("Updating 1 register from return");
                                    p.registers[reg] = p.lastReturn;
                                }
                                else
                                {
                                    debug("Updating " + tern + " registers from return");
                                    List<object> ret = (List<object>)p.lastReturn;
                                    for (int i = 0; i < p.lastReturnCount; i++)
                                    {
                                        if (i + 1 >= ret.Count)
                                        {
                                            p.registers[reg + i] = null;
                                        }
                                        else
                                        {
                                            p.registers[reg + i] = ret[i];
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //all non-protos are assumed to only return one thing, void or anything else.
                            debug("We're calling a non-proto");
                            if (returns != 0)
                            {
                                error("Tried calling a non-proto that did not expect 0 return values.");
                                return (null, 0);
                            }
                            Action<object[]> target = (Action<object[]>)p.registers[reg];
                            switch (args)
                            {
                                case -1:
                                    {
                                        debug("Passing arguments from last return");
                                        if (p.lastReturnCount == 0)
                                        {
                                            ((Action)p.registers[reg])();
                                        }
                                        else if (p.lastReturnCount == 1)
                                        {
                                            target(new object[1] { p.lastReturn });
                                        }
                                        else
                                        {
                                            target(((List<object>)p.lastReturn).ToArray());
                                        }
                                    }
                                    break;
                                case 0:
                                    break;
                                default:
                                    {
                                        debug("Passing " + args + " arguments");
                                        object[] passArgs = new object[args];
                                        for (int i = 0; i < args; i++)
                                        {
                                            passArgs[i] = p.registers[reg + i + 1];
                                        }
                                        target(passArgs);
                                    }
                                    break;
                            }
                        }
                        break;
                    }
                case LuauOpcode.LOP_CONCAT:
                    {
                        string output = "";
                        uint regStart = Luau.INSN_B(inst);
                        uint regEnd = Luau.INSN_C(inst);
                        for(int i = 0; i < regEnd-regStart+1; i++)
                        {
                            output += p.registers[regStart + i].ToString();
                        }
                        p.registers[Luau.INSN_A(inst)] = output;
                    }
                    break;
                case LuauOpcode.LOP_DIV:
                    {
                        double rg1 = (double)p.registers[Luau.INSN_B(inst)];
                        double rg2 = (double)p.registers[Luau.INSN_C(inst)];
                        p.registers[Luau.INSN_A(inst)] = rg1 / rg2;
                    }
                    break;
                case LuauOpcode.LOP_FORNPREP:
                    {
                        uint regId = Luau.INSN_A(inst);
                        double limit = (double)p.registers[regId];
                        double step = (double)p.registers[regId+1];
                        double idx = (double)p.registers[regId+2];
                        p.instpos += (step > 0 ? idx <= limit : limit <= idx) ? 0 : Luau.INSN_D(inst);
                        //return (null, 0);
                    }
                    break;
                case LuauOpcode.LOP_FORNLOOP:
                    {
                        uint regId = Luau.INSN_A(inst);
                        double limit = (double)p.registers[regId];
                        double step = (double)p.registers[regId + 1];
                        double idx = (double)p.registers[regId + 2] + step;
                        p.registers[regId + 2] = idx;
                        if (step > 0 ? idx <= limit : limit <= idx)
                        {
                            p.instpos += Luau.INSN_D(inst);
                        }
                    }
                    break;
                case LuauOpcode.LOP_GETGLOBAL:
                    {
                        uint aux = getNext(ref p);
                        p.registers[Luau.INSN_A(inst)] = GetGlobal((string)p.k[aux]);
                    }
                    break;
                case LuauOpcode.LOP_GETTABLE:
                    //big lmao
                    p.registers[Luau.INSN_A(inst)] = ((object[])p.registers[Luau.INSN_B(inst)])[Convert.ToInt32((double)p.registers[Luau.INSN_C(inst)]-1d)];
                    break;
                case LuauOpcode.LOP_JUMP:
                case LuauOpcode.LOP_JUMPBACK:
                    p.instpos += Luau.INSN_D(inst);
                    break;
                case LuauOpcode.LOP_JUMPIF:
                    {
                        object reg = p.registers[Luau.INSN_A(inst)];
                        if (reg != null && (reg.GetType() != typeof(bool) || (bool)reg))
                        {
                            debug("JUMPIF PASS");
                            p.instpos += Luau.INSN_D(inst);
                        }
                    }
                    break;
                case LuauOpcode.LOP_JUMPIFNOT:
                    {
                        object reg = p.registers[Luau.INSN_A(inst)];
                        if (reg == null || (reg.GetType() == typeof(bool) && !(bool)reg))
                        {
                            debug("JUMPIFNOT PASS");
                            p.instpos += Luau.INSN_D(inst);
                        }
                    }
                    break;
                case LuauOpcode.LOP_JUMPIFEQ:
                    if (Luau.EQUAL(p.registers[Luau.INSN_A(inst)],p.registers[getNext(ref p)]))
                    {
                        debug("JUMPIFEQ PASS");
                        p.instpos += Luau.INSN_D(inst) - 1;
                    }
                    break;
                case LuauOpcode.LOP_JUMPIFNOTEQ:
                    if (!Luau.EQUAL(p.registers[Luau.INSN_A(inst)], p.registers[getNext(ref p)]))
                    {
                        debug("JUMPIFNOTEQ PASS");
                        p.instpos += Luau.INSN_D(inst) - 1;
                    }
                    break;
                case LuauOpcode.LOP_JUMPIFLT:
                    if ((double)p.registers[Luau.INSN_A(inst)] < (double)p.registers[getNext(ref p)])
                    {
                        debug("JUMPIFLT PASS");
                        p.instpos += Luau.INSN_D(inst) - 1;
                    }
                    break;
                case LuauOpcode.LOP_JUMPIFNOTLT:
                    if (!((double)p.registers[Luau.INSN_A(inst)] < (double)p.registers[getNext(ref p)]))
                    {
                        debug("JUMPIFNOTLT PASS");
                        p.instpos += Luau.INSN_D(inst) - 1;
                    }
                    break;
                case LuauOpcode.LOP_LOADB:
                    p.registers[Luau.INSN_A(inst)] = Luau.INSN_B(inst)==1;
                    p.instpos += (int)Luau.INSN_C(inst);
                    break;
                case LuauOpcode.LOP_LOADK:
                    p.registers[Luau.INSN_A(inst)] = p.k[Luau.INSN_B(inst)];
                    debug("LOADK CONSTANT: " + p.k[Luau.INSN_B(inst)]);
                    break;
                case LuauOpcode.LOP_LOADN:
                    p.registers[Luau.INSN_A(inst)] = (double)Luau.INSN_D(inst);
                    break;
                case LuauOpcode.LOP_LOADNIL:
                    p.registers[Luau.INSN_A(inst)] = null;
                    break;
                case LuauOpcode.LOP_MOD:
                    {
                        double rg1 = (double)p.registers[Luau.INSN_B(inst)];
                        double rg2 = (double)p.registers[Luau.INSN_C(inst)];
                        p.registers[Luau.INSN_A(inst)] = rg1 % rg2;
                    }
                    break;
                case LuauOpcode.LOP_MOVE:
                    p.registers[Luau.INSN_A(inst)] = p.registers[Luau.INSN_B(inst)];
                    break;
                case LuauOpcode.LOP_MUL:
                    {
                        double rg1 = (double)p.registers[Luau.INSN_B(inst)];
                        double rg2 = (double)p.registers[Luau.INSN_C(inst)];
                        p.registers[Luau.INSN_A(inst)] = rg1 * rg2;
                    }
                    break;
                case LuauOpcode.LOP_NEWCLOSURE:
                    p.registers[Luau.INSN_A(inst)] = p.p[Luau.INSN_B(inst)];
                    break;
                case LuauOpcode.LOP_NEWTABLE:
                    //oversimplified as fuck, watch out
                    p.registers[Luau.INSN_A(inst)] = new object[getNext(ref p)];
                    break;
                case LuauOpcode.LOP_POW:
                    {
                        double rg1 = (double)p.registers[Luau.INSN_B(inst)];
                        double rg2 = (double)p.registers[Luau.INSN_C(inst)];
                        p.registers[Luau.INSN_A(inst)] = Math.Pow(rg1, rg2);
                    }
                    break;
                case LuauOpcode.LOP_PREPVARARGS:
                    warn("Skipped opcode LOP_PREPVARARGS");
                    break;
                case LuauOpcode.LOP_RETURN:
                    {
                        uint startReg = Luau.INSN_A(inst);
                        int returnCount = (int)Luau.INSN_B(inst) - 1;
                        switch (returnCount)
                        {
                            case 0:
                                debug("Empty return happening");
                                return (null, 0);
                            case -1:
                                debug("Return is passing variables from previous return");
                                return (p.lastReturn, p.lastReturnCount);
                            case 1:
                                return (p.registers[startReg], 1);
                        }
                        debug("Return was non-empty.");
                        List<object> rts = new List<object>();
                        for (int i = 0; i < returnCount; i++)
                        {
                            debug("Returning " + p.registers[startReg + i]);
                            rts.Add(p.registers[startReg + i]);
                        }
                        return (rts, returnCount);
                    }
                case LuauOpcode.LOP_SETGLOBAL:
                    {
                        uint aux = getNext(ref p);
                        SetGlobal((string)p.k[aux], p.registers[Luau.INSN_A(inst)]);
                    }
                    break;
                case LuauOpcode.LOP_SETLIST:
                    {
                        int valcount = (int)Luau.INSN_C(inst) - 1;
                        object[] reg = (object[])p.registers[Luau.INSN_A(inst)];
                        uint src = Luau.INSN_B(inst);
                        uint aux = getNext(ref p);
                        for (int i = 0; i < valcount; i++)
                        {
                            reg[aux + i - 1] = p.registers[src + i]; // minus one on reg write because lua is WEIRD and starts indexes on 1 and not 0
                        }
                    }
                    break;
                case LuauOpcode.LOP_SETTABLE:
                    //big lmao 2 electric boogaloo
                    ((object[])p.registers[Luau.INSN_B(inst)])[Convert.ToInt32((double)p.registers[Luau.INSN_C(inst)] - 1d)] = p.registers[Luau.INSN_A(inst)];
                    break;
                case LuauOpcode.LOP_SUB:
                    {
                        double rg1 = (double)p.registers[Luau.INSN_B(inst)];
                        double rg2 = (double)p.registers[Luau.INSN_C(inst)];
                        p.registers[Luau.INSN_A(inst)] = rg1 - rg2;
                    }
                    break;
                default:
                    error("Unsupported instruction: " + Enum.GetName(typeof(LuauOpcode), opcode));
                    return (null, 0);
            }
        }
    }
}
