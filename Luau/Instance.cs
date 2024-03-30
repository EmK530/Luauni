#pragma warning disable CS8632

using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class Instance
{
    public GameObject src;
    public string path;
    public Dictionary<string, object> members = new Dictionary<string, object>();

    public object this[string key]
    {
        get { return members[key]; }
        set
        {
            Logging.Error(key);
            switch(key)
            {
                case "Visible":
                    src.SetActive((bool)value);
                    break;
            }
            members[key] = value;
        }
    }

    public Instance(GameObject inp, string pathAdd = "")
    {
        src = inp;
        if (pathAdd == "") {
            path = inp.name;
        } else
        {
            path = "/" + inp.name;
        }
        object get = Luau.SAFEINDEX(ClassFunctions.list, inp.tag);
        if (get!=null)
        {
            members = (Dictionary<string, object>)get;
        }
        members["Visible"] = src.activeSelf;
    }

    public Instance? Index(string name)
    {
        Transform find = src.transform.Find(name);
        if (find == null)
            return null;
        return new Instance(find.gameObject, path);
    }
}

public static class ClassFunctions
{
    public static bool prepared = false;
    public static Dictionary<string, object> list = new Dictionary<string, object>();

    public static void Init()
    {
        Globals.IterateClass(typeof(Collection), list, "", true);
        foreach(string i in list.Keys)
        {
            Globals.IterateClass(typeof(Collection.Any), (Dictionary<string, object>)list[i], i, false);
        }
    }

    public static class Collection
    {
        public static class DataModel
        {
            public static System.Collections.IEnumerator GetService(CallData dat)
            {
                object[] inp = Luau.getAllArgs(ref dat);
                Instance src = (Instance)dat.initiator.recentNameCalledRegister;
                string key = (string)inp[1];
                Transform find = src.src.transform.Find(key);
                if (find == null)
                {
                    dat.initiator.globalErrored = true;
                    Logging.Error($"'{key}' is not a valid Service name", "DataModel:GetService");
                    yield break;
                }
                Luau.returnToProto(ref dat, new object[1] { new Instance(find.gameObject, src.path) });
                yield break;
            }
        }
        public static class LocalPlayer
        {
            public static System.Collections.IEnumerator GetMouse(CallData dat)
            {
                Luau.returnToProto(ref dat, new object[1] { new Mouse() });
                yield break;
            }
        }
        public static class Untagged
        {
            public static System.Collections.IEnumerator Clone(CallData dat)
            {
                GameObject src = ESS.Clone(((Instance)dat.initiator.recentNameCalledRegister).src);
                Luau.returnToProto(ref dat, new object[1] { new Instance(src) });
                yield break;
            }
        }
        public static class Any
        {
            public static bool Visible = true;
            public static System.Collections.IEnumerator WaitForChild(CallData dat)
            {
                object[] inp = Luau.getAllArgs(ref dat);
                Instance src = (Instance)dat.initiator.recentNameCalledRegister;
                string key = (string)inp[1];
                Transform find = src.src.transform.Find(key);
                float st = Time.realtimeSinceStartup;
                bool alerted = false;
                while (find == null)
                {
                    yield return null;
                    find = src.src.transform.Find(key);
                    if(!alerted && find == null && Time.realtimeSinceStartup-st > 10f)
                    {
                        Logging.Warn($"Infinite yield possible on '{src.src.name}:WaitForChild(\"{key}\")'", "Instance:WaitForChild");
                        alerted = true;
                    }
                }
                Luau.returnToProto(ref dat, new object[1] { new Instance(find.gameObject, src.path) });
                yield break;
            }
        }
        public static class StarterGui
        {
            //dummy function because we have no coregui
            public static System.Collections.IEnumerator SetCoreGuiEnabled(CallData dat)
            {
                yield break;
            }
        }
        public static class UserInputService
        {
            public static bool MouseIconEnabled = true;
        }
    }
}

public class Mouse
{
    
}