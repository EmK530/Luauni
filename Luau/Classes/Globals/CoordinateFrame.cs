using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateFrame
{
    public readonly string ClassName = "CoordinateFrame";

    public readonly Vector3 Position;
    public readonly Quaternion Rotation;

    public CoordinateFrame(float x, float y, float z)
    {
        this.Position = new Vector3(x, y, z);
        this.Rotation = Quaternion.identity;
    }

    public CoordinateFrame(float x, float y, float z, Quaternion rotation)
    {
        this.Position = new Vector3(x, y, z);
        this.Rotation = rotation;
    }

    public CoordinateFrame(Vector3 position, Quaternion rotation)
    {
        this.Position = position;
        this.Rotation = rotation;
    }

    public static IEnumerator @new(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        switch (inp.Length)
        {
            case 0:
                Luau.returnToProto(ref dat, new object[1] { new CoordinateFrame(0, 0, 0) });
                break;
            case 3:
                Luau.returnToProto(ref dat, new object[1] { new CoordinateFrame(Convert.ToSingle(inp[0]), Convert.ToSingle(inp[1]), Convert.ToSingle(inp[2])) });
                break;
            default:
                Logging.Error("Unsupported number of CFrame.new arguments: " + inp.Length);
                break;
        }
        yield break;
    }

    public void ApplyToTransform(Transform transform)
    {
        transform.position = Position;
        transform.rotation = Rotation;
    }

    public CoordinateFrame Transform(CoordinateFrame other)
    {
        Vector3 newPosition = Position + Rotation * other.Position;
        Quaternion newRotation = Rotation * other.Rotation;
        return new CoordinateFrame(newPosition, newRotation);
    }

    public static IEnumerator Angles(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Quaternion rotation = Quaternion.identity;
        switch (inp.Length)
        {
            case 1:
                rotation = Quaternion.Euler(Convert.ToSingle(inp[0]) * Mathf.Rad2Deg, 0f, 0f);
                break;
            case 2:
                rotation = Quaternion.Euler(Convert.ToSingle(inp[0]) * Mathf.Rad2Deg, Convert.ToSingle(inp[1]) * Mathf.Rad2Deg + 180f, 0f);
                break;
            case 3:
                rotation = Quaternion.Euler(Convert.ToSingle(inp[0]) * Mathf.Rad2Deg, Convert.ToSingle(inp[1]) * Mathf.Rad2Deg + 180f, Convert.ToSingle(inp[2]) * Mathf.Rad2Deg);
                break;
        }
        Luau.returnToProto(ref dat, new object[1] { new CoordinateFrame(new Vector3(), rotation) });
        yield break;
    }

    public static CoordinateFrame operator *(CoordinateFrame a, CoordinateFrame b)
    {
        return a.Transform(b);
    }
}
