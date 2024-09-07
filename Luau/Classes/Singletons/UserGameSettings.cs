using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserGameSettings : MonoBehaviour
{
    public static Enum.SavedQualitySetting SavedQualityLevel = Enum.SavedQualitySetting.QualityLevel10;

    public static UserGameSettings instance;
    public static bool isObject = false;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning($"Destroying duplicate singleton '{name}'");
            DestroyImmediate(gameObject);
        }
    }
}
