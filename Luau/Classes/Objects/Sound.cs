using System;
using System.Collections;
using UnityEngine;

public class Sound : MonoBehaviour
{
    public readonly string ClassName = "Sound";

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

    AudioSource src;

    [SerializeField]
    private string _soundId;
    public string SoundId
    {
        get { return _soundId; }
        set
        {
            SetSoundId(value);
        }
    }

    [SerializeField]
    private double _volume;
    public double Volume
    {
        get { return _volume; }
        set
        {
            GetComponent<AudioSource>().volume = (float)value;
            _volume = value;
        }
    }

    void SetSoundId(string value)
    {
        if (value.Contains("rbxassetid://"))
        {
            if (src == null) { src = GetComponent<AudioSource>(); }
            string id = value.Split("rbxassetid://")[1];
            AudioClip m = Resources.Load<AudioClip>("rbxassetid/Sound/" + id);
            if (m != null)
            {
                src.clip = m;
                _soundId = value;
            }
            else
            {
                Logging.Warn("Could not load asset ID: " + id, "Sound:SoundId");
            }
        }
    }

    public IEnumerator Play(CallData dat)
    {
        if (src == null) { src = GetComponent<AudioSource>(); }
        src.Play();
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }

    public IEnumerator Stop(CallData dat)
    {
        if (src == null) { src = GetComponent<AudioSource>(); }
        src.Stop();
        Luau.returnToProto(ref dat, new object[0]);
        yield break;
    }

    public static bool isObject = true;

    void Start()
    {
        src = GetComponent<AudioSource>();
        SetSoundId(_soundId);
    }
}