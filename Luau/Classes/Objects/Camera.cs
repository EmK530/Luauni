using System.Collections;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public readonly string ClassName = "Camera";
    public string CameraType = "Scriptable";

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

    public static bool isObject = true;

    void Start()
    {
        _cframe = new CoordinateFrame(transform.position, transform.rotation);
    }
}