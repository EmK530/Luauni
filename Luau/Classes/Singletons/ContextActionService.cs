using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextActionService : MonoBehaviour
{
    public static IEnumerator BindActionToInputTypes(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        //we can't do this yet
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }

    public static ContextActionService instance;
    public static bool isObject = false;
    public static bool isStatic = true;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning($"Destroying duplicate singleton '{name}'");
            DestroyImmediate(gameObject);
        }
    }
}
