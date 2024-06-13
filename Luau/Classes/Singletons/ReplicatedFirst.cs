using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplicatedFirst : MonoBehaviour
{
    private static bool _mouseIconEnabled = false;
    public static bool MouseIconEnabled
    {
        get { return _mouseIconEnabled; }
        set
        {
            _mouseIconEnabled = value;
            Cursor.visible = value;
        }
    }

    public static ReplicatedFirst instance;
    public static bool isObject = true;
    public static GameObject source;
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            source = gameObject;
            source.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Destroying duplicate singleton '{name}'");
            DestroyImmediate(gameObject);
        }
    }
}
