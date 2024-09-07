using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using System.Security.Permissions;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

public class Beam : MonoBehaviour
{
    public static readonly List<Type> _inherits = new List<Type>()
    {
        typeof(Instance)
    };

    public readonly string ClassName = "Beam";

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

    LineRenderer lr;

    double w0 = 0;
    double w1 = 0;

    public double Width0
    {
        get { return w0; }
        set { w0 = value; lr.startWidth = (float)value; }
    }
    public double Width1
    {
        get { return w1; }
        set { w1 = value; lr.endWidth = (float)value; }
    }
    public bool Enabled
    {
        get { return lr.enabled; }
        set { lr.enabled = value; }
    }
    public NumberSequence _transparency = new NumberSequence(0);
    public NumberSequence Transparency
    {
        get { return _transparency; }
        set {
            _transparency = value;
            Gradient gd = value.ToAlphaGradient();
            //temporary fix while color is missing
            gd.colorKeys = new GradientColorKey[] {
                new GradientColorKey(Color.black,0f),
                new GradientColorKey(Color.black,1f)
            };
            lr.colorGradient = gd;
        }
    }
    [SerializeField]
    public Attachment Attachment0;
    [SerializeField]
    public Attachment Attachment1;

    public static bool isObject = true;

    void Awake()
    {
        if (lr == null) { lr = GetComponent<LineRenderer>(); }
    }

    void Update()
    {
        if(Attachment0!=null && Attachment1 != null)
        {
            lr.SetPosition(0, (Attachment0.transform.parent.position) + (UnityEngine.Vector3)Attachment0.Position);
            lr.SetPosition(1, (Attachment1.transform.parent.position) + (UnityEngine.Vector3)Attachment1.Position);
        }
    }
}