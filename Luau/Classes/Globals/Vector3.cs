using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3
{
    public readonly string ClassName = "Vector3";

    public double x, y, z;
    public readonly double Magnitude;
    public Vector3 unit { get { return normalize(this); } }

    public Vector3(double x = 0, double y = 0, double z = 0)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        Magnitude = calcMagnitude(this);
    }

    // opperator overloads

    public static Vector3 operator -(Vector3 v)
    {
        return new Vector3(-v.x, -v.y, -v.z);
    }

    public static Vector3 operator *(float k, Vector3 a)
    {
        return new Vector3(a.x * k, a.y * k, a.z * k);
    }

    public static Vector3 operator *(Vector3 a, float k)
    {
        return new Vector3(a.x * k, a.y * k, a.z * k);
    }

    public static Vector3 operator /(Vector3 a, float k)
    {
        return new Vector3(a.x / k, a.y / k, a.z / k);
    }

    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Vector3 operator *(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static Vector3 operator *(Quaternion rotation, Vector3 point)
    {
        float num = rotation.x * 2f;
        float num2 = rotation.y * 2f;
        float num3 = rotation.z * 2f;
        float num4 = rotation.x * num;
        float num5 = rotation.y * num2;
        float num6 = rotation.z * num3;
        float num7 = rotation.x * num2;
        float num8 = rotation.x * num3;
        float num9 = rotation.y * num3;
        float num10 = rotation.w * num;
        float num11 = rotation.w * num2;
        float num12 = rotation.w * num3;
        Vector3 result = new Vector3(
            (1f - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z,
            (num7 + num12) * point.x + (1f - (num4 + num6)) * point.y + (num9 - num10) * point.z,
            (num8 - num11) * point.x + (num9 + num10) * point.y + (1f - (num4 + num5)) * point.z
        );
        return result;
    }

    public static Vector3 operator /(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    public static implicit operator UnityEngine.Vector3(Vector3 v)
    {
        return new UnityEngine.Vector3(Convert.ToSingle(v.x), Convert.ToSingle(v.y), Convert.ToSingle(v.z));
    }

    public static implicit operator Vector3(UnityEngine.Vector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public override string ToString()
    {
        return x + ", " + y + ", " + z;
    }

    // statics

    private static double calcMagnitude(Vector3 v)
    {
        return Math.Sqrt(Dot(v, v));
    }

    private static Vector3 normalize(Vector3 v)
    {
        double m = calcMagnitude(v);
        double nx = v.x / m, ny = v.y / m, nz = v.z / m;
        return new Vector3(nx, ny, nz);
    }

    public static double Dot(Vector3 a, Vector3 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    public static Vector3 Cross(Vector3 a, Vector3 b)
    {
        return new Vector3(
            a.y * b.z - b.y * a.z,
            a.z * b.x - b.z * a.x,
            a.x * b.y - b.x * a.y
        );
    }

    // methods

    public Vector3 Lerp(Vector3 b, float t)
    {
        return (1 - t) * this + t * b;
    }

    public static IEnumerator @new(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        switch(inp.Length)
        {
            case 0:
                Luau.returnToProto(ref dat, new object[1] { new Vector3() });
                break;
            case 1:
                Luau.returnToProto(ref dat, new object[1] { new Vector3((float)inp[0]) });
                break;
            case 2:
                Luau.returnToProto(ref dat, new object[1] { new Vector3((float)inp[0], (float)inp[1]) });
                break;
            default:
                Luau.returnToProto(ref dat, new object[1] { new Vector3((float)inp[0], (float)inp[1], (float)inp[2]) });
                break;
        }
        yield break;
    }

    public static bool isObject = false;
}
