#pragma warning disable CS8632

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class Luauni : MonoBehaviour
{
    public string script = "CharacterScript";
    public TextMeshProUGUI tx;

    bool printed = false;

    void Start()
    {
        if (!Globals.IsInitialized())
        {
            Logging.Debug("Initializing class functions...", "Luauni:Start");
            ClassFunctions.Init();
            Logging.Debug("Initializing globals...", "Luauni:Start");
            GC.game = new Instance(ESS.game);
            Globals.Init();
            if (!Globals.IsInitialized())
            {
                Logging.Error("Could not initialize globals, cannot proceed with execution.", "Luauni:Start");
                enabled = false;
                return;
            }
        }
        if (File.Exists(Application.streamingAssetsPath + "\\Scripts\\" + script + ".bin"))
        {
            br = new ByteReader(File.ReadAllBytes(Application.streamingAssetsPath + "\\Scripts\\" + script + ".bin"));
            Parse();
            if(ready)
                StartCoroutine(Execute());
        } else
        {
            Logging.Error("Could not load script file: " + script, "Luauni:Start");
            enabled = false;
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

    void Update()
    {
        tx.text = $"Luauni Debug\nProto {mainProtoId} Position: {iP[cEL]} / {protos[mainProtoId].sizecode}\nRecent Error: {Logging.LastError}";
        if (!ready && !printed)
        {
            Logging.Warn($"Proto {mainProtoId} emulated progress: {iP[cEL]} instructions / {protos[mainProtoId].sizecode} instructions.");
            printed = true;
        }
    }

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
            string str = br.ReadRangeStr(br.ReadVariableLen());
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

    public int cEL = 0; // current execution layer
    public List<Proto> pL = new List<Proto>(); // proto layers
    public List<int> iP = new List<int>(); // instruction position

    private uint nextInst()
    {
        iP[cEL]++;
        return pL[cEL].code[iP[cEL]];
    }

    private void jumpSteps(int steps)
    {
        iP[cEL] += steps;
    }

    public System.Collections.IEnumerator Execute()
    {
        if (!ready)
        {
            Logging.Error("No script is loaded, cannot perform step.", "Luauni:Step"); yield break;
        }
        bool should_loop = true;
        while (should_loop)
        {
            uint inst = nextInst();
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
                case LuauOpcode.LOP_AND:
                    pL[cEL].registers[Luau.INSN_A(inst)] = Luau.LIKELY(pL[cEL].registers[Luau.INSN_B(inst)]) ? pL[cEL].registers[Luau.INSN_C(inst)] : null;
                    break;
                case LuauOpcode.LOP_CALL:
                    {
                        uint reg = Luau.INSN_A(inst);
                        int args = (int)Luau.INSN_B(inst) - 1;
                        int returns = (int)Luau.INSN_C(inst) - 1;
                        object regcopy = pL[cEL].registers[reg];
                        if(regcopy == null)
                        {
                            Logging.Error($"Attempt to call a nil value", "Luauni:Step");
                            ready = false;
                            yield break;
                        }
                        Type regType = regcopy.GetType();
                        if (regType == typeof(Globals.Standard))
                        {
                            Logging.Debug("Calling a global function.", "Luauni:Step");
                            Globals.Standard target = (Globals.Standard)regcopy;
                            Proto sendProto = pL[cEL];
                            CallData send = new CallData()
                            {
                                initiator = sendProto,
                                funcRegister = reg,
                                args = args,
                                returns = returns
                            };
                            yield return target(send);
                            if (sendProto.globalErrored)
                            {
                                ready = false;
                                yield break;
                            }
                            pL[cEL] = sendProto;
                        } else if (regType == typeof(Proto))
                        {
                            Proto edit = pL[cEL]; edit.callReg = reg; edit.expectedReturns = returns; pL[cEL] = edit; // how annoying
                            Proto pr = (Proto)pL[cEL].registers[reg];
                            Logging.Debug($"Moving exeution to proto {pr.bytecodeid}, passing {args} args.", "Luauni:Step");
                            if (args == -1)
                            {
                                Logging.Debug("Argument count is -1, sending from lastReturn.", "Luauni:Step");
                                for (int i = 0; i < edit.lastReturn.Length; i++)
                                {
                                    pr.registers[i] = edit.lastReturn[i];
                                }
                            }
                            else
                            {
                                for (int i = 0; i < args; i++)
                                {
                                    pr.registers[i] = edit.registers[reg + i + 1];
                                }
                            }
                            pL.Add(pr);
                            iP.Add(-1);
                            cEL++;
                        } else
                        {
                            Logging.Warn("Unsupported function type: " + regType);
                        }
                        break;
                    }
                case LuauOpcode.LOP_CONCAT:
                    {
                        string output = "";
                        uint regStart = Luau.INSN_B(inst);
                        uint regEnd = Luau.INSN_C(inst);
                        for (int i = 0; i < regEnd - regStart + 1; i++)
                        {
                            output += pL[cEL].registers[regStart + i].ToString();
                        }
                        pL[cEL].registers[Luau.INSN_A(inst)] = output;
                    }
                    break;
                case LuauOpcode.LOP_DIV:
                    {
                        double rg1 = (double)pL[cEL].registers[Luau.INSN_B(inst)];
                        double rg2 = (double)pL[cEL].registers[Luau.INSN_C(inst)];
                        pL[cEL].registers[Luau.INSN_A(inst)] = rg1 / rg2;
                    }
                    break;
                case LuauOpcode.LOP_DUPTABLE:
                    {
                        pL[cEL].registers[Luau.INSN_A(inst)] = pL[cEL].k[Luau.INSN_D(inst)];
                        break;
                    }
                case LuauOpcode.LOP_FORGPREP:
                    {
                        int targ = (int)Luau.INSN_A(inst);
                        pL[cEL].registers[targ + 2] = 0;
                        jumpSteps(Luau.INSN_D(inst));
                    }
                    break;
                case LuauOpcode.LOP_FORGLOOP:
                    {
                        int targ = (int)Luau.INSN_A(inst);
                        int jmp = Luau.INSN_D(inst);
                        Type t = pL[cEL].registers[targ].GetType();
                        uint varcount = nextInst();
                        (bool, object[]) get;
                        if (t == typeof(TableIterator))
                        {
                            TableIterator iter = (TableIterator)pL[cEL].registers[targ];
                            get = iter.Get();
                        } else
                        {
                            ArrayIterator iter = (ArrayIterator)pL[cEL].registers[targ];
                            get = iter.Get();
                        }
                        if (get.Item1)
                        {
                            object[] array = get.Item2;
                            for (int i = 0; i < varcount; i++)
                            {
                                pL[cEL].registers[targ + 3 + i] = (i < array.Length ? array[i] : null);
                            }
                            jumpSteps(jmp - 1);
                        }
                    }
                    break;
                case LuauOpcode.LOP_FORNPREP:
                    {
                        uint regId = Luau.INSN_A(inst);
                        double limit = (double)pL[cEL].registers[regId];
                        double step = (double)pL[cEL].registers[regId + 1];
                        double idx = (double)pL[cEL].registers[regId + 2];
                        jumpSteps((step > 0 ? idx <= limit : limit <= idx) ? 0 : Luau.INSN_D(inst));
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
                        pL[cEL].registers[Luau.INSN_A(inst)] = Globals.Get(key);
                        break;
                    }
                case LuauOpcode.LOP_GETTABLE:
                    {
                        object rg = pL[cEL].registers[Luau.INSN_B(inst)];
                        if (rg.GetType() == typeof(object[]))
                        {
                            object[] arr = (object[])rg;
                            int index = Convert.ToInt32((double)pL[cEL].registers[Luau.INSN_C(inst)]) - 1;
                            pL[cEL].registers[Luau.INSN_A(inst)] = index < arr.Length && index >= 0 ? arr[index] : null;
                        } else if (rg.GetType() == typeof(Dictionary<string, object>))
                        {
                            Dictionary<string, object> arr = (Dictionary<string, object>)rg;
                            object idx = pL[cEL].registers[Luau.INSN_C(inst)];
                            if (idx.GetType() == typeof(string))
                            {
                                pL[cEL].registers[Luau.INSN_A(inst)] = arr[(string)pL[cEL].registers[Luau.INSN_C(inst)]];
                            } else
                            {
                                Logging.Error($"Attempt to index table with {idx.GetType()}", "Luauni:Step");
                                ready = false;
                                yield break;
                            }
                        } else
                        {
                            Logging.Error($"Cannot perform indexing on a {rg.GetType()}", "Luauni:Step");
                            ready = false;
                            yield break;
                        }
                        break;
                    }
                case LuauOpcode.LOP_GETTABLEKS:
                    {
                        string key = (string)pL[cEL].k[nextInst()];
                        Logging.Debug($"GETTABLEKS key: {key}", "Luauni:Step");
                        object target = pL[cEL].registers[Luau.INSN_B(inst)];
                        if(target==null)
                        {
                            Logging.Error($"Attempt to index nil with {key}", "Luauni:Step");
                            ready = false;
                            yield break;
                        }
                        Type t = target.GetType();
                        Logging.Debug(t);
                        if (t == typeof(Instance))
                        {
                            Instance from = (Instance)target;
                            Instance? ret = from.Index(key);
                            if(ret == null)
                            {
                                Logging.Error($"{key} is not a valid member of {from.src.transform.name}", "Luauni:Step");
                                ready = false;
                                yield break;
                            }
                            pL[cEL].registers[Luau.INSN_A(inst)] = (Instance)ret;
                        }
                        else
                        {
                            NamedDict nd = (NamedDict)target;
                            Dictionary<string, object> dict = nd.dict;
                            if (dict.TryGetValue(key, out object val))
                            {
                                pL[cEL].registers[Luau.INSN_A(inst)] = val;
                                Logging.Debug($"Found key: {val}", "Luauni:Step");
                            }
                            else
                            {
                                Logging.Error($"{key} is not a valid member of {nd.name}", "Luauni:Step");
                                ready = false;
                                yield break;
                            }
                        }
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
                    uint AUX = nextInst();
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
                case LuauOpcode.LOP_LENGTH:
                    {
                        object reg = pL[cEL].registers[Luau.INSN_B(inst)];
                        Type tp = reg.GetType();
                        if (tp == typeof(string))
                        {
                            pL[cEL].registers[Luau.INSN_A(inst)] = (double)((string)reg).Length;
                        }
                        else if (tp == typeof(object[]))
                        {
                            pL[cEL].registers[Luau.INSN_A(inst)] = (double)((object[])reg).Length;
                        }
                        else
                        {
                            Logging.Error($"attempt to get length of a {tp}", "Luauni:Step");
                            ready = false;
                            yield break;
                        }
                    }
                    break;
                case LuauOpcode.LOP_LOADB:
                    pL[cEL].registers[Luau.INSN_A(inst)] = Luau.INSN_B(inst)==1;
                    jumpSteps((int)Luau.INSN_C(inst));
                    break;
                case LuauOpcode.LOP_LOADK:
                    object constant = pL[cEL].k[Luau.INSN_B(inst)];
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
                case LuauOpcode.LOP_MINUS:
                    pL[cEL].registers[Luau.INSN_A(inst)] = -(double)pL[cEL].registers[Luau.INSN_B(inst)];
                    break;
                case LuauOpcode.LOP_MOD:
                    {
                        double rg1 = (double)pL[cEL].registers[Luau.INSN_B(inst)];
                        double rg2 = (double)pL[cEL].registers[Luau.INSN_C(inst)];
                        double result = 0;
                        if (double.IsInfinity(rg2))
                        {
                            result = double.NaN;
                        } else
                        {
                            result = rg1 % rg2;
                            if (rg2 < 0 && result >= 0) { result += rg2; }
                        }
                        pL[cEL].registers[Luau.INSN_A(inst)] = result;
                    }
                    break;
                case LuauOpcode.LOP_MUL:
                    {
                        double rg1 = (double)pL[cEL].registers[Luau.INSN_B(inst)];
                        double rg2 = (double)pL[cEL].registers[Luau.INSN_C(inst)];
                        pL[cEL].registers[Luau.INSN_A(inst)] = rg1 * rg2;
                    }
                    break;
                case LuauOpcode.LOP_NAMECALL:
                    {
                        string key = (string)pL[cEL].k[nextInst()];
                        Instance ins = (Instance)pL[cEL].registers[Luau.INSN_B(inst)];
                        if (ins.members.TryGetValue(key, out object val)) {
                            pL[cEL].recentNameCalledRegister = ins;
                            pL[cEL].registers[Luau.INSN_A(inst)] = (Globals.Standard)val;
                        } else
                        {
                            Logging.Error($"{key} is not a valid member of {ins.src.tag}", "Luauni:Step");
                            ready = false;
                            yield break;
                        }
                        break;
                    }
                case LuauOpcode.LOP_NEWCLOSURE:
                    pL[cEL].registers[Luau.INSN_A(inst)] = pL[cEL].p[Luau.INSN_D(inst)];
                    break;
                case LuauOpcode.LOP_NEWTABLE:
                    pL[cEL].registers[Luau.INSN_A(inst)] = new object[nextInst()];
                    break;
                case LuauOpcode.LOP_NOT:
                    pL[cEL].registers[Luau.INSN_A(inst)] = !Luau.LIKELY(pL[cEL].registers[Luau.INSN_B(inst)]);
                    break;
                case LuauOpcode.LOP_OR:
                    pL[cEL].registers[Luau.INSN_A(inst)] = Luau.LIKELY(pL[cEL].registers[Luau.INSN_B(inst)]) ? pL[cEL].registers[Luau.INSN_B(inst)] : pL[cEL].registers[Luau.INSN_C(inst)];
                    break;
                case LuauOpcode.LOP_POW:
                    {
                        double rg1 = (double)pL[cEL].registers[Luau.INSN_B(inst)];
                        double rg2 = (double)pL[cEL].registers[Luau.INSN_C(inst)];
                        pL[cEL].registers[Luau.INSN_A(inst)] = Math.Pow(rg1, rg2);
                    }
                    break;
                case LuauOpcode.LOP_RETURN:
                    if (pL.Count == 1)
                    {
                        Logging.Print("Main proto execution finished.", "Luauni:Step");
                        ready = false;
                        yield break;
                    } else
                    {
                        Logging.Debug($"Proto {pL[cEL].bytecodeid} is returning.", "Luauni:Step");
                        uint begin = Luau.INSN_A(inst);
                        int returns = (int)Luau.INSN_B(inst) - 1;
                        bool returnwhatever = false;
                        if(returns == -1)
                        {
                            returnwhatever = true;
                            returns = pL[cEL].lastReturn.Length;
                        }
                        Logging.Debug($"Returning {returns} values.", "Luauni:Step");
                        uint startReg = pL[cEL - 1].callReg;
                        int expects = pL[cEL - 1].expectedReturns;
                        int tern = expects == -1 ? returns : expects;
                        Proto edit = pL[cEL - 1];
                        edit.lastReturn = new object[tern];
                        for (int i = 0; i < tern; i++)
                        {
                            object sendback;
                            if (returnwhatever)
                            {
                                sendback = i < returns ? pL[cEL].lastReturn[i] : null;
                            } else
                            {
                                sendback = i < returns ? pL[cEL].registers[begin + i] : null;
                            }
                            pL[cEL - 1].registers[startReg + i] = sendback;
                            edit.lastReturn[i] = sendback;
                        }
                        pL[cEL - 1] = edit;
                        pL.RemoveAt(cEL);
                        iP.RemoveAt(cEL);
                        cEL--;
                    }
                    break;
                case LuauOpcode.LOP_SETGLOBAL:
                    {
                        Globals.Set((string)pL[cEL].k[nextInst()], pL[cEL].registers[Luau.INSN_A(inst)]);
                    }
                    break;
                case LuauOpcode.LOP_SETLIST:
                    {
                        int valcount = (int)Luau.INSN_C(inst) - 1;
                        object[] reg = (object[])pL[cEL].registers[Luau.INSN_A(inst)];
                        uint src = Luau.INSN_B(inst);
                        uint aux = nextInst();
                        for (int i = 0; i < valcount; i++)
                        {
                            reg[aux + i - 1] = pL[cEL].registers[src + i];
                        }
                    }
                    break;
                case LuauOpcode.LOP_SETTABLE:
                    {
                        object idx = pL[cEL].registers[Luau.INSN_C(inst)];
                        Type t = idx.GetType();
                        Logging.Debug(t);
                        if (t == typeof(string))
                        {
                            ((Dictionary<string, object>)pL[cEL].registers[Luau.INSN_B(inst)])[(string)idx] = pL[cEL].registers[Luau.INSN_A(inst)];
                        }
                        else
                        {
                            int index = Convert.ToInt32((double)idx) - 1;
                            object[] src = (object[])pL[cEL].registers[Luau.INSN_B(inst)];
                            int len = src.Length;
                            if (len < index + 1) {
                                object[] nw = new object[index + 1];
                                Array.Copy(src, 0, nw, 0, len);
                                pL[cEL].registers[Luau.INSN_B(inst)] = nw;
                            }
                            ((object[])pL[cEL].registers[Luau.INSN_B(inst)])[index] = pL[cEL].registers[Luau.INSN_A(inst)];
                        }
                    }
                    break;
                case LuauOpcode.LOP_SETTABLEKS:
                    {
                        string key = (string)pL[cEL].k[nextInst()];
                        Logging.Debug($"SETTABLEKS key: {key}", "Luauni:Step");
                        object target = pL[cEL].registers[Luau.INSN_B(inst)];
                        if (target == null)
                        {
                            Logging.Error($"Attempt to index nil with {key}", "Luauni:Step");
                            ready = false;
                            yield break;
                        }
                        Type t = target.GetType();
                        Logging.Debug(t);
                        if (t == typeof(Instance))
                        {
                            Instance ins = (Instance)target;
                            if (ins.members.TryGetValue(key, out object val))
                            {
                                ins[key] = pL[cEL].registers[Luau.INSN_A(inst)];
                                Logging.Debug($"Found key: {val}", "Luauni:Step");
                            }
                            else
                            {
                                Logging.Error($"{key} is not a valid member of {ins.src.name}", "Luauni:Step");
                                ready = false;
                                yield break;
                            }
                        }
                        else if (t == typeof(NamedDict))
                        {
                            NamedDict nd = (NamedDict)target;
                            if (nd.dict.TryGetValue(key, out object val))
                            {
                                nd.dict[key] = pL[cEL].registers[Luau.INSN_A(inst)];
                            }
                            else
                            {
                                nd.dict.Add(key, pL[cEL].registers[Luau.INSN_A(inst)]);
                            }
                        } else
                        {
                            Logging.Error($"Cannot apply indexing on register type {t}", "Luauni:Step");
                            ready = false;
                            yield break;
                        }
                        break;
                    }
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
                    yield break;
            }
        }
        yield break;
    }
}
