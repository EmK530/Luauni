using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Services
{
    public static Dictionary<string, object> List = new Dictionary<string, object>()
    {
        ["Players"] = Players.instance,
        ["ReplicatedFirst"] = ReplicatedFirst.instance,
        ["UserInputService"] = UserInputService.instance,
        ["GuiService"] = GuiService.instance
    };
}
