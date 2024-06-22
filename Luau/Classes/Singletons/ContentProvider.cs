using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentProvider : MonoBehaviour
{
    public static IEnumerator PreloadAsync(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        //we can't do this yet
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }

    public static ContentProvider instance;
    public static bool isObject = false;

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
