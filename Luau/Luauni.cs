#pragma warning disable CS8632

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Diagnostics.Eventing.Reader;
using static UnityEngine.GraphicsBuffer;

public class Luauni : MonoBehaviour
{
    public string targetScript = "EngineScript";
    public TextMeshProUGUI tx;

    bool printed = false;

    string[] validExecutionSpots = new string[] {"LocalPlayer","Backpack","PlayerGui","PlayerScripts"};

    BindingFlags search = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

    void Start()
    {
        if (!Globals.IsInitialized())
        {
            Logging.Debug("Initializing globals...", "Luauni:Start");
            Globals.Init();
            if (!Globals.IsInitialized())
            {
                Logging.Error("Could not initialize globals, cannot proceed with execution.", "Luauni:Start");
                enabled = false;
                return;
            }
        }

        if (File.Exists(Application.streamingAssetsPath + "\\Scripts\\" + targetScript + ".bin"))
        {
            br = new ByteReader(File.ReadAllBytes(Application.streamingAssetsPath + "\\Scripts\\" + targetScript + ".bin"));
            Parse();
            if (ready)
                if (tag == "LocalScript" && Array.IndexOf(validExecutionSpots, transform.parent.tag) >= 0)
                {
                    Logging.Debug(targetScript + " will execute.");
                    StartCoroutine(Execute());
                }
        } else
        {
            Logging.Error("Could not load script file: " + targetScript, "Luauni:Start");
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
        if (targetScript == "EngineScript")
        {
            tx.text = $"Luauni Debug\nProto {mainProtoId} Position: {iP[0] + 1} / {protos[mainProtoId].sizecode}\nRecent Error: {Logging.LastError}";
            if (!ready && !printed)
            {
                Logging.Warn($"Proto {mainProtoId} emulated progress: {iP[0] + 1} instructions / {protos[mainProtoId].sizecode} instructions.");
                printed = true;
            }
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
        cL.Add(null);
        iP.Add(-1);
        ready = true;
        Logging.Print("Bytecode loaded for "+targetScript+", ready for execution.", "Luauni:Parse");
    }

    [HideInInspector] public int cEL = 0; // current execution layer
    public List<Proto> pL = new List<Proto>(); // proto layers
    public List<Closure?> cL = new List<Closure?>(); // closure layers
    [HideInInspector] public List<int> iP = new List<int>(); // instruction position
    public Closure lCC;

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
                case LuauOpcode.LOP_CLOSEUPVALS:
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
                            Logging.Debug("Calling a standard function.", "Luauni:Step");
                            Globals.Standard target = (Globals.Standard)regcopy;
                            Proto sendProto = pL[cEL];
                            CallData send = new CallData()
                            {
                                initiator = sendProto,
                                funcRegister = reg,
                                args = args,
                                returns = returns
                            };
                            yield return target.Invoke(send);
                            if (sendProto.globalErrored)
                            {
                                ready = false;
                                yield break;
                            }
                            pL[cEL] = sendProto;
                        } else if (regType == typeof(Closure))
                        {

                            Proto edit = pL[cEL]; edit.callReg = reg; edit.expectedReturns = returns; pL[cEL] = edit; // how annoying
                            Closure cl = (Closure)pL[cEL].registers[reg];
                            Proto pr = ((Closure)pL[cEL].registers[reg]).p;
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
                            cL.Add(cl);
                            cEL++;
                        } else
                        {
                            Logging.Warn("Unsupported function type: " + regType);
                        }
                        break;
                    }
                case LuauOpcode.LOP_CAPTURE:
                    {
                        LuauCaptureType cap = (LuauCaptureType)Luau.INSN_A(inst);
                        if(lCC.loadedUps >= lCC.p.nups)
                        {
                            Logging.Error($"Cannot CAPTURE upvalue because nups limit has been exceeded for the proto.", "Luauni:Step"); ready = false; yield break;
                        }
                        Logging.Debug($"Capturing upvalue of type {cap}", "Luauni:Step");
                        if(cap == LuauCaptureType.LCT_VAL)
                        {
                            lCC.upvals[lCC.loadedUps] = pL[cEL].registers[Luau.INSN_B(inst)];
                        } else
                        {
                            lCC.upvals[lCC.loadedUps] = new UpvalREF()
                            {
                                src = pL[cEL],
                                register = Luau.INSN_B(inst)
                            };
                        }
                        lCC.loadedUps++;
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
                        Type t = rg.GetType();
                        if (t == typeof(object[]))
                        {
                            object[] arr = (object[])rg;
                            int index = Convert.ToInt32((double)pL[cEL].registers[Luau.INSN_C(inst)]) - 1;
                            pL[cEL].registers[Luau.INSN_A(inst)] = index < arr.Length && index >= 0 ? arr[index] : null;
                        }
                        else if (t == typeof(Dictionary<string, object>))
                        {
                            Dictionary<string, object> arr = (Dictionary<string, object>)rg;
                            object idx = pL[cEL].registers[Luau.INSN_C(inst)];
                            if (idx.GetType() == typeof(string))
                            {
                                pL[cEL].registers[Luau.INSN_A(inst)] = arr[(string)pL[cEL].registers[Luau.INSN_C(inst)]];
                            }
                            else
                            {
                                Logging.Error($"Attempt to index table with {idx.GetType()}", "Luauni:Step"); ready = false; yield break;
                            }
                        }
                        else if (t == typeof(NamedDict))
                        {
                            NamedDict nd = (NamedDict)rg;
                            string key = (string)pL[cEL].registers[Luau.INSN_C(inst)];
                            Dictionary<string, object> dict = nd.dict;
                            if (dict.TryGetValue(key, out object val))
                            {
                                if (val.GetType() == typeof(Dictionary<string, object>))
                                {
                                    val = new NamedDict()
                                    {
                                        name = key,
                                        dict = (Dictionary<string, object>)val
                                    };
                                }
                                pL[cEL].registers[Luau.INSN_A(inst)] = val;
                                Logging.Debug($"Found key: {val}", "Luauni:Step");
                            }
                            else
                            {
                                Logging.Error($"{key} is not a valid member of {nd.name}", "Luauni:Step"); ready = false; yield break;
                            }
                        } else {
                            Logging.Error($"Cannot perform indexing on a {rg.GetType()}", "Luauni:Step"); ready = false; yield break;
                        }
                        break;
                    }
                case LuauOpcode.LOP_GETTABLEKS:
                    {
                        string key = (string)pL[cEL].k[nextInst()];
                        Logging.Debug($"GETTABLEKS key: {key}", "Luauni:Step");
                        object target = pL[cEL].registers[Luau.INSN_B(inst)];
                        Logging.Debug(target);
                        if(target==null)
                        {
                            Logging.Error($"Attempt to index nil with {key}", "Luauni:Step"); ready = false; yield break;
                        }
                        Type t = Misc.SafeType(target);
                        if(t == typeof(NamedDict))
                        {
                            NamedDict nd = (NamedDict)target;
                            Dictionary<string, object> dict = nd.dict;
                            if (dict.TryGetValue(key, out object val))
                            {
                                if (val.GetType() == typeof(Dictionary<string, object>))
                                {
                                    val = new NamedDict()
                                    {
                                        name = key,
                                        dict = (Dictionary<string, object>)val
                                    };
                                }
                                pL[cEL].registers[Luau.INSN_A(inst)] = val;
                                Logging.Debug($"Found key: {val}", "Luauni:Step");
                            }
                            else
                            {
                                Logging.Error($"{key} is not a valid member of {nd.name}", "Luauni:Step"); ready = false; yield break;
                            }
                        }
                        else if(t == typeof(GameObject))
                        {
                            GameObject obj = (GameObject)target;
                            Transform find = obj.transform.Find(key);
                            if (find != null)
                            {
                                pL[cEL].registers[Luau.INSN_A(inst)] = Misc.TryGetType(find);
                            }
                            else
                            {
                                Logging.Error($"{key} is not a valid member of {t.Name}", "Luauni:Step"); ready = false; yield break;
                            }
                        }
                        else 
                        {
                            FieldInfo test = t.GetField(key, search);
                            Type test2 = t.GetNestedType(key, search);
                            MethodInfo test3 = t.GetMethod(key);
                            PropertyInfo test4 = t.GetProperty(key, search);
                            if (test != null)
                            {
                                object send = test.GetValue(target);
                                pL[cEL].registers[Luau.INSN_A(inst)] = send;
                            } else if(test2 != null)
                            {
                                pL[cEL].registers[Luau.INSN_A(inst)] = test2;
                            } else if(test3 != null)
                            {
                                pL[cEL].registers[Luau.INSN_A(inst)] = (Globals.Standard)Delegate.CreateDelegate(typeof(Globals.Standard), test3.IsStatic ? null : target, test3);
                            } else if(test4 != null)
                            {
                                object send = test4.GetValue(target);
                                pL[cEL].registers[Luau.INSN_A(inst)] = send;
                            }
                            else
                            {
                                object temp = null;
                                if (temp == null) { temp = t.GetNestedType(key, search); }
                                if (temp == null)
                                {
                                    FieldInfo isObject = t.GetField("isObject");
                                    if (isObject == null)
                                    {
                                        Logging.Error($"Internal error: isObject not part of class {t.Name}", "Luauni:Step"); ready = false; yield break;
                                    }
                                    bool indexable = (bool)isObject.GetValue(t);
                                    if (indexable)
                                    {
                                        FieldInfo f1 = t.GetField("source", search);
                                        GameObject obj;
                                        if(f1 != null)
                                        {
                                            obj = (GameObject)(f1.GetValue(target));
                                        } else
                                        {
                                            obj = ((Component)target).gameObject;
                                        }
                                        Transform find = obj.transform.Find(key);
                                        if (find != null)
                                        {
                                            pL[cEL].registers[Luau.INSN_A(inst)] = Misc.TryGetType(find);
                                        }
                                        else
                                        {
                                            Logging.Error($"{key} is not a valid member of {t.Name}", "Luauni:Step"); ready = false; yield break;
                                        }
                                    }
                                    else
                                    {
                                        Logging.Error($"{key} is not a valid member of {t.Name}", "Luauni:Step"); ready = false; yield break;
                                    }
                                }
                            }
                        }
                        break;
                    }
                case LuauOpcode.LOP_GETUPVAL:
                    {
                        uint idx = Luau.INSN_B(inst);
                        Closure cl = cL[cEL];
                        if(cl.loadedUps <= idx)
                        {
                            Logging.Error($"Cannot GETUPVAL because index is outside the range of loaded upvalues.", "Luauni:Step");
                            ready = false;
                            yield break;
                        }
                        object upvalue = cl.upvals[idx];
                        if(upvalue.GetType() == typeof(UpvalREF))
                        {
                            UpvalREF refer = (UpvalREF)upvalue;
                            pL[cEL].registers[Luau.INSN_A(inst)] = refer.src.registers[refer.register];
                        } else
                        {
                            pL[cEL].registers[Luau.INSN_A(inst)] = upvalue;
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
                            Logging.Error($"attempt to get length of a {tp}", "Luauni:Step"); ready = false; yield break;
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
                        object reg = pL[cEL].registers[Luau.INSN_B(inst)];
                        Logging.Debug(reg);
                        Type t = Misc.SafeType(reg);
                        MethodInfo get2 = t.GetMethod(key);
                        if (get2 != null)
                        {
                            pL[cEL].recentNameCalledRegister = pL[cEL].registers[Luau.INSN_B(inst)];
                            pL[cEL].registers[Luau.INSN_A(inst)] = (Globals.Standard)Delegate.CreateDelegate(typeof(Globals.Standard), get2.IsStatic ? null : reg, get2); ;
                        }
                        else
                        {
                            MethodInfo get = Type.GetType("InheritedByAll").GetMethod(key);
                            if (get != null)
                            {
                                pL[cEL].recentNameCalledRegister = pL[cEL].registers[Luau.INSN_B(inst)];
                                pL[cEL].registers[Luau.INSN_A(inst)] = (Globals.Standard)Delegate.CreateDelegate(typeof(Globals.Standard), get.IsStatic ? null : reg, get); ;
                            }
                            else
                            {
                                Logging.Error($"{key} is not a valid member of {reg}", "Luauni:Step"); ready = false; yield break;
                            }
                        }
                        break;
                    }
                case LuauOpcode.LOP_NEWCLOSURE:
                    lCC = new Closure() {
                        p = pL[cEL].p[Luau.INSN_D(inst)],
                        upvals = new object[pL[cEL].p[Luau.INSN_D(inst)].nups]
                    };
                    pL[cEL].registers[Luau.INSN_A(inst)] = lCC;
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
                        Logging.Print("Main proto execution finished.", "Luauni:Step"); ready = false; yield break;
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
                        cL.RemoveAt(cEL);
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
                            Logging.Error($"Attempt to index nil with {key}", "Luauni:Step"); ready = false; yield break;
                        }
                        Type t = Misc.SafeType(target);
                        Logging.Debug(t);
                        Logging.Debug(target);
                        if (t == typeof(NamedDict))
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
                            PropertyInfo p = t.GetProperty(key, search);
                            if (p != null)
                            {
                                p.SetValue(target, pL[cEL].registers[Luau.INSN_A(inst)]);
                            }
                            else
                            {
                                FieldInfo f = t.GetField(key, search);
                                if (f != null)
                                {
                                    f.SetValue(target, pL[cEL].registers[Luau.INSN_A(inst)]);
                                }
                                else
                                {
                                    Logging.Error($"{key} is not a valid member of {t}", "Luauni:Step"); ready = false; yield break;
                                }
                            }
                        }
                        break;
                    }
                case LuauOpcode.LOP_SETUPVAL:
                    {
                        uint idx = Luau.INSN_B(inst);
                        Closure cl = cL[cEL];
                        if (cl.loadedUps <= idx)
                        {
                            Logging.Error($"Cannot SETUPVAL because index is outside the range of loaded upvalues.", "Luauni:Step"); ready = false; yield break;
                        }
                        object upvalue = cl.upvals[idx];
                        if (upvalue.GetType() == typeof(UpvalREF))
                        {
                            UpvalREF refer = (UpvalREF)upvalue;
                            refer.src.registers[refer.register] = pL[cEL].registers[Luau.INSN_A(inst)];
                        }
                        else
                        {
                            cl.upvals[idx] = pL[cEL].registers[Luau.INSN_A(inst)];
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
                    Logging.Error($"Unsupported opcode: {opcode}", "Luauni:Step"); ready = false; yield break;
            }
        }
        yield break;
    }
}
