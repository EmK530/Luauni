using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskScheduler : MonoBehaviour
{
    public static TaskScheduler instance;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            StartCoroutine(MainLoopHandler());
        }
        else
        {
            Debug.LogWarning($"Destroying duplicate singleton '{name}'");
            DestroyImmediate(gameObject);
        }
    }

    List<SClosure> firstRuns = new List<SClosure>();
    List<SClosure> incompleteClosures = new List<SClosure>();
    public int lastRanCount = 0;
    public List<SClosure> hybridSpawns = new List<SClosure>();
    public List<SClosure> taskSpawns = new List<SClosure>();

    public void InitiateScript(ref SClosure closure)
    {
        firstRuns.Add(closure);
    }

    float timeSinceHybrid = 0f;
    float constant = 1 / 30f;

    public IEnumerator MainLoopHandler()
    {
        yield return Misc.ExecuteCoroutine(MainLoop());
    }

    public IEnumerator MainLoop()
    {
        while (true)
        {
            yield return null;
            lastRanCount = 0;
            SClosure[] execute = firstRuns.ToArray();
            firstRuns.Clear();
            foreach (SClosure c in execute)
            {
                lastRanCount++;
                yield return Misc.ExecuteCoroutine(Luauni.Execute(c));
                if (!c.complete && c.yielded)
                {
                    incompleteClosures.Add(c);
                }
            }
            yield return Misc.ExecuteCoroutine(RunService.RenderStepped._fire(new object[1] { Convert.ToDouble(Time.unscaledDeltaTime) }));
            bool shouldRunHybrid = false;
            timeSinceHybrid += Time.unscaledDeltaTime;
            if (timeSinceHybrid >= constant)
            {
                timeSinceHybrid -= constant;
                shouldRunHybrid = true;
            }
            if (shouldRunHybrid)
            {
                SClosure[] hybrids = hybridSpawns.ToArray();
                hybridSpawns.Clear();
                foreach (SClosure c in hybrids)
                {
                    lastRanCount++;
                    yield return Misc.ExecuteCoroutine(Luauni.Execute(c));
                    if (!c.complete && c.yielded)
                    {
                        incompleteClosures.Add(c);
                    }
                }
                yield return Misc.ExecuteCoroutine(RunHybrid());
            }
            SClosure[] tasks = taskSpawns.ToArray();
            taskSpawns.Clear();
            foreach (SClosure c in tasks)
            {
                lastRanCount++;
                yield return Misc.ExecuteCoroutine(Luauni.Execute(c));
                if (!c.complete && c.yielded)
                {
                    incompleteClosures.Add(c);
                }
            }
            yield return Misc.ExecuteCoroutine(RunTask());
            yield return Misc.ExecuteCoroutine(RunService.Heartbeat._fire(new object[1] { Convert.ToDouble(Time.unscaledDeltaTime) }));
            //clean list
            SClosure[] safeCopy = incompleteClosures.ToArray();
            incompleteClosures.Clear();
            foreach(SClosure closure in safeCopy)
            {
                if (!closure.complete)
                {
                    incompleteClosures.Add(closure);
                }
            }
        }
    }

    IEnumerator RunHybrid()
    {
        SClosure[] safeCopy = incompleteClosures.ToArray();
        foreach(SClosure closure in safeCopy)
        {
            if (!closure.complete && closure.type == YieldType.Hybrid && Time.realtimeSinceStartupAsDouble >= closure.resumeAt)
            {
                Luau.returnToProto(ref closure.yieldReturnTo, new object[1] { Time.realtimeSinceStartupAsDouble - closure.yieldStart });
                lastRanCount++;
                yield return Misc.ExecuteCoroutine(Luauni.Execute(closure));
            }
        }
    }

    IEnumerator RunTask()
    {
        SClosure[] safeCopy = incompleteClosures.ToArray();
        foreach (SClosure closure in safeCopy)
        {
            if (!closure.complete && closure.type == YieldType.Task && Time.realtimeSinceStartupAsDouble >= closure.resumeAt)
            {
                Luau.returnToProto(ref closure.yieldReturnTo, new object[1] { Time.realtimeSinceStartupAsDouble - closure.yieldStart });
                lastRanCount++;
                yield return Misc.ExecuteCoroutine(Luauni.Execute(closure));
            }
        }
    }

    public IEnumerator Spawn(SClosure closure)
    {
        lastRanCount++;
        yield return Misc.ExecuteCoroutine(Luauni.Execute(closure));
        if (!closure.complete && closure.yielded)
        {
            incompleteClosures.Add(closure);
        }
        yield break;
    }

    public void SpawnHybrid(SClosure closure)
    {
        hybridSpawns.Add(closure);
    }

    public void SpawnTask(SClosure closure)
    {
        taskSpawns.Add(closure);
    }
}
