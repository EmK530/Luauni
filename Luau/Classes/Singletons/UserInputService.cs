using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInputService : MonoBehaviour
{
    public static UserInputService instance;
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
