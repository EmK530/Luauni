using System.Collections;
using UnityEngine;

public class Model : MonoBehaviour
{
    public readonly string ClassName = "Model";

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