using System.Collections;
using UnityEngine;

public class Frame : MonoBehaviour
{
    public readonly string ClassName = "Frame";

    [SerializeField]
    private bool _visible = true;
    public bool Visible
    {
        get { return _visible; }
        set
        {
            _visible = value;
            gameObject.SetActive(value);
        }
    }

    public static bool isObject = true;

    void Start()
    {
        gameObject.SetActive(_visible);
    }
}