using System.Collections;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
{
    public static IEnumerator GetMouse(CallData dat)
    {
        Luau.returnToProto(ref dat, new object[1] { Mouse.instance.GetType() });
        yield break;
    }

    public RBXScriptSignal Chatted = new RBXScriptSignal(); // we have no chat

    public static LocalPlayer instance;
    public static bool isObject = true;
    public static GameObject source;
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            source = gameObject;
        }
        else
        {
            Debug.LogWarning($"Destroying duplicate singleton '{name}'");
            DestroyImmediate(gameObject);
        }
    }
}