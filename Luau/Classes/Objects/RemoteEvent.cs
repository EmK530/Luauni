using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RemoteEvent : MonoBehaviour
{
    public readonly string ClassName = "RemoteEvent";

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

    public IEnumerator FireServer(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Logging.Debug($"GameObject {name} received FireServer with {inp.Length} args");
        ServerFired.Invoke(inp);
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }

    public RBXScriptSignal OnClientEvent = new RBXScriptSignal();

    public UnityEvent<object[]> ServerFired = new UnityEvent<object[]>();

    public static bool isObject = true;
}