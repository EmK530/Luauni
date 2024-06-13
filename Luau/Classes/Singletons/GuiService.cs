using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuiService : MonoBehaviour
{
    public static IEnumerator IsTenFootInterface(CallData dat) {
        // just say no
        Luau.returnToProto(ref dat, new object[1] { false });
        yield break;
    }

    public static GuiService instance;
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
