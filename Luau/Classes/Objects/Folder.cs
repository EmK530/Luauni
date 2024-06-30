using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Folder : MonoBehaviour
{
    public readonly string ClassName = "Decal";

    public string Name
    {
        get { return name; }
        set
        {
            name = value;
        }
    }

    public object Parent
    {
        get { return Misc.TryGetType(transform.parent); }
        set
        {
            transform.SetParent(Misc.SafeGameObjectFromClass(value).transform);
        }
    }

    public static IEnumerator ClearAllChildren(CallData dat) {
        ESS.ClearAllChildren(Misc.SafeGameObjectFromClass(dat.initiator.recentNameCalledRegister));
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }

    public RBXScriptSignal ChildAdded = new RBXScriptSignal();
    public RBXScriptSignal ChildRemoved = new RBXScriptSignal();

    public static bool isObject = true;
    
    void Start()
    {
        
    }
}
