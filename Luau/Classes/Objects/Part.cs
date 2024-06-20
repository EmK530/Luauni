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

    public double CollisionGroupId
    {
        get { return 1d; }
        set
        {
            Logging.Warn("Support for this property is postponed.", "Part:CollisionGroupId");
        }
    }

    public RBXScriptSignal Touched = new RBXScriptSignal();

    public static bool isObject = true;

    void Start()
    {
        
    }
}