using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class ESS
{
    public static GameObject game = GameObject.FindWithTag("DataModel");
    public static Transform np = GameObject.FindWithTag("NilContainer").transform;
    public static GameObject cam = Camera.main.gameObject;
    public static GameObject FindFirstChildG(string name)
    {
        Transform obj = game.transform;
        return obj.Find(name).gameObject;
    }
    public static Transform FindFirstChildT(string name)
    {
        Transform obj = game.transform;
        return obj.Find(name);
    }
    public static GameObject FindFirstChildGT(Transform obj,string name)
    {
        return obj.Find(name).gameObject;
    }
    public static Transform FindFirstChildTT(Transform obj,string name)
    {
        return obj.Find(name);
    }
    public static GameObject FindFirstChildGG(GameObject obj, string name)
    {
        return obj.transform.Find(name).gameObject;
    }
    public static Transform FindFirstChildTG(GameObject obj, string name)
    {
        return obj.transform.Find(name);
    }
    public static GameObject Clone(GameObject obj, Transform par = null)
    {
        if (par == null)
            par = np;
        GameObject temp = UnityEngine.Object.Instantiate(obj, par);
        temp.name = obj.name;
        return temp;
    }
    public static GameObject Clone(Transform obj, Transform par = null)
    {
        if (par == null)
            par = np;
        GameObject temp = UnityEngine.Object.Instantiate(obj.gameObject, par);
        temp.name = obj.name;
        return temp;
    }
    public static AudioSource GetSound(string name)
    {
        GameObject temp = cam.transform.Find(name).gameObject;
        if (temp)
        {
            return temp.GetComponent<AudioSource>();
        }
        else
        {
            Debug.LogWarning("Could not get sound " + name + " because it does not exist.");
            return null;
        }
    }
    public static AudioSource GetSound(GameObject src, string name)
    {
        GameObject temp = src.transform.Find(name).gameObject;
        if (temp)
        {
            return temp.GetComponent<AudioSource>();
        }
        else
        {
            Debug.LogWarning("Could not get sound " + name + " because it does not exist.");
            return null;
        }
    }
    public static void ClearAllChildren(GameObject src)
    {
        foreach (Transform child in src.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
    public static void ClearAllChildren(Transform src)
    {
        foreach (Transform child in src)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}