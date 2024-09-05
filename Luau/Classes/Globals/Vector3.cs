using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Enum;

public class Vector3
{
    public readonly string ClassName = "Vector3";
    public static bool isObject = false;

    public readonly float X, Y, Z;
    public override string ToString() => $"{X}, {Y}, {Z}";

    public double Magnitude
    {
        get
        {
            float product = _dot(this);
            double magnitude = Math.Sqrt(product);
            return magnitude;
        }
    }
    public double magnitude
    {
        get { return Magnitude; }
    }

    public Vector3 Unit
    {
        get { return this / Magnitude; }
    }

    public Vector3(float x = 0, float y = 0, float z = 0)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3(params float[] coords)
    {
        X = coords.Length > 0 ? coords[0] : 0;
        Y = coords.Length > 1 ? coords[1] : 0;
        Z = coords.Length > 2 ? coords[2] : 0;
    }

    public static Vector3 FromAxis(Axis axis)
    {
        float[] coords = new float[3] { 0f, 0f, 0f };

        int index = (int)axis;
        coords[index] = 1f;

        return new Vector3(coords);
    }

    public static Vector3 FromNormalId(NormalId normalId)
    {
        float[] coords = new float[3] { 0f, 0f, 0f };

        int index = (int)normalId;
        coords[index % 3] = (index > 2 ? -1f : 1f);

        return new Vector3(coords);
    }

    private delegate Vector3 Operator(Vector3 a, Vector3 b);

    private static Vector3 UpcastDoubleOp(Vector3 vec, double num, Operator upcast)
    {
        Vector3 numVec = new Vector3((float)num, (float)num, (float)num);
        return upcast(vec, numVec);
    }

    private static Vector3 UpcastDoubleOp(double num, Vector3 vec, Operator upcast)
    {
        Vector3 numVec = new Vector3((float)num, (float)num, (float)num);
        return upcast(numVec, vec);
    }

    private static readonly Operator add = (a, b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    private static readonly Operator sub = (a, b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    private static readonly Operator mul = (a, b) => new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    private static readonly Operator div = (a, b) => new Vector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);

    public static Vector3 operator +(Vector3 a, Vector3 b) => add(a, b);
    public static Vector3 operator +(Vector3 v, double n) => UpcastDoubleOp(v, n, add);
    public static Vector3 operator +(double n, Vector3 v) => UpcastDoubleOp(n, v, add);

    public static Vector3 operator -(Vector3 a, Vector3 b) => sub(a, b);
    public static Vector3 operator -(Vector3 v, double n) => UpcastDoubleOp(v, n, sub);
    public static Vector3 operator -(double n, Vector3 v) => UpcastDoubleOp(n, v, sub);

    public static Vector3 operator *(Vector3 a, Vector3 b) => mul(a, b);
    public static Vector3 operator *(Vector3 v, double n) => UpcastDoubleOp(v, n, mul);
    public static Vector3 operator *(double n, Vector3 v) => UpcastDoubleOp(n, v, mul);

    public static Vector3 operator /(Vector3 a, Vector3 b) => div(a, b);
    public static Vector3 operator /(Vector3 v, double n) => UpcastDoubleOp(v, n, div);
    public static Vector3 operator /(double n, Vector3 v) => UpcastDoubleOp(n, v, div);

    public static Vector3 operator -(Vector3 v) => new Vector3(-v.X, -v.Y, -v.Z);

    public static implicit operator UnityEngine.Vector3(Vector3 v)
    {
        return new UnityEngine.Vector3(v.X, v.Y, v.Z);
    }

    public static implicit operator Vector3(UnityEngine.Vector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }

    public static readonly Vector3 zero = new Vector3(0, 0, 0);
    public static readonly Vector3 one = new Vector3(1, 1, 1);

    public static readonly Vector3 xAxis = new Vector3(1, 0, 0);
    public static readonly Vector3 yAxis = new Vector3(0, 1, 0);
    public static readonly Vector3 zAxis = new Vector3(0, 0, 1);

    public static Vector3 Right => xAxis;
    public static Vector3 Up => yAxis;
    public static Vector3 Back => zAxis;

    public float _dot(Vector3 other)
    {
        float dotX = X * other.X;
        float dotY = Y * other.Y;
        float dotZ = Z * other.Z;

        return dotX + dotY + dotZ;
    }

    public static IEnumerator Dot(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        Vector3 v1 = (Vector3)inp[0];
        Vector3 v2 = (Vector3)inp[1];
        Luau.returnToProto(ref dat, new object[1] { (v1.X * v2.X) + (v1.Y * v2.Y) + (v1.Z * v2.Z) });
        yield break;
    }

    public Vector3 Cross(Vector3 other)
    {
        float crossX = Y * other.Z - other.Y * Z;
        float crossY = Z * other.X - other.Z * X;
        float crossZ = X * other.Y - other.X * Y;

        return new Vector3(crossX, crossY, crossZ);
    }

    public Vector3 Lerp(Vector3 other, float t)
    {
        return this + (other - this) * t;
    }

    public bool IsClose(Vector3 other, float epsilon = 0.0f)
    {
        return (other - this).Magnitude <= Math.Abs(epsilon);
    }

    public override int GetHashCode()
    {
        int hash = X.GetHashCode()
                 ^ Y.GetHashCode()
                 ^ Z.GetHashCode();

        return hash;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Vector3 other))
            return false;

        if (!X.Equals(other.X))
            return false;

        if (!Y.Equals(other.Y))
            return false;

        if (!Z.Equals(other.Z))
            return false;

        return true;
    }

    public static IEnumerator @new(CallData dat)
    {
        dynamic[] inp = Luau.getAllArgs(ref dat);
        switch (inp.Length)
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
}