#pragma warning disable CS8632

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using System.Diagnostics;
using System.Linq;

public struct LuauniConfig
{
    public bool debugGlobals;
    public bool yieldPerInstruction;
}

public class Luauni : MonoBehaviour
{
    public string targetScript = "EngineScript";

    bool printed = false;

    static BindingFlags search = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

    private static bool[] config = new bool[]
    {
        false
    };

    void Start()
    {
        Dictionary<string, object> cfg = new Dictionary<string, object>()
        {
            ["yieldPerInstruction"] = false,
            ["logEmulationDebug"] = false,
            ["allowSystemService"] = false
        };
        if (!File.Exists("config.json"))
        {
            File.WriteAllText("config.json",JsonConvert.SerializeObject(cfg, Formatting.Indented));
        } else
        {
            Dictionary<string, object> cfg2 = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText("config.json"));
            foreach(string i in cfg.Keys.ToArray<string>())
            {
                object value = null;
                if (!cfg2.TryGetValue(i, out value))
                {
                    value = cfg[i];
                } else
                {
                    cfg[i] = value;
                }
                if (value != null)
                {
                    switch (i)
                    {
                        case "yieldPerInstruction":
                            config[0] = (bool)value;
                            break;
                        case "logEmulationDebug":
                            Logging.ShowDebug = (bool)value;
                            break;
                        case "allowSystemService":
                            if ((bool)value)
                            {
                                SystemService.Windows = new WindowsFunctions();
                            }
                            break;
                    }
                }
            }
            File.WriteAllText("config.json", JsonConvert.SerializeObject(cfg, Formatting.Indented));
        }

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

        if (File.Exists(Application.streamingAssetsPath + "\\Scripts\\" + targetScript + ".lua"))
        {
            if(File.Exists(Application.streamingAssetsPath + "\\luau-compile.exe"))
            {
                Logging.Print($"Compiling {Application.streamingAssetsPath}\\Scripts\\{targetScript}.lua");
                Process compile = new Process()
                {
                    StartInfo =
                    {
                        FileName = Application.streamingAssetsPath + "\\luau-compile.exe",
                        Arguments = $"--binary -O2 -g0 \"{Application.streamingAssetsPath + "\\Scripts\\" + targetScript + ".lua"}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                using (FileStream outputFile = new FileStream(Application.streamingAssetsPath + "\\Compiled\\" + targetScript + ".bin", FileMode.Create))
                {
                    compile.Start();
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = compile.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outputFile.Write(buffer, 0, bytesRead);
                    }
                }
                string stdout = compile.StandardOutput.ReadToEnd();
                string stderr = compile.StandardOutput.ReadToEnd();
                compile.WaitForExit();
                if (File.Exists(Application.streamingAssetsPath + "\\Compiled\\" + targetScript + ".bin"))
                {
                    Logging.Print("Script compiled, attempting read...");
                    br = new ByteReader(File.ReadAllBytes(Application.streamingAssetsPath + "\\Compiled\\" + targetScript + ".bin"));
                    Parse();
                    /*
                    if (ready)
                        if (tag == "LocalScript" && Array.IndexOf(validExecutionSpots, transform.parent.tag) >= 0)
                        {
                            Logging.Debug(targetScript + " will execute.");
                            StartCoroutine(Execute());
                        }
                    */
                }
            } else {
                Logging.Error("Could not find luau-compile.exe, cannot compile.", "Luauni:Start");
                enabled = false;
            }
        } else
        {
            Logging.Error("Could not find script file: " + targetScript + ".lua", "Luauni:Start");
            enabled = false;
        }
    }

    public bool IsReady()
    {
        return ready;
    }

    private ByteReader? br;
    private bool ready = true;
    public bool initiated = true;
    private string[]? stringtable;
    private Proto[]? protos;

    private int mainProtoId;
    private Proto mainProto;

    private List<string> watchGlobals = new List<string>();

    void Update()
    {
        string gen = $"\n\n{targetScript} {(main.complete ? "(DEAD)" : (main.yielded ? "(YIELDED)" : ""))}";
        for(int i = 0; i < main.pL.Count; i++)
        {
            gen+=$"\nLayer {i + 1}: Proto {main.pL[i].bytecodeid}, Position: {main.iP[i] + 1} / {main.pL[i].sizecode}";
        }
        if (!DebugText.scriptInfo.ContainsKey(targetScript)){
            DebugText.scriptInfo.Add(targetScript, gen);
        } else
        {
            DebugText.scriptInfo[targetScript] = gen;
        }
        if (main.complete && !printed)
        {
            Logging.Warn($"{targetScript} - Proto {mainProtoId} emulated progress: {main.iP[0] + 1} instructions / {protos[mainProtoId].sizecode} instructions.");
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
        Logging.Debug($"Loading {strings} strings from the string table...", "Luauni:Parse");
        for(int i = 0; i < strings; i++)
        {
            string str = br.ReadRangeStr(br.ReadVariableLen());
            stringtable[i] = str;
            //Logging.Debug($"String table entry #{i+1}: {str}", "Luauni:Parse");
        }

        // read and init protos

        int protoCount = br.ReadVariableLen();
        protos = new Proto[protoCount];
        Logging.Debug($"Script proto count: {protoCount}", "Luauni:Parse");
        for(int i = 0; i < protoCount; i++)
        {
            Proto p = ParseEssentials.PrepareProto(br, i, stringtable, transform, protos, this);
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

        maincl = new Closure()
        {
            p = mainProto,
            upvals = new object[0],
            owner = this
        };
        main = new SClosure()
        {
            source = maincl,
            args = new object[0],
            initiated = true
        };
        main.pL.Add(mainProto);
        main.cL.Add(null);
        main.iP.Add(-1);
        ready = true;
        Logging.Print("Bytecode loaded for "+targetScript+", ready for execution.", "Luauni:Parse");
        TaskScheduler.instance.InitiateScript(ref main);
    }

    private Closure maincl;
    public SClosure main;

    public static bool ReflectionIndex(string key, ref SClosure target, uint inst)
    {
        object tgt = target.pL[target.cEL].registers[Luau.INSN_B(inst)];
        Type t = Misc.SafeType(tgt);
        FieldInfo test = t.GetField(key, search);
        Type test2 = t.GetNestedType(key, search);
        MethodInfo test3 = t.GetMethod(key, search);
        PropertyInfo test4 = t.GetProperty(key, search);
        if (test != null)
        {
            object send = test.GetValue(tgt);
            target.pL[target.cEL].registers[Luau.INSN_A(inst)] = send;
        }
        else if (test2 != null)
        {
            target.pL[target.cEL].registers[Luau.INSN_A(inst)] = test2;
        }
        else if (test3 != null)
        {
            target.pL[target.cEL].registers[Luau.INSN_A(inst)] = (Globals.Standard)Delegate.CreateDelegate(typeof(Globals.Standard), test3.IsStatic ? null : tgt, test3);
        }
        else if (test4 != null)
        {
            object send = test4.GetValue(tgt);
            target.pL[target.cEL].registers[Luau.INSN_A(inst)] = send;
        }
        else
        {
            FieldInfo isObject = t.GetField("isObject");
            if (isObject == null)
            {
                Logging.Error($"Internal error: isObject not part of class {t.Name}", "Luauni:Step"); return false;
            }
            bool indexable = (bool)isObject.GetValue(t);
            if (indexable)
            {
                FieldInfo f1 = t.GetField("source", search);
                GameObject obj;
                if (f1 != null)
                {
                    obj = (GameObject)(f1.GetValue(tgt));
                }
                else
                {
                    obj = ((Component)tgt).gameObject;
                }
                Transform find = obj.transform.Find(key);
                if (find != null)
                {
                    target.pL[target.cEL].registers[Luau.INSN_A(inst)] = Misc.TryGetType(find);
                }
                else
                {
                    Logging.Error($"{key} is not a valid member of {tgt}", "Luauni:Step"); return false;
                }
            }
            else
            {
                Logging.Error($"{key} is not a valid member of {tgt}", "Luauni:Step"); return false;
            }
        }
        return true;
    }

    public static System.Collections.IEnumerator Execute(SClosure target)
    {
        target.yielded = false;
        if (!target.initiated)
        {
            int idx = 0;
            foreach(object o in target.args)
            {
                target.source.p.registers[idx] = o;
                idx++;
            }
            target.initiated = true;
        }
        bool should_loop = true;
        Proto currentProto = target.pL[target.cEL];
        while (should_loop && !target.yielded)
        {
            uint inst = target.nextInst();
            LuauOpcode opcode = (LuauOpcode)Luau.INSN_OP(inst);
            Logging.Debug($"Proto {target.pL[target.cEL].bytecodeid} executing opcode {opcode}", "Luauni:Step");
            switch (opcode)
            {
                case LuauOpcode.LOP_NOP:
                case LuauOpcode.LOP_BREAK:
                case LuauOpcode.LOP_PREPVARARGS:
                case LuauOpcode.LOP_CLOSEUPVALS:
                case LuauOpcode.LOP_FASTCALL:
                case LuauOpcode.LOP_FASTCALL1:
                case LuauOpcode.LOP_FASTCALL2:
                case LuauOpcode.LOP_FASTCALL2K:
                    Logging.Debug($"Ignoring opcode not planned to support: {opcode}", "Luauni:Step");
                    break;
                case LuauOpcode.LOP_ADD:
                    {
                        uint b = Luau.INSN_B(inst);
                        object rg1 = currentProto.registers[Luau.INSN_B(inst)];
                        object rg2 = currentProto.registers[Luau.INSN_C(inst)];
                        if (rg1 == null || rg2 == null)
                        {
                            Logging.Error($"attempt to perform arithmetic (add) on {Misc.GetTypeName(rg1)} and {Misc.GetTypeName(rg2)}", "Luauni:Step");
                            target.complete = true;
                            yield break;
                        }
                        else
                        {
                            currentProto.registers[Luau.INSN_A(inst)] = (double)rg1 + (double)rg2;
                        }
                    }
                    break;
                case LuauOpcode.LOP_ADDK:
                    {
                        double rg1 = (double)currentProto.registers[Luau.INSN_B(inst)];
                        double rg2 = (double)currentProto.k[Luau.INSN_C(inst)];
                        currentProto.registers[Luau.INSN_A(inst)] = rg1 + rg2;
                    }
                    break;
                case LuauOpcode.LOP_AND:
                    currentProto.registers[Luau.INSN_A(inst)] = Luau.LIKELY(currentProto.registers[Luau.INSN_B(inst)]) ? currentProto.registers[Luau.INSN_C(inst)] : null;
                    break;
                case LuauOpcode.LOP_CALL:
                    {
                        uint reg = Luau.INSN_A(inst);
                        uint args = Luau.INSN_B(inst);
                        uint returns = Luau.INSN_C(inst);
                        object regcopy = currentProto.registers[reg];
                        if(regcopy == null)
                        {
                            Logging.Error($"Attempt to call a nil value", "Luauni:Step");
                            target.complete = true;
                            yield break;
                        }
                        Type regType = regcopy.GetType();
                        if (regType == typeof(Globals.Standard))
                        {
                            Logging.Debug("Calling a standard function.", "Luauni:Step");
                            Globals.Standard tgt = (Globals.Standard)regcopy;
                            CallData send = new CallData()
                            {
                                initiator = currentProto,
                                closure = target,
                                funcRegister = reg,
                                args = args,
                                returns = returns
                            };
                            yield return tgt.Invoke(send);
                            if (currentProto.globalErrored)
                            {
                                target.complete = true;
                                yield break;
                            }
                        } else if (regType == typeof(Closure))
                        {
                            currentProto.callReg = reg; currentProto.expectedReturns = returns;
                            Closure cl = (Closure)currentProto.registers[reg];
                            Proto pr = cl.p;
                            Logging.Debug($"Moving exeution to proto {pr.bytecodeid}, passing {args} args.", "Luauni:Step");
                            if (args == 0)
                            {
                                args = currentProto.stacktop - reg;
                                Logging.Debug($"Sending MULTRET ({args}) arguments.", "Luauni:Step");
                            } else
                            {
                                args--;
                            }
                            for (int i = 0; i < pr.maxstacksize; i++)
                            {
                                pr.registers[i] = i < args ? currentProto.registers[reg + i + 1] : null;
                            }
                            target.pL.Add(pr);
                            target.iP.Add(-1);
                            target.cL.Add(cl);
                            target.cEL++;
                            currentProto = pr;
                        } else
                        {
                            Logging.Warn("Unsupported function type: " + regType);
                        }
                        break;
                    }
                case LuauOpcode.LOP_CAPTURE:
                    {
                        LuauCaptureType cap = (LuauCaptureType)Luau.INSN_A(inst);
                        if(target.lCC.loadedUps >= target.lCC.p.nups)
                        {
                            Logging.Error($"Cannot CAPTURE upvalue because nups limit has been exceeded for the proto.", "Luauni:Step"); target.complete = true; yield break;
                        }
                        Logging.Debug($"Capturing upvalue of type {cap}", "Luauni:Step");
                        if(cap == LuauCaptureType.LCT_VAL)
                        {
                            target.lCC.upvals[target.lCC.loadedUps] = currentProto.registers[Luau.INSN_B(inst)];
                        } else
                        {
                            target.lCC.upvals[target.lCC.loadedUps] = new UpvalREF()
                            {
                                src = currentProto,
                                register = Luau.INSN_B(inst)
                            };
                        }
                        target.lCC.loadedUps++;
                        break;
                    }
                case LuauOpcode.LOP_CONCAT:
                    {
                        string output = "";
                        uint regStart = Luau.INSN_B(inst);
                        uint regEnd = Luau.INSN_C(inst);
                        for (int i = 0; i < regEnd - regStart + 1; i++)
                        {
                            object reg = currentProto.registers[regStart + i];
                            output += reg == null ? "null" : Luau.accurate_tostring(reg);
                        }
                        currentProto.registers[Luau.INSN_A(inst)] = output;
                    }
                    break;
                case LuauOpcode.LOP_DIV:
                    {
                        double rg1 = (double)currentProto.registers[Luau.INSN_B(inst)];
                        double rg2 = (double)currentProto.registers[Luau.INSN_C(inst)];
                        currentProto.registers[Luau.INSN_A(inst)] = rg1 / rg2;
                    }
                    break;
                case LuauOpcode.LOP_DIVK:
                    {
                        double rg1 = (double)currentProto.registers[Luau.INSN_B(inst)];
                        double rg2 = (double)currentProto.k[Luau.INSN_C(inst)];
                        currentProto.registers[Luau.INSN_A(inst)] = rg1 / rg2;
                    }
                    break;
                case LuauOpcode.LOP_DUPCLOSURE:
                    {
                        target.lCC = (Closure)currentProto.k[Luau.INSN_D(inst)];
                        currentProto.registers[Luau.INSN_A(inst)] = target.lCC;
                        break;
                    }
                case LuauOpcode.LOP_DUPTABLE:
                    {
                        currentProto.registers[Luau.INSN_A(inst)] = currentProto.k[Luau.INSN_D(inst)];
                        break;
                    }
                case LuauOpcode.LOP_FORGPREP_NEXT:
                case LuauOpcode.LOP_FORGPREP:
                    {
                        int targ = (int)Luau.INSN_A(inst);
                        currentProto.registers[targ + 2] = 0;
                        target.jumpSteps(Luau.INSN_D(inst));
                    }
                    break;
                case LuauOpcode.LOP_FORGLOOP:
                    {
                        int targ = (int)Luau.INSN_A(inst);
                        int jmp = Luau.INSN_D(inst);
                        Type t = Misc.SafeType(currentProto.registers[targ]);
                        uint varcount = target.nextInst();
                        (bool, object[]) get;
                        if (t == typeof(TableIterator))
                        {
                            TableIterator iter = (TableIterator)currentProto.registers[targ];
                            get = iter.Get();
                        } else
                        {
                            ArrayIterator iter = (ArrayIterator)currentProto.registers[targ];
                            get = iter.Get();
                        }
                        if (get.Item1)
                        {
                            object[] array = get.Item2;
                            for (int i = 0; i < varcount; i++)
                            {
                                currentProto.registers[targ + 3 + i] = (i < array.Length ? array[i] : null);
                            }
                            target.jumpSteps(jmp - 1);
                        }
                    }
                    break;
                case LuauOpcode.LOP_FORNPREP:
                    {
                        uint regId = Luau.INSN_A(inst);
                        double limit = (double)currentProto.registers[regId];
                        double step = (double)currentProto.registers[regId + 1];
                        double idx = (double)currentProto.registers[regId + 2];
                        target.jumpSteps((step > 0 ? idx <= limit : limit <= idx) ? 0 : Luau.INSN_D(inst));
                    }
                    break;
                case LuauOpcode.LOP_FORNLOOP:
                    {
                        uint regId = Luau.INSN_A(inst);
                        double limit = (double)currentProto.registers[regId];
                        double step = (double)currentProto.registers[regId + 1];
                        double idx = (double)currentProto.registers[regId + 2] + step;
                        currentProto.registers[regId + 2] = idx;
                        if (step > 0 ? idx <= limit : limit <= idx)
                        {
                            target.jumpSteps(Luau.INSN_D(inst));
                        }
                    }
                    break;
                case LuauOpcode.LOP_GETGLOBAL:
                    {
                        string key = (string)currentProto.k[target.nextInst()];
                        if (key == "script")
                        {
                            currentProto.registers[Luau.INSN_A(inst)] = target.source.owner.gameObject;
                        }
                        else
                        {
                            currentProto.registers[Luau.INSN_A(inst)] = Globals.Get(key);
                        }
                        break;
                    }
                case LuauOpcode.LOP_GETIMPORT:
                    {
                        uint aux = target.nextInst();
                        int pathLength = (int)(aux >> 30);
                        List<string> importPathParts = new List<string>();
                        for (int a = 0; a < pathLength; a++)
                        {
                            int shiftAmount = 10 * a;
                            int index = (int)((aux >> (20 - shiftAmount)) & 1023);
                            if (index >= currentProto.k.Length)
                            {
                                Logging.Error($"Invalid constant index for GETIMPORT: {index}."); target.complete = true; yield break;
                            }
                            else
                            {
                                importPathParts.Add(currentProto.k[index].ToString());
                            }
                        }
                        string importPath = string.Join('.', importPathParts);
                        Logging.Debug("Retrieving import: " + importPath, "Luauni:Step");
                        currentProto.registers[Luau.INSN_A(inst)] = currentProto.imports[importPath];
                        break;
                    }
                case LuauOpcode.LOP_GETTABLE:
                    {
                        object rg = currentProto.registers[Luau.INSN_B(inst)];
                        Type t = Misc.SafeType(rg);
                        if (t == typeof(object[]))
                        {
                            object[] arr = (object[])rg;
                            int index = Convert.ToInt32((double)currentProto.registers[Luau.INSN_C(inst)]) - 1;
                            currentProto.registers[Luau.INSN_A(inst)] = index < arr.Length && index >= 0 ? arr[index] : null;
                        }
                        else if (t == typeof(Dictionary<string, object>))
                        {
                            Dictionary<string, object> arr = (Dictionary<string, object>)rg;
                            object idx = currentProto.registers[Luau.INSN_C(inst)];
                            if (idx.GetType() == typeof(string))
                            {
                                currentProto.registers[Luau.INSN_A(inst)] = arr[(string)currentProto.registers[Luau.INSN_C(inst)]];
                            }
                            else
                            {
                                Logging.Error($"Attempt to index table with {idx.GetType()}", "Luauni:Step"); target.complete = true; yield break;
                            }
                        }
                        else if (t == typeof(NamedDict))
                        {
                            NamedDict nd = (NamedDict)rg;
                            string key = (string)currentProto.registers[Luau.INSN_C(inst)];
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
                                currentProto.registers[Luau.INSN_A(inst)] = val;
                                Logging.Debug($"Found key: {val}", "Luauni:Step");
                            }
                            else
                            {
                                Logging.Error($"{key} is not a valid member of {nd.name}", "Luauni:Step"); target.complete = true; yield break;
                            }
                        } else {
                            string key = (string)currentProto.registers[Luau.INSN_C(inst)];
                            if (!ReflectionIndex(key, ref target, inst))
                            {
                                target.complete = true; yield break;
                            };
                        }
                        break;
                    }
                case LuauOpcode.LOP_GETTABLEKS:
                    {
                        string key = (string)currentProto.k[target.nextInst()];
                        Logging.Debug($"GETTABLEKS key: {key}", "Luauni:Step");
                        object tgt = currentProto.registers[Luau.INSN_B(inst)];
                        Logging.Debug(tgt);
                        if(tgt==null)
                        {
                            Logging.Error($"Attempt to index nil with {key}", "Luauni:Step"); target.complete = true; yield break;
                        }
                        Type t = Misc.SafeType(tgt);
                        if(t == typeof(NamedDict))
                        {
                            NamedDict nd = (NamedDict)tgt;
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
                                currentProto.registers[Luau.INSN_A(inst)] = val;
                                Logging.Debug($"Found key: {val}", "Luauni:Step");
                            }
                            else
                            {
                                //Logging.Error($"{key} is not a valid member of {nd.name}", "Luauni:Step"); target.complete = true; yield break;
                                currentProto.registers[Luau.INSN_A(inst)] = null;
                            }
                        }
                        else if(t == typeof(GameObject) || key == "camera")
                        {
                            GameObject obj = (GameObject)(t == typeof(GameObject) ? tgt : Misc.SafeGameObjectFromClass(tgt));
                            Transform find = obj.transform.Find(key);
                            if (find != null)
                            {
                                currentProto.registers[Luau.INSN_A(inst)] = Misc.TryGetType(find);
                            }
                            else
                            {
                                Logging.Error($"{key} is not a valid member of {t.Name}", "Luauni:Step"); target.complete = true; yield break;
                            }
                        }
                        else 
                        {
                            if (!ReflectionIndex(key, ref target, inst))
                            {
                                target.complete = true; yield break;
                            };
                        }
                        break;
                    }
                case LuauOpcode.LOP_GETTABLEN:
                    {
                        object rg = currentProto.registers[Luau.INSN_B(inst)];
                        Type t = Misc.SafeType(rg);
                        if (t == typeof(object[]))
                        {
                            object[] arr = (object[])rg;
                            int index = Convert.ToInt32(Luau.INSN_C(inst)) - 1;
                            currentProto.registers[Luau.INSN_A(inst)] = index < arr.Length && index >= 0 ? arr[index] : null;
                        }
                        else
                        {
                            Logging.Error($"Cannot perform GETTABLEN operation on variable of type {rg}", "Luauni:Step");
                            target.complete = true;
                            yield break;
                        }
                        break;
                    }
                case LuauOpcode.LOP_GETUPVAL:
                    {
                        uint idx = Luau.INSN_B(inst);
                        Closure cl = target.cL[target.cEL];
                        if(cl.loadedUps <= idx)
                        {
                            Logging.Error($"Cannot GETUPVAL because index is outside the range of loaded upvalues.", "Luauni:Step");
                            target.complete = true;
                            yield break;
                        }
                        object upvalue = cl.upvals[idx];
                        if(upvalue.GetType() == typeof(UpvalREF))
                        {
                            UpvalREF refer = (UpvalREF)upvalue;
                            currentProto.registers[Luau.INSN_A(inst)] = refer.src.registers[refer.register];
                        } else
                        {
                            currentProto.registers[Luau.INSN_A(inst)] = upvalue;
                        }
                        break;
                    }
                case LuauOpcode.LOP_JUMP:
                case LuauOpcode.LOP_JUMPBACK:
                    target.jumpSteps(Luau.INSN_D(inst));
                    break;
                case LuauOpcode.LOP_JUMPIF:
                    {
                        object reg = currentProto.registers[Luau.INSN_A(inst)];
                        if (Luau.LIKELY(reg))
                        {
                            Logging.Debug("JUMPIF PASS");
                            target.jumpSteps(Luau.INSN_D(inst));
                        }
                    }
                    break;
                case LuauOpcode.LOP_JUMPIFNOT:
                    {
                        object reg = currentProto.registers[Luau.INSN_A(inst)];
                        if (!Luau.LIKELY(reg))
                        {
                            Logging.Debug("JUMPIFNOT PASS");
                            target.jumpSteps(Luau.INSN_D(inst));
                        }
                    }
                    break;
                case LuauOpcode.LOP_JUMPIFEQ:
                    {
                        uint AUX = target.nextInst();
                        if (Luau.EQUAL(currentProto.registers[Luau.INSN_A(inst)], currentProto.registers[AUX]))
                        {
                            Logging.Debug("JUMPIFEQ PASS");
                            target.jumpSteps(Luau.INSN_D(inst) - 1);
                        }
                        break;
                    }
                case LuauOpcode.LOP_JUMPIFNOTEQ:
                    if (!Luau.EQUAL(currentProto.registers[Luau.INSN_A(inst)], currentProto.registers[target.nextInst()]))
                    {
                        Logging.Debug("JUMPIFNOTEQ PASS");
                        target.jumpSteps(Luau.INSN_D(inst) - 1);
                    }
                    break;
                case LuauOpcode.LOP_JUMPIFLT:
                    if ((double)currentProto.registers[Luau.INSN_A(inst)] < (double)currentProto.registers[target.nextInst()])
                    {
                        Logging.Debug("JUMPIFLT PASS");
                        target.jumpSteps(Luau.INSN_D(inst) - 1);
                    }
                    break;
                case LuauOpcode.LOP_JUMPIFNOTLT:
                    if (!((double)currentProto.registers[Luau.INSN_A(inst)] < (double)currentProto.registers[target.nextInst()]))
                    {
                        Logging.Debug("JUMPIFNOTLT PASS");
                        target.jumpSteps(Luau.INSN_D(inst) - 1);
                    }
                    break;
                case LuauOpcode.LOP_JUMPXEQKNIL:
                case LuauOpcode.LOP_JUMPXEQKB:
                case LuauOpcode.LOP_JUMPXEQKN:
                case LuauOpcode.LOP_JUMPXEQKS:
                    {
                        uint AUX = target.nextInst();
                        bool flip = (AUX >> 31) == 1 ? false : true;
                        if (flip == Luau.EQUAL(currentProto.registers[Luau.INSN_A(inst)], currentProto.k[AUX & 16777215]))
                        {
                            Logging.Debug("JUMPXEQK PASS");
                            target.jumpSteps(Luau.INSN_D(inst) - 1);
                        }
                        break;
                    }
                case LuauOpcode.LOP_LENGTH:
                    {
                        object reg = currentProto.registers[Luau.INSN_B(inst)];
                        Type tp = reg.GetType();
                        if (tp == typeof(string))
                        {
                            currentProto.registers[Luau.INSN_A(inst)] = (double)((string)reg).Length;
                        }
                        else if (tp == typeof(object[]))
                        {
                            currentProto.registers[Luau.INSN_A(inst)] = (double)((object[])reg).Length;
                        }
                        else
                        {
                            Logging.Error($"attempt to get length of a {tp}", "Luauni:Step"); target.complete = true; yield break;
                        }
                    }
                    break;
                case LuauOpcode.LOP_LOADB:
                    currentProto.registers[Luau.INSN_A(inst)] = Luau.INSN_B(inst)==1;
                    target.jumpSteps((int)Luau.INSN_C(inst));
                    break;
                case LuauOpcode.LOP_LOADK:
                    object constant = currentProto.k[Luau.INSN_D(inst)];
                    currentProto.registers[Luau.INSN_A(inst)] = constant;
                    break;
                case LuauOpcode.LOP_LOADN:
                    currentProto.registers[Luau.INSN_A(inst)] = (double)Luau.INSN_D(inst);
                    break;
                case LuauOpcode.LOP_LOADNIL:
                    currentProto.registers[Luau.INSN_A(inst)] = null;
                    break;
                case LuauOpcode.LOP_MOVE:
                    currentProto.registers[Luau.INSN_A(inst)] = currentProto.registers[Luau.INSN_B(inst)];
                    break;
                case LuauOpcode.LOP_MINUS:
                    currentProto.registers[Luau.INSN_A(inst)] = -(double)currentProto.registers[Luau.INSN_B(inst)];
                    break;
                case LuauOpcode.LOP_MOD:
                    {
                        double rg1 = (double)currentProto.registers[Luau.INSN_B(inst)];
                        double rg2 = (double)currentProto.registers[Luau.INSN_C(inst)];
                        double result = 0;
                        if (double.IsInfinity(rg2))
                        {
                            result = double.NaN;
                        } else
                        {
                            result = rg1 % rg2;
                            if (rg2 < 0 && result >= 0) { result += rg2; }
                        }
                        currentProto.registers[Luau.INSN_A(inst)] = result;
                    }
                    break;
                case LuauOpcode.LOP_MODK:
                    {
                        double rg1 = (double)currentProto.registers[Luau.INSN_B(inst)];
                        double rg2 = (double)currentProto.k[Luau.INSN_C(inst)];
                        double result = 0;
                        if (double.IsInfinity(rg2))
                        {
                            result = double.NaN;
                        }
                        else
                        {
                            result = rg1 % rg2;
                            if (rg2 < 0 && result >= 0) { result += rg2; }
                        }
                        currentProto.registers[Luau.INSN_A(inst)] = result;
                    }
                    break;
                case LuauOpcode.LOP_MUL:
                    {
                        dynamic rg1 = currentProto.registers[Luau.INSN_B(inst)];
                        dynamic rg2 = currentProto.registers[Luau.INSN_C(inst)];
                        currentProto.registers[Luau.INSN_A(inst)] = rg1 * rg2;
                    }
                    break;
                case LuauOpcode.LOP_MULK:
                    {
                        dynamic rg1 = currentProto.registers[Luau.INSN_B(inst)];
                        dynamic rg2 = currentProto.k[Luau.INSN_C(inst)];
                        currentProto.registers[Luau.INSN_A(inst)] = rg1 * rg2;
                    }
                    break;
                case LuauOpcode.LOP_NAMECALL:
                    {
                        uint a = Luau.INSN_A(inst);
                        uint b = Luau.INSN_B(inst);
                        string key = (string)currentProto.k[target.nextInst()];
                        object reg = currentProto.registers[b];
                        if(reg == null)
                        {
                            Logging.Error($"Attempt to index nil with {key}", "Luauni:Step"); target.complete = true; yield break;
                        }
                        Type t = Misc.SafeType(reg);
                        MethodInfo get2 = t.GetMethod(key, search);
                        if (get2 != null)
                        {
                            currentProto.recentNameCalledRegister = currentProto.registers[b];
                            currentProto.registers[a] = (Globals.Standard)Delegate.CreateDelegate(typeof(Globals.Standard), get2.IsStatic ? null : reg, get2); ;
                        }
                        else
                        {
                            MethodInfo get = Type.GetType("InheritedByAll").GetMethod(key);
                            if (get != null)
                            {
                                currentProto.recentNameCalledRegister = currentProto.registers[b];
                                currentProto.registers[a] = (Globals.Standard)Delegate.CreateDelegate(typeof(Globals.Standard), get.IsStatic ? null : reg, get); ;
                            }
                            else
                            {
                                Logging.Error($"{key} is not a valid member of {reg}", "Luauni:Step"); target.complete = true; yield break;
                            }
                        }
                        break;
                    }
                case LuauOpcode.LOP_NEWCLOSURE:
                   target. lCC = new Closure() {
                        p = currentProto.p[Luau.INSN_D(inst)],
                        upvals = new object[currentProto.p[Luau.INSN_D(inst)].nups],
                        owner = target.source.owner
                    };
                    currentProto.registers[Luau.INSN_A(inst)] = target.lCC;
                    break;
                case LuauOpcode.LOP_NEWTABLE:
                    currentProto.registers[Luau.INSN_A(inst)] = new object[target.nextInst()];
                    break;
                case LuauOpcode.LOP_NOT:
                    currentProto.registers[Luau.INSN_A(inst)] = !Luau.LIKELY(currentProto.registers[Luau.INSN_B(inst)]);
                    break;
                case LuauOpcode.LOP_OR:
                    currentProto.registers[Luau.INSN_A(inst)] = Luau.LIKELY(currentProto.registers[Luau.INSN_B(inst)]) ? currentProto.registers[Luau.INSN_B(inst)] : currentProto.registers[Luau.INSN_C(inst)];
                    break;
                case LuauOpcode.LOP_POW:
                    {
                        double rg1 = (double)currentProto.registers[Luau.INSN_B(inst)];
                        double rg2 = (double)currentProto.registers[Luau.INSN_C(inst)];
                        currentProto.registers[Luau.INSN_A(inst)] = Math.Pow(rg1, rg2);
                    }
                    break;
                case LuauOpcode.LOP_POWK:
                    {
                        double rg1 = (double)currentProto.registers[Luau.INSN_B(inst)];
                        double rg2 = (double)currentProto.k[Luau.INSN_C(inst)];
                        currentProto.registers[Luau.INSN_A(inst)] = Math.Pow(rg1, rg2);
                    }
                    break;
                case LuauOpcode.LOP_RETURN:
                    if (target.pL.Count == 1)
                    {
                        //Logging.Print("Main proto execution finished.", "Luauni:Step"); target.complete = true; yield break;
                        target.complete = true; yield break;
                    } else
                    {
                        Logging.Debug($"Proto {currentProto.bytecodeid} is returning.", "Luauni:Step");
                        uint begin = Luau.INSN_A(inst);
                        uint returns = Luau.INSN_B(inst);
                        if(returns == 0)
                        {
                            returns = currentProto.stacktop - begin + 1;
                        } else
                        {
                            returns--;
                        }
                        uint startReg = target.pL[target.cEL - 1].callReg;
                        Proto edit = target.pL[target.cEL - 1];
                        for (int i = 0; i < returns; i++)
                        {
                            edit.registers[startReg + i] = i < edit.expectedReturns ? currentProto.registers[begin + i] : null;
                        }
                        currentProto = edit;
                        target.pL.RemoveAt(target.cEL);
                        target.iP.RemoveAt(target.cEL);
                        target.cL.RemoveAt(target.cEL);
                        target.cEL--;
                    }
                    break;
                case LuauOpcode.LOP_SETGLOBAL:
                    {
                        string key = (string)currentProto.k[target.nextInst()];
                        Globals.Set(key, currentProto.registers[Luau.INSN_A(inst)]);
                    }
                    break;
                case LuauOpcode.LOP_SETLIST:
                    {
                        int valcount = (int)Luau.INSN_C(inst) - 1;
                        object[] reg = (object[])currentProto.registers[Luau.INSN_A(inst)];
                        uint src = Luau.INSN_B(inst);
                        uint aux = target.nextInst();
                        for (int i = 0; i < valcount; i++)
                        {
                            reg[aux + i - 1] = currentProto.registers[src + i];
                        }
                    }
                    break;
                case LuauOpcode.LOP_SETTABLE:
                    {
                        object idx = currentProto.registers[Luau.INSN_C(inst)];
                        Type t = idx.GetType();
                        Logging.Debug(t);
                        if (t == typeof(string))
                        {
                            ((Dictionary<string, object>)currentProto.registers[Luau.INSN_B(inst)])[(string)idx] = currentProto.registers[Luau.INSN_A(inst)];
                        }
                        else
                        {
                            int index = Convert.ToInt32((double)idx) - 1;
                            object[] src = (object[])currentProto.registers[Luau.INSN_B(inst)];
                            int len = src.Length;
                            if (len < index + 1) {
                                object[] nw = new object[index + 1];
                                Array.Copy(src, 0, nw, 0, len);
                                currentProto.registers[Luau.INSN_B(inst)] = nw;
                            }
                            ((object[])currentProto.registers[Luau.INSN_B(inst)])[index] = currentProto.registers[Luau.INSN_A(inst)];
                        }
                    }
                    break;
                case LuauOpcode.LOP_SETTABLEKS:
                    {
                        string key = (string)currentProto.k[target.nextInst()];
                        Logging.Debug($"SETTABLEKS key: {key}", "Luauni:Step");
                        object tgt = currentProto.registers[Luau.INSN_B(inst)];
                        if (tgt == null)
                        {
                            Logging.Error($"Attempt to index nil with {key}", "Luauni:Step"); target.complete = true; yield break;
                        }
                        Type t = Misc.SafeType(tgt);
                        Logging.Debug(t);
                        Logging.Debug(tgt);
                        if (t == typeof(NamedDict))
                        {
                            NamedDict nd = (NamedDict)tgt;
                            if (nd.dict.TryGetValue(key, out object val))
                            {
                                nd.dict[key] = currentProto.registers[Luau.INSN_A(inst)];
                            }
                            else
                            {
                                nd.dict.Add(key, currentProto.registers[Luau.INSN_A(inst)]);
                            }
                        } else if (t == typeof(Dictionary<string, object>))
                        {
                            Dictionary<string, object> arr = (Dictionary<string, object>)tgt;
                            if (arr.ContainsKey(key))
                            {
                                arr[key] = currentProto.registers[Luau.INSN_A(inst)];
                            } else
                            {
                                arr.Add(key, currentProto.registers[Luau.INSN_A(inst)]);
                            }
                        } else
                        {
                            PropertyInfo p = t.GetProperty(key, search);
                            if (p != null)
                            {
                                p.SetValue(tgt, currentProto.registers[Luau.INSN_A(inst)]);
                            }
                            else
                            {
                                FieldInfo f = t.GetField(key, search);
                                if (f != null)
                                {
                                    f.SetValue(tgt, currentProto.registers[Luau.INSN_A(inst)]);
                                }
                                else
                                {
                                    Logging.Error($"{key} is not a valid member of {t}", "Luauni:Step"); target.complete = true; yield break;
                                }
                            }
                        }
                        break;
                    }
                case LuauOpcode.LOP_SETTABLEN:
                    {
                        object rg = currentProto.registers[Luau.INSN_B(inst)];
                        Type t = Misc.SafeType(rg);
                        if (t == typeof(object[]))
                        {
                            object[] arr = (object[])rg;
                            int index = Convert.ToInt32(Luau.INSN_C(inst)) - 1;
                            if (arr.Length <= index)
                            {
                                Logging.Debug("Array extension required", "Luauni:Step");
                                object[] newarr = new object[index+1];
                                int a = 0;
                                foreach(object i in arr)
                                {
                                    newarr[a] = i;
                                    a++;
                                }
                                arr = newarr;
                            }
                            arr[index] = currentProto.registers[Luau.INSN_A(inst)];
                            currentProto.registers[Luau.INSN_B(inst)] = arr;
                        }
                        else
                        {
                            Logging.Error($"Cannot perform SETTABLEN operation on variable of type {rg}", "Luauni:Step");
                            target.complete = true;
                            yield break;
                        }
                        break;
                    }
                case LuauOpcode.LOP_SETUPVAL:
                    {
                        uint idx = Luau.INSN_B(inst);
                        Closure cl = target.cL[target.cEL];
                        if (cl.loadedUps <= idx)
                        {
                            Logging.Error($"Cannot SETUPVAL because index is outside the range of loaded upvalues.", "Luauni:Step"); target.complete = true; yield break;
                        }
                        object upvalue = cl.upvals[idx];
                        if (upvalue.GetType() == typeof(UpvalREF))
                        {
                            UpvalREF refer = (UpvalREF)upvalue;
                            refer.src.registers[refer.register] = currentProto.registers[Luau.INSN_A(inst)];
                        }
                        else
                        {
                            cl.upvals[idx] = currentProto.registers[Luau.INSN_A(inst)];
                        }
                        break;
                    }
                case LuauOpcode.LOP_SUB:
                    {
                        double rg1 = (double)currentProto.registers[Luau.INSN_B(inst)];
                        double rg2 = (double)currentProto.registers[Luau.INSN_C(inst)];
                        currentProto.registers[Luau.INSN_A(inst)] = rg1 - rg2;
                    }
                    break;
                case LuauOpcode.LOP_SUBK:
                    {
                        double rg1 = (double)currentProto.registers[Luau.INSN_B(inst)];
                        double rg2 = (double)currentProto.k[Luau.INSN_C(inst)];
                        currentProto.registers[Luau.INSN_A(inst)] = rg1 - rg2;
                    }
                    break;
                default:
                    Logging.Error($"Unsupported opcode: {opcode}", "Luauni:Step"); target.complete = true; yield break;
            }
            if (config[0])
            {
                yield return null;
            }
        }
        yield break;
    }
}
