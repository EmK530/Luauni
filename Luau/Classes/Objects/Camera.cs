using System.Collections;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class Camera : MonoBehaviour
{
    public readonly string ClassName = "Camera";
    public string CameraType = "Scriptable";

    public string Name
    {
        get { return name; }
        set
        {
            name = value;
        }
    }

    private CoordinateFrame _cframe;
    public CoordinateFrame CFrame
    {
        get { return _cframe; }
        set
        {
            _cframe = value;
            transform.position = _cframe.Position;
            transform.rotation = _cframe.Rotation;
        }
    }

    public static Camera instance;
    public static bool isObject = true;
    public static GameObject source;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            source = gameObject;
            _cframe = new CoordinateFrame(transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning($"Destroying duplicate singleton '{name}'");
            DestroyImmediate(gameObject);
        }
    }
}