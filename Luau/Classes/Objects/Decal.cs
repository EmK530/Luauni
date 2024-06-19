using System.Collections;
using UnityEngine;

public class Decal : MonoBehaviour
{
    public readonly string ClassName = "Decal";

    public object Parent
    {
        get { return Misc.TryGetType(transform.parent); }
        set
        {
            transform.SetParent(Misc.SafeGameObjectFromClass(value).transform);
        }
    }

    public static bool isObject = true;

    void Start()
    {
        
    }
}