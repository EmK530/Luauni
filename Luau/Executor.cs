#pragma warning disable CS8602

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

public class Luauni
{
    public Luauni(string path)
    {
        if (!Globals.IsInitialized())
        {
            Logging.Debug("Initializing globals...", "Luauni:Constructor");
            Globals.Init();
            if (!Globals.IsInitialized())
            {
                Logging.Error("Could not initialize globals, cannot proceed with execution.", "Luauni:Constructor");
                return;
            }
        }
        if (File.Exists(path))
        {
            br = new ByteReader(File.ReadAllBytes(path));
            Parse();
        } else
        {
            Logging.Error("Could not load file path: " + path, "Luauni:Constructor");
        }
    }

    public bool IsReady()
    {
        return ready;
    }

    private ByteReader? br;
    private bool ready = false;
    private string[]? stringtable;
    private Proto[]? protos;

    private int mainProtoId;
    private Proto mainProto;

    private void Parse()
    {
        if (br == null)
        {
            Logging.Error("ByteReader is null, cannot parse.", "Luauni:Parse");
            return;
        }
        Logging.Print("Parsing bytecode...", "Luauni:Parse");

        // version checks

        byte ver = br.ReadByte();
        byte varver = br.ReadByte();
        Logging.Debug($"Luau version: {ver}", "Luauni:Parse");
        Logging.Debug($"Type version: {varver}", "Luauni:Parse");
        if(ver!=5)
            Logging.Warn("Lua version mismatch! Expect issues!", "Luauni:Parse");
        if(varver!=1)
            Logging.Warn("Type version mismatch! Expect issues!", "Luauni:Parse");

        // build string table

        int strings = br.ReadVariableLen();
        stringtable = new string[strings];
        Logging.Debug($"String table size: {strings}", "Luauni:Parse");
        for(int i = 0; i < strings; i++)
        {
            string str = br.ReadRangeStr(br.ReadByte());
            stringtable[i] = str;
            Logging.Debug($"String table entry #{i+1}: {str}", "Luauni:Parse");
        }

        // read and init protos

        int protoCount = br.ReadVariableLen();
        protos = new Proto[protoCount];
        Logging.Debug($"Script proto count: {protoCount}", "Luauni:Parse");
        for(int i = 0; i < protoCount; i++)
        {
            Proto p = ParseEssentials.PrepareProto(br, i, stringtable);
            for (int j = 0; j < p.sizep; ++j)
            {
                int read = br.ReadVariableLen();
                Logging.Debug($"Added proto {read} to sub proto index {j}", "Luauni:Parse");
                p.p[j] = protos[read];
            }
            // skip debug
            br.ReadVariableLen(); br.ReadVariableLen();
            if (br.ReadByte() == 1)
                Logging.Warn("Line info is enabled, this is unexpected.");
            if (br.ReadByte() == 1)
                Logging.Warn("Debug info is enabled, this is unexpected.");
            protos[i] = p;
        }

        // finalize

        mainProtoId = br.ReadVariableLen();
        mainProto = protos[mainProtoId];
        Logging.Debug($"Main proto ID: {mainProtoId}", "Luauni:Parse");

        pL.Add(mainProto);
        ready = true;
        Logging.Print("Bytecode loaded, ready for execution.", "Luauni:Parse");
    }

    private int cEL = 0; // current execution layer
    private List<Proto> pL = new List<Proto>(); // proto layers

    private Instruction nextInst()
    {
        pL[cEL].code_iter.MoveNext();
        return pL[cEL].code_iter.Current;
    }

    public void Step()
    {
        if (!ready)
        {
            Logging.Error("No script is loaded, cannot perform step.", "Luauni:Step"); return;
        }
        bool should_loop = true;
        while (should_loop)
        {
            Instruction inst = nextInst();
            LuauOpcode opcode = (LuauOpcode)Luau.INSN_OP(inst);
            Logging.Debug($"Proto {pL[cEL].bytecodeid} executing opcode {opcode}", "Luauni:Step");
            switch (opcode)
            {
                case LuauOpcode.LOP_NOP:
                case LuauOpcode.LOP_BREAK:
                case LuauOpcode.LOP_PREPVARARGS:
                case LuauOpcode.LOP_GETVARARGS:
                    Logging.Warn($"Ignoring opcode not planned to support: {opcode}", "Luauni:Step");
                    break;
                case LuauOpcode.LOP_GETGLOBAL:
                    pL[cEL].registers[Luau.INSN_A(inst)] = Globals.Get((string)pL[cEL].k[nextInst()]);
                    break;
                case LuauOpcode.LOP_NEWCLOSURE:
                    pL[cEL].registers[Luau.INSN_A(inst)] = pL[cEL].p[Luau.INSN_D(inst)];
                    break;
                default:
                    Logging.Error($"Unsupported opcode: {opcode}", "Luauni:Step");
                    return;
            }
            if (opcode == LuauOpcode.LOP_RETURN)
            {
                break;
            }
        }
    }
}