using System.Collections;
using UnityEngine;

public class Sound : MonoBehaviour
{
    public readonly string ClassName = "Sound";

    AudioSource src;

    public IEnumerator Play(CallData dat)
    {
        src.Play();
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }

    public IEnumerator Stop(CallData dat)
    {
        src.Stop();
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }

    public static bool isObject = true;

    void Start()
    {
        src = GetComponent<AudioSource>();
    }
}