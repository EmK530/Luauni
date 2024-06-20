using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class BillboardGui : MonoBehaviour
{
    public readonly string ClassName = "BillboardGui";

    Canvas cv;

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

    [SerializeField]
    private bool _enabled;
    public bool Enabled
    {
        get {
            if (cv == null) { cv = GetComponent<Canvas>(); cv.enabled = _enabled; }
            return cv.enabled;
        }
        set
        {
            if(cv == null) { cv = GetComponent<Canvas>(); }
            cv.enabled = value;
        }
    }

    public static bool isObject = true;

    void Start()
    {
        cv = GetComponent<Canvas>();
        cv.enabled = _enabled;
    }

    void LateUpdate()
    {
        Transform cam = Camera.source.transform;
        transform.LookAt(transform.position + cam.rotation * UnityEngine.Vector3.forward, cam.rotation * UnityEngine.Vector3.up);
    }
}