using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector2
{
    public readonly string ClassName = "Vector2";

    public double x, y, z;
    public readonly double Magnitude;
    public Vector2 unit { get { return normalize(this); } }

    public Vector2(double x = 0, double y = 0)
    {
        this.x = x;
        this.y = y;
        Magnitude = calcMagnitude(this);
    }

    // opperator overloads

    public static Vector2 operator -(Vector2 v)
    {
        return new Vector2(-v.x, -v.y);
    }

    public static Vector2 operator *(float k, Vector2 a)
    {
        return new Vector2(a.x * k, a.y * k);
    }

    public static Vector2 operator *(Vector2 a, float k)
    {
        return new Vector2(a.x * k, a.y * k);
    }

    public static Vector2 operator /(Vector2 a, float k)
    {
        return new Vector2(a.x / k, a.y / k);
    }

    public static Vector2 operator +(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x + b.x, a.y + b.y);
    }

    public static Vector2 operator -(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x - b.x, a.y - b.y);
    }

    public static Vector2 operator *(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x * b.x, a.y * b.y);
    }

    public static Vector2 operator *(Quaternion rotation, Vector2 point)
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
        Vector2 result = new Vector2(
            (1f - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z,
            (num7 + num12) * point.x + (1f - (num4 + num6)) * point.y + (num9 - num10) * point.z
        );
        return result;
    }

    public static Vector2 operator /(Vector2 a, Vector2 b)
    {
        return new Vector2(a.x / b.x, a.y / b.y);
    }

    public static implicit operator UnityEngine.Vector2(Vector2 v)
    {
        return new UnityEngine.Vector2(Convert.ToSingle(v.x), Convert.ToSingle(v.y));
    }

    public static implicit operator Vector2(UnityEngine.Vector2 v)
    {
        return new Vector2(v.x, v.y);
    }

    public override string ToString()
    {
        return x + ", " + y + ", " + z;
    }

    // statics

    private static double calcMagnitude(Vector2 v)
    {
        return Math.Sqrt(Dot(v, v));
    }

    private static Vector2 normalize(Vector2 v)
    {
        double m = calcMagnitude(v);
        double nx = v.x / m, ny = v.y / m, nz = v.z / m;
        return new Vector2(nx, ny);
    }

    public static double Dot(Vector2 a, Vector2 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    public static Vector2 Cross(Vector2 a, Vector2 b)
    {
        return new Vector2(
            a.y * b.z - b.y * a.z,
            a.z * b.x - b.z * a.x
        );
    }

    // methods

    public Vector2 Lerp(Vector2 b, float t)
    {
        return (1 - t) * this + t * b;
    }

    public static IEnumerator @new(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        switch(inp.Length)
        {
            case 0:
                Luau.returnToProto(ref dat, new object[1] { new Vector2() });
                break;
            case 1:
                Luau.returnToProto(ref dat, new object[1] { new Vector2(Convert.ToInt32(inp[0])) });
                break;
            default:
                Luau.returnToProto(ref dat, new object[1] { new Vector2(Convert.ToInt32(inp[0]), Convert.ToInt32(inp[1])) });
                break;
        }
        yield break;
    }

    public static bool isObject = false;
}
