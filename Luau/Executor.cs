#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CS8605

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            // skip Logging.Debug
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
        iP.Add(-1);
        ready = true;
        Logging.Print("Bytecode loaded, ready for execution.", "Luauni:Parse");
    }

    private int cEL = 0; // current execution layer
    private List<Proto> pL = new List<Proto>(); // proto layers
    private List<int> iP = new List<int>(); // instruction position

    private Instruction nextInst()
    {
        iP[cEL]++;
        return pL[cEL].code[iP[cEL]];
    }

    private void jumpSteps(int steps)
    {
        iP[cEL] += steps;
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
                case LuauOpcode.LOP_ADD:
                    {
                        double rg1 = (double)pL[cEL].registers[Luau.INSN_B(inst)];
                        double rg2 = (double)pL[cEL].registers[Luau.INSN_C(inst)];
                        pL[cEL].registers[Luau.INSN_A(inst)] = rg1 + rg2;
                    }
                    break;
                case LuauOpcode.LOP_CALL:
                    {
                        Logging.Debug("Performing a function call.", "Luauni:Step");
                        uint reg = Luau.INSN_A(inst);
                        Type regType = pL[cEL].registers[reg].GetType();
                        Logging.Debug(regType, "Luauni:Step");
                        Logging.Debug(Luau.INSN_B(inst) - 1, "Luauni:Step");
                        Logging.Debug(Luau.INSN_C(inst) - 1, "Luauni:Step");
                        if (regType == typeof(Globals.Standard))
                        {
                            Logging.Debug("Calling a global function.", "Luauni:Step");
                            Globals.Standard target = (Globals.Standard)pL[cEL].registers[reg];
                            Proto sendProto = pL[cEL];
                            CallData send = new CallData()
                            {
                                initiator = ref sendProto,
                                funcRegister = reg,
                                args = (int)Luau.INSN_B(inst) - 1,
                                returns = (int)Luau.INSN_C(inst) - 1
                            };
                            target(ref send);
                            pL[cEL] = sendProto;
                        } else
                        {
                            Logging.Warn("Unsupported function type: " + regType);
                        }
                        break;
                    }
                case LuauOpcode.LOP_DIV:
                    {
                        double rg1 = (double)pL[cEL].registers[Luau.INSN_B(inst)];
                        double rg2 = (double)pL[cEL].registers[Luau.INSN_C(inst)];
                        pL[cEL].registers[Luau.INSN_A(inst)] = rg1 / rg2;
                    }
                    break;
                case LuauOpcode.LOP_FORNPREP:
                    {
                        uint regId = Luau.INSN_A(inst);
                        double limit = (double)pL[cEL].registers[regId];
                        double step = (double)pL[cEL].registers[regId + 1];
                        double idx = (double)pL[cEL].registers[regId + 2];
                        jumpSteps((step > 0 ? idx <= limit : limit <= idx) ? 0 : Luau.INSN_D(inst));
                        //return (null, 0);
                    }
                    break;
                case LuauOpcode.LOP_FORNLOOP:
                    {
                        uint regId = Luau.INSN_A(inst);
                        double limit = (double)pL[cEL].registers[regId];
                        double step = (double)pL[cEL].registers[regId + 1];
                        double idx = (double)pL[cEL].registers[regId + 2] + step;
                        pL[cEL].registers[regId + 2] = idx;
                        if (step > 0 ? idx <= limit : limit <= idx)
                        {
                            jumpSteps(Luau.INSN_D(inst));
                        }
                    }
                    break;
                case LuauOpcode.LOP_GETGLOBAL:
                    {
                        string key = (string)pL[cEL].k[nextInst()];
                        Logging.Debug("GETGLOBAL KEY: " + key, "Luauni:Step");
                        pL[cEL].registers[Luau.INSN_A(inst)] = Globals.Get(key);
                        break;
                    }
                case LuauOpcode.LOP_GETTABLEKS:
                    {
                        string key = (string)pL[cEL].k[nextInst()];
                        Logging.Debug("GETTABLEKS KEY: " + key, "Luauni:Step");
                        pL[cEL].registers[Luau.INSN_A(inst)] = ((Dictionary<string, object>)pL[cEL].registers[Luau.INSN_B(inst)])[key];
                        break;
                    }
                case LuauOpcode.LOP_JUMP:
                case LuauOpcode.LOP_JUMPBACK:
                    jumpSteps(Luau.INSN_D(inst));
                    break;
                case LuauOpcode.LOP_JUMPIF:
                    {
                        object reg = pL[cEL].registers[Luau.INSN_A(inst)];
                        if (reg != null && (reg.GetType() != typeof(bool) || (bool)reg))
                        {
                            Logging.Debug("JUMPIF PASS");
                            jumpSteps(Luau.INSN_D(inst));
                        }
                    }
                    break;
                case LuauOpcode.LOP_JUMPIFNOT:
                    {
                        object reg = pL[cEL].registers[Luau.INSN_A(inst)];
                        if (reg == null || (reg.GetType() == typeof(bool) && !(bool)reg))
                        {
                            Logging.Debug("JUMPIFNOT PASS");
                            jumpSteps(Luau.INSN_D(inst));
                        }
                    }
                    break;
                case LuauOpcode.LOP_JUMPIFEQ:
                    Instruction AUX = nextInst();
                    if (Luau.EQUAL(pL[cEL].registers[Luau.INSN_A(inst)], pL[cEL].registers[AUX]))
                    {
                        Logging.Debug("JUMPIFEQ PASS");
                        jumpSteps(Luau.INSN_D(inst) - 1);
                    }
                    break;
                case LuauOpcode.LOP_JUMPIFNOTEQ:
                    if (!Luau.EQUAL(pL[cEL].registers[Luau.INSN_A(inst)], pL[cEL].registers[nextInst()]))
                    {
                        Logging.Debug("JUMPIFNOTEQ PASS");
                        jumpSteps(Luau.INSN_D(inst) - 1);
                    }
                    break;
                case LuauOpcode.LOP_JUMPIFLT:
                    if ((double)pL[cEL].registers[Luau.INSN_A(inst)] < (double)pL[cEL].registers[nextInst()])
                    {
                        Logging.Debug("JUMPIFLT PASS");
                        jumpSteps(Luau.INSN_D(inst) - 1);
                    }
                    break;
                case LuauOpcode.LOP_JUMPIFNOTLT:
                    if (!((double)pL[cEL].registers[Luau.INSN_A(inst)] < (double)pL[cEL].registers[nextInst()]))
                    {
                        Logging.Debug("JUMPIFNOTLT PASS");
                        jumpSteps(Luau.INSN_D(inst) - 1);
                    }
                    break;
                case LuauOpcode.LOP_LOADB:
                    pL[cEL].registers[Luau.INSN_A(inst)] = Luau.INSN_B(inst)==1;
                    jumpSteps((int)Luau.INSN_C(inst));
                    break;
                case LuauOpcode.LOP_LOADK:
                    object constant = pL[cEL].k[Luau.INSN_B(inst)];
                    Logging.Debug("LOADK CONSTANT: " + constant, "Luauni:Step");
                    pL[cEL].registers[Luau.INSN_A(inst)] = constant;
                    break;
                case LuauOpcode.LOP_LOADN:
                    pL[cEL].registers[Luau.INSN_A(inst)] = (double)Luau.INSN_D(inst);
                    break;
                case LuauOpcode.LOP_LOADNIL:
                    pL[cEL].registers[Luau.INSN_A(inst)] = null;
                    break;
                case LuauOpcode.LOP_MOVE:
                    pL[cEL].registers[Luau.INSN_A(inst)] = pL[cEL].registers[Luau.INSN_B(inst)];
                    break;
                case LuauOpcode.LOP_MUL:
                    {
                        double rg1 = (double)pL[cEL].registers[Luau.INSN_B(inst)];
                        double rg2 = (double)pL[cEL].registers[Luau.INSN_C(inst)];
                        pL[cEL].registers[Luau.INSN_A(inst)] = rg1 * rg2;
                    }
                    break;
                case LuauOpcode.LOP_MOD:
                    {
                        double rg1 = (double)pL[cEL].registers[Luau.INSN_B(inst)];
                        double rg2 = (double)pL[cEL].registers[Luau.INSN_C(inst)];
                        pL[cEL].registers[Luau.INSN_A(inst)] = rg1 % rg2;
                    }
                    break;
                case LuauOpcode.LOP_NEWCLOSURE:
                    pL[cEL].registers[Luau.INSN_A(inst)] = pL[cEL].p[Luau.INSN_D(inst)];
                    break;
                case LuauOpcode.LOP_POW:
                    {
                        double rg1 = (double)pL[cEL].registers[Luau.INSN_B(inst)];
                        double rg2 = (double)pL[cEL].registers[Luau.INSN_C(inst)];
                        pL[cEL].registers[Luau.INSN_A(inst)] = Math.Pow(rg1, rg2);
                    }
                    break;
                case LuauOpcode.LOP_RETURN:
                    should_loop = false;
                    pL.RemoveAt(cEL);
                    cEL--;
                    if (pL.Count == 0)
                    {
                        Logging.Print("Proto execution finished.", "Luauni:Step");
                        ready = false;
                        return;
                    }
                    break;
                case LuauOpcode.LOP_SUB:
                    {
                        double rg1 = (double)pL[cEL].registers[Luau.INSN_B(inst)];
                        double rg2 = (double)pL[cEL].registers[Luau.INSN_C(inst)];
                        pL[cEL].registers[Luau.INSN_A(inst)] = rg1 - rg2;
                    }
                    break;
                default:
                    Logging.Error($"Unsupported opcode: {opcode}", "Luauni:Step");
                    ready = false;
                    return;
            }
        }
    }
}