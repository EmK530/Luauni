using System.Collections;
using UnityEngine;

public class Part : MonoBehaviour
{
    public static bool isObject = true;
    public static bool isStatic = false;
    [HideInInspector]
    public GameObject source;

    void Start()
    {
        source = gameObject;
    }
}