using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInputService : MonoBehaviour
{
    private static bool _mouseIconEnabled = false;
    public static bool MouseIconEnabled
    {
        get { return _mouseIconEnabled; }
        set
        {
            _mouseIconEnabled = value;
            //Cursor.visible = value;
        }
    }

    public static RBXScriptSignal InputBegan = new RBXScriptSignal();
    public static RBXScriptSignal InputEnded = new RBXScriptSignal();
    public static RBXScriptSignal InputChanged = new RBXScriptSignal();
    public static RBXScriptSignal TouchStarted = new RBXScriptSignal();
    public static RBXScriptSignal TextBoxFocused = new RBXScriptSignal();
    public static RBXScriptSignal TextBoxFocusReleased = new RBXScriptSignal();

    public static UserInputService instance;
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
