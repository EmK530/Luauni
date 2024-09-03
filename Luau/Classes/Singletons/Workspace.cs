using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workspace : MonoBehaviour
{
    public string Name
    {
        get { return name; }
        set
        {
            name = value;
        }
    }

    public static readonly List<Type> _inherits = new List<Type>()
    {
        typeof(Instance)
    };

    public static double DistributedGameTime
    {
        get { return Time.realtimeSinceStartupAsDouble; }
    }
    private static double _gravity = 196.2;
    public static double Gravity
    {
        get { return _gravity; }
        set
        {
            _gravity = value;
            Physics.gravity = new Vector3(0, (float)value, 0);
        }
    }
    public static Camera CurrentCamera;

    public static IEnumerator FindPartsInRegion3WithWhiteList(CallData dat) {
        object[] inp = Luau.getAllArgs(ref dat);
        Logging.Debug("FindPartsInRegion3WithWhiteList call: " + inp.ToString(), "Workspace");
        Region3 region = (Region3)inp[1];
        RaycastHit[] hits = Physics.BoxCastAll((region.max + region.min)/2, (region.max - region.min)/2, UnityEngine.Vector3.forward);
        List<object> parts = new List<object>();
        foreach (var hit in hits) {
            if (hit.transform.gameObject.tag == "Part" || hit.transform.gameObject.tag == "MeshPart") {
                parts.Add(Misc.TryGetType(hit.transform));
            }
        }
        Luau.returnToProto(ref dat, new object[1] { parts.ToArray() });
        yield break;
    }
    
    public static IEnumerator FindPartOnRayWithIgnoreList(CallData dat) {
        object[] inp = Luau.getAllArgs(ref dat);

        Logging.Debug("FindPartOnRayWithIgnoreList call: " + inp.ToString(), "Workspace");
        Ray ray = (Ray)inp[1];
        RaycastHit hit;
        if (Physics.Raycast(ray.Origin, ray.Direction, out hit)) {
            Luau.returnToProto(ref dat, new object[3] { Misc.TryGetType(hit.transform), (Vector3)hit.point, (Vector3)hit.normal });
            yield break;
        } else {
            Luau.returnToProto(ref dat, new object[3] { null, new Vector3(0, 0, 0), new Vector3(0, 0, 0) });
            yield break;
        }
    }

    public static Workspace instance;
    public static bool isObject = true;
    public static GameObject source;
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            source = gameObject;
            CurrentCamera = transform.Find("Camera").GetComponent<Camera>();
        }
        else
        {
            Debug.LogWarning($"Destroying duplicate singleton '{name}'");
            DestroyImmediate(gameObject);
        }
    }
}
