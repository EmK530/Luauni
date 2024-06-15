using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarterGui : MonoBehaviour
{
    public static IEnumerator SetCoreGuiEnabled(CallData dat) { Luau.returnToProto(ref dat, new object[0]); yield break; } // we do not have coregui

    public static StarterGui instance;
    public static bool isObject = true;

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
