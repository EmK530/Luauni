using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Singletons
{
    public static Dictionary<string, object> Services = new Dictionary<string, object>()
    {
        ["ReplicatedFirst"] = ReplicatedFirst.instance
    };
}
