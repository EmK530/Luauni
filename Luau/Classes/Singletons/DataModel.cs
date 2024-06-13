using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataModel : MonoBehaviour
{
    public static double CreatorId = 71510352;
    public static Enum.CreatorType CreatorType = Enum.CreatorType.User;
    public static double GameId = 0;
    public static Enum.Genre Genre = Enum.Genre.All;
    public static string JobId = "";
    public static double PlaceId = 0;
    public static double PlaceVersion = 0;
    public static string PrivateServerId = "";
    public static double PrivateServerOwnerId = 0;
    public static Workspace Workspace = Workspace.instance;

    public static IEnumerator GetService(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        string key = (string)inp[1];
        if (!Services.List.ContainsKey(key))
        {
            dat.initiator.globalErrored = true; Logging.Error($"'{key}' is not a valid Service name", "DataModel:GetService"); yield break;
        }
        Luau.returnToProto(ref dat, new object[1] { Services.List[key].GetType() });
        yield break;
    }

    public static DataModel instance;
    public static bool isObject = true;
    public static GameObject source;
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            source = gameObject;
        }
        else
        {
            Debug.LogWarning($"Destroying duplicate singleton '{name}'");
            DestroyImmediate(gameObject);
        }
    }
}
