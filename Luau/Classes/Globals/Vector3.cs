using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3
{
    public readonly string ClassName = "Vector3";

    public double X, Y, Z;
    public readonly double Magnitude;
    public Vector3 unit { get { return normalize(this); } }

    public Vector3(double x = 0, double y = 0, double z = 0)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
        Magnitude = calcMagnitude(this);
    }

    // opperator overloads

    public static Vector3 operator -(Vector3 v)
    {
        return new Vector3(-v.X, -v.Y, -v.Z);
    }

    public static Vector3 operator *(float k, Vector3 a)
    {
        return new Vector3(a.X * k, a.Y * k, a.Z * k);
    }

    public static Vector3 operator *(Vector3 a, float k)
    {
        return new Vector3(a.X * k, a.Y * k, a.Z * k);
    }

    public static Vector3 operator /(Vector3 a, float k)
    {
        return new Vector3(a.X / k, a.Y / k, a.Z / k);
    }

    public static Vector3 operator +(Vector3 a, Vector3 b)
    {
        return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Vector3 operator -(Vector3 a, Vector3 b)
    {
        return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static Vector3 operator *(Vector3 a, Vector3 b)
    {
        return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
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
            (1f - (num5 + num6)) * point.X + (num7 - num12) * point.Y + (num8 + num11) * point.Z,
            (num7 + num12) * point.X + (1f - (num4 + num6)) * point.Y + (num9 - num10) * point.Z,
            (num8 - num11) * point.X + (num9 + num10) * point.Y + (1f - (num4 + num5)) * point.Z
        );
        return result;
    }

    public static Vector3 operator /(Vector3 a, Vector3 b)
    {
        return new Vector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
    }

    public static implicit operator UnityEngine.Vector3(Vector3 v)
    {
        return new UnityEngine.Vector3(Convert.ToSingle(v.X), Convert.ToSingle(v.Y), Convert.ToSingle(v.Z));
    }

    public static implicit operator Vector3(UnityEngine.Vector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public override string ToString()
    {
        return X + ", " + Y + ", " + Z;
    }

    // statics

    private static double calcMagnitude(Vector3 v)
    {
        return Math.Sqrt(Dot(v, v));
    }

    private static Vector3 normalize(Vector3 v)
    {
        double m = calcMagnitude(v);
        double nx = v.X / m, ny = v.Y / m, nz = v.Z / m;
        return new Vector3(nx, ny, nz);
    }

    public static double Dot(Vector3 a, Vector3 b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    public static Vector3 Cross(Vector3 a, Vector3 b)
    {
        return new Vector3(
            a.Y * b.Z - b.Y * a.Z,
            a.Z * b.X - b.Z * a.X,
            a.X * b.Y - b.X * a.Y
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
                Luau.returnToProto(ref dat, new object[1] { new Vector3((double)inp[0]) });
                break;
            case 2:
                Luau.returnToProto(ref dat, new object[1] { new Vector3((double)inp[0], (double)inp[1]) });
                break;
            default:
                Luau.returnToProto(ref dat, new object[1] { new Vector3((double)inp[0], (double)inp[1], (double)inp[2]) });
                break;
        }
        yield break;
    }

    public static bool isObject = false;
}
