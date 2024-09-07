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
            if(!Application.isEditor)
                Cursor.visible = value;
        }
    }

    public static bool TouchEnabled = false;

    public Enum.MouseBehavior MouseBehavior = Enum.MouseBehavior.Default;

    public static RBXScriptSignal InputBegan = new RBXScriptSignal();
    public static RBXScriptSignal InputEnded = new RBXScriptSignal();
    public static RBXScriptSignal InputChanged = new RBXScriptSignal();
    public static RBXScriptSignal TouchStarted = new RBXScriptSignal();
    public static RBXScriptSignal TextBoxFocused = new RBXScriptSignal();
    public static RBXScriptSignal TextBoxFocusReleased = new RBXScriptSignal();

    public static UserInputService instance;
    public static bool isObject = false;

    void Awake()
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

    void Update()
    {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                InputObject obj = new InputObject();
                obj.UserInputState = Enum.UserInputState.Begin;
                obj.KeyCode = (Enum.KeyCode)key;
                StartCoroutine(
                    Misc.ExecuteCoroutine(
                        InputBegan._fire(new object[2] { obj, true })
                    )
                );
            } else if (Input.GetKeyUp(key))
            {
                InputObject obj = new InputObject();
                obj.UserInputState = Enum.UserInputState.End;
                obj.KeyCode = (Enum.KeyCode)key;
                StartCoroutine(
                    Misc.ExecuteCoroutine(
                        InputEnded._fire(new object[2] { obj, true })
                    )
                );
            }
        }
    }
}

public class InputObject
{
    public readonly string ClassName = "InputObject";
    public static bool isObject = false;

    public Vector3 Delta = new Vector3();
    public Enum.KeyCode KeyCode = 0;
    public Vector3 Position = new Vector3();
    public Enum.UserInputState UserInputState = Enum.UserInputState.None;
    public Enum.UserInputType UserInputType = Enum.UserInputType.None;
}