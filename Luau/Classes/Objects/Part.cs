using System.Collections;
using UnityEngine;

public class Part : MonoBehaviour
{
    public readonly string ClassName = "BasePart";

    public RBXScriptSignal Touched = new RBXScriptSignal();

    public static bool isObject = true;

    void Start()
    {
        
    }
}