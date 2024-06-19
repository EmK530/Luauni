using System.Collections;
using UnityEngine;

public class Part : MonoBehaviour
{
    public readonly string ClassName = "BasePart";

    public object Parent
    {
        get { return Misc.TryGetType(transform.parent); }
        set
        {
            transform.SetParent(Misc.SafeGameObjectFromClass(value).transform);
        }
    }

    public RBXScriptSignal Touched = new RBXScriptSignal();

    public static bool isObject = true;

    void Start()
    {
        
    }
}