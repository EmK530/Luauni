using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CFrame
{
    public readonly string ClassName = "CFrame";

    private double m11 = 1, m12 = 0, m13 = 0, m14 = 0;
    private double m21 = 0, m22 = 1, m23 = 0, m24 = 0;
    private double m31 = 0, m32 = 0, m33 = 1, m34 = 0;
    private const double m41 = 0, m42 = 0, m43 = 0, m44 = 1;

    // modification - make x,y,z not readonly
    public double X = 0, Y = 0, Z = 0;
    public readonly Vector3 p = new Vector3(0, 0, 0);
    public readonly Vector3 lookVector = new Vector3(0, 0, -1);
    public readonly Vector3 rightVector = new Vector3(1, 0, 0);
    public readonly Vector3 upVector = new Vector3(0, 1, 0);

    private static Vector3 RIGHT = new Vector3(1, 0, 0);
    private static Vector3 UP = new Vector3(0, 1, 0);
    private static Vector3 BACK = new Vector3(0, 0, 1);

    // constructors

    public CFrame(Vector3 pos)
    {
        m14 = pos.X;
        m24 = pos.Y;
        m34 = pos.Z;
        X = m14; Y = m24; Z = m34;
        p = new Vector3(m14, m24, m34);
        lookVector = new Vector3(-m13, -m23, -m33);
        rightVector = new Vector3(m11, m21, m31);
        upVector = new Vector3(m12, m22, m32);
    }

    public CFrame(Vector3 eye, Vector3 look)
    {
        Vector3 zAxis = (eye - look).unit;
        Vector3 xAxis = Vector3.Cross(UP, zAxis);
        Vector3 yAxis = Vector3.Cross(zAxis, xAxis);
        if (xAxis.Magnitude == 0)
        {
            if (zAxis.Y < 0)
            {
                xAxis = new Vector3(0, 0, -1);
                yAxis = new Vector3(1, 0, 0);
                zAxis = new Vector3(0, -1, 0);
            }
            else
            {
                xAxis = new Vector3(0, 0, 1);
                yAxis = new Vector3(1, 0, 0);
                zAxis = new Vector3(0, 1, 0);
            }
        }
        m11 = xAxis.X; m12 = yAxis.X; m13 = zAxis.X; m14 = eye.X;
        m21 = xAxis.Y; m22 = yAxis.Y; m23 = zAxis.Y; m24 = eye.Y;
        m31 = xAxis.Z; m32 = yAxis.Z; m33 = zAxis.Z; m34 = eye.Z;
        X = m14; Y = m24; Z = m34;
        p = new Vector3(m14, m24, m34);
        lookVector = new Vector3(-m13, -m23, -m33);
        rightVector = new Vector3(m11, m21, m31);
        upVector = new Vector3(m12, m22, m32);
    }

    public CFrame(double nx = 0, double ny = 0, double nz = 0)
    {
        m14 = nx;
        m24 = ny;
        m34 = nz;
        X = m14; Y = m24; Z = m34;
        p = new Vector3(m14, m24, m34);
        lookVector = new Vector3(-m13, -m23, -m33);
        rightVector = new Vector3(m11, m21, m31);
        upVector = new Vector3(m12, m22, m32);
    }

    public CFrame(double nx, double ny, double nz, double i, double j, double k, double w)
    {
        m14 = nx;
        m24 = ny;
        m34 = nz;
        m11 = 1 - 2 * Math.Pow(j, 2) - 2 * Math.Pow(k, 2);
        m12 = 2 * (i * j - k * w);
        m13 = 2 * (i * k + j * w);
        m21 = 2 * (i * j + k * w);
        m22 = 1 - 2 * Math.Pow(i, 2) - 2 * Math.Pow(k, 2);
        m23 = 2 * (j * k - i * w);
        m31 = 2 * (i * k - j * w);
        m32 = 2 * (j * k + i * w);
        m33 = 1 - 2 * Math.Pow(i, 2) - 2 * Math.Pow(j, 2);
        X = m14; Y = m24; Z = m34;
        p = new Vector3(m14, m24, m34);
        lookVector = new Vector3(-m13, -m23, -m33);
        rightVector = new Vector3(m11, m21, m31);
        upVector = new Vector3(m12, m22, m32);
    }

    public CFrame(double n14, double n24, double n34, double n11, double n12, double n13, double n21, double n22, double n23, double n31, double n32, double n33)
    {
        m14 = n14; m24 = n24; m34 = n34;
        m11 = n11; m12 = n12; m13 = n13;
        m21 = n21; m22 = n22; m23 = n23;
        m31 = n31; m32 = n32; m33 = n33;
        X = m14; Y = m24; Z = m34;
        p = new Vector3(m14, m24, m34);
        lookVector = new Vector3(-m13, -m23, -m33);
        rightVector = new Vector3(m11, m21, m31);
        upVector = new Vector3(m12, m22, m32);
    }

    // opperator overloads

    public static CFrame operator +(CFrame a, Vector3 b)
    {
        double[] ac = a.components();
        double x = ac[0], y = ac[1], z = ac[2], m11 = ac[3], m12 = ac[4], m13 = ac[5], m21 = ac[6], m22 = ac[7], m23 = ac[8], m31 = ac[9], m32 = ac[10], m33 = ac[11];
        return new CFrame(x + b.X, y + b.Y, z + b.Z, m11, m12, m13, m21, m22, m23, m31, m32, m33);
    }

    public static CFrame operator -(CFrame a, Vector3 b)
    {
        double[] ac = a.components();
        double x = ac[0], y = ac[1], z = ac[2], m11 = ac[3], m12 = ac[4], m13 = ac[5], m21 = ac[6], m22 = ac[7], m23 = ac[8], m31 = ac[9], m32 = ac[10], m33 = ac[11];
        return new CFrame(x - b.X, y - b.Y, z - b.Z, m11, m12, m13, m21, m22, m23, m31, m32, m33);
    }

    public static Vector3 operator *(CFrame a, Vector3 b)
    {
        double[] ac = a.components();
        double x = ac[0], y = ac[1], z = ac[2], m11 = ac[3], m12 = ac[4], m13 = ac[5], m21 = ac[6], m22 = ac[7], m23 = ac[8], m31 = ac[9], m32 = ac[10], m33 = ac[11];
        Vector3 right = new Vector3(m11, m21, m31);
        Vector3 up = new Vector3(m12, m22, m32);
        Vector3 back = new Vector3(m13, m23, m33);
        return a.p + b.X * right + b.Y * up + b.Z * back;
    }

    public static CFrame operator *(CFrame a, CFrame b)
    {
        double[] ac = a.components();
        double[] bc = b.components();
        double a14 = ac[0], a24 = ac[1], a34 = ac[2], a11 = ac[3], a12 = ac[4], a13 = ac[5], a21 = ac[6], a22 = ac[7], a23 = ac[8], a31 = ac[9], a32 = ac[10], a33 = ac[11];
        double b14 = bc[0], b24 = bc[1], b34 = bc[2], b11 = bc[3], b12 = bc[4], b13 = bc[5], b21 = bc[6], b22 = bc[7], b23 = bc[8], b31 = bc[9], b32 = bc[10], b33 = bc[11];
        double n11 = a11 * b11 + a12 * b21 + a13 * b31 + a14 * m41;
        double n12 = a11 * b12 + a12 * b22 + a13 * b32 + a14 * m42;
        double n13 = a11 * b13 + a12 * b23 + a13 * b33 + a14 * m43;
        double n14 = a11 * b14 + a12 * b24 + a13 * b34 + a14 * m44;
        double n21 = a21 * b11 + a22 * b21 + a23 * b31 + a24 * m41;
        double n22 = a21 * b12 + a22 * b22 + a23 * b32 + a24 * m42;
        double n23 = a21 * b13 + a22 * b23 + a23 * b33 + a24 * m43;
        double n24 = a21 * b14 + a22 * b24 + a23 * b34 + a24 * m44;
        double n31 = a31 * b11 + a32 * b21 + a33 * b31 + a34 * m41;
        double n32 = a31 * b12 + a32 * b22 + a33 * b32 + a34 * m42;
        double n33 = a31 * b13 + a32 * b23 + a33 * b33 + a34 * m43;
        double n34 = a31 * b14 + a32 * b24 + a33 * b34 + a34 * m44;
        double n41 = m41 * b11 + m42 * b21 + m43 * b31 + m44 * m41;
        double n42 = m41 * b12 + m42 * b22 + m43 * b32 + m44 * m42;
        double n43 = m41 * b13 + m42 * b23 + m43 * b33 + m44 * m43;
        double n44 = m41 * b14 + m42 * b24 + m43 * b34 + m44 * m44;
        return new CFrame(n14, n24, n34, n11, n12, n13, n21, n22, n23, n31, n32, n33);
    }

    public static CFrame operator *(CFrame a, Quaternion b)
    {
        double i,k,j,w;
        i = b.x; k = b.y; j = b.z; w = b.w;
        double n11 = 1 - 2 * Math.Pow(j, 2) - 2 * Math.Pow(k, 2);
        double n12 = 2 * (i * j - k * w);
        double n13 = 2 * (i * k + j * w);
        double n21 = 2 * (i * j + k * w);
        double n22 = 1 - 2 * Math.Pow(i, 2) - 2 * Math.Pow(k, 2);
        double n23 = 2 * (j * k - i * w);
        double n31 = 2 * (i * k - j * w);
        double n32 = 2 * (j * k + i * w);
        double n33 = 1 - 2 * Math.Pow(i, 2) - 2 * Math.Pow(j, 2);
        return new CFrame(a.p, new Vector3(-n13, -n23, -n33));
    }

    public static implicit operator CFrame(Vector3 v)
    {
        return new CFrame(v.X, v.Y, v.Z);
    }

    public static implicit operator Quaternion(CFrame c)
    {
        double[] q = quaternionFromCFrame(c);
        return new Quaternion((float)q[1],(float)q[2],(float)q[3],(float)q[0]);
    }

    public override string ToString()
    {
        return System.String.Join(", ", components());
    }

    // private static functions

    private static Vector3 vectorAxisAngle(Vector3 n, Vector3 v, double t)
    {
        n = n.unit;
        return v * Math.Cos(t) + Vector3.Dot(v, n) * n * (1 - Math.Cos(t)) + Vector3.Cross(n, v) * Math.Sin(t);
    }

    private static double getDeterminant(CFrame a)
    {
        double[] ac = a.components();
        double a14 = ac[0], a24 = ac[1], a34 = ac[2], a11 = ac[3], a12 = ac[4], a13 = ac[5], a21 = ac[6], a22 = ac[7], a23 = ac[8], a31 = ac[9], a32 = ac[10], a33 = ac[11];
        double det = (a11 * a22 * a33 * m44 + a11 * a23 * a34 * m42 + a11 * a24 * a32 * m43
                + a12 * a21 * a34 * m43 + a12 * a23 * a31 * m44 + a12 * a24 * a33 * m41
                + a13 * a21 * a32 * m44 + a13 * a22 * a34 * m41 + a13 * a24 * a31 * m42
                + a14 * a21 * a33 * m42 + a14 * a22 * a31 * m43 + a14 * a23 * a32 * m41
                - a11 * a22 * a34 * m43 - a11 * a23 * a32 * m44 - a11 * a24 * a33 * m42
                - a12 * a21 * a33 * m44 - a12 * a23 * a34 * m41 - a12 * a24 * a31 * m43
                - a13 * a21 * a34 * m42 - a13 * a22 * a31 * m44 - a13 * a24 * a32 * m41
                - a14 * a21 * a32 * m43 - a14 * a22 * a33 * m41 - a14 * a23 * a31 * m42);
        return det;
    }

    private static CFrame invert4x4(CFrame a)
    {
        double[] ac = a.components();
        double a14 = ac[0], a24 = ac[1], a34 = ac[2], a11 = ac[3], a12 = ac[4], a13 = ac[5], a21 = ac[6], a22 = ac[7], a23 = ac[8], a31 = ac[9], a32 = ac[10], a33 = ac[11];
        double det = getDeterminant(a);
        if (det == 0) { return a; }
        double b11 = (a22 * a33 * m44 + a23 * a34 * m42 + a24 * a32 * m43 - a22 * a34 * m43 - a23 * a32 * m44 - a24 * a33 * m42) / det;
        double b12 = (a12 * a34 * m43 + a13 * a32 * m44 + a14 * a33 * m42 - a12 * a33 * m44 - a13 * a34 * m42 - a14 * a32 * m43) / det;
        double b13 = (a12 * a23 * m44 + a13 * a24 * m42 + a14 * a22 * m43 - a12 * a24 * m43 - a13 * a22 * m44 - a14 * a23 * m42) / det;
        double b14 = (a12 * a24 * a33 + a13 * a22 * a34 + a14 * a23 * a32 - a12 * a23 * a34 - a13 * a24 * a32 - a14 * a22 * a33) / det;
        double b21 = (a21 * a34 * m43 + a23 * a31 * m44 + a24 * a33 * m41 - a21 * a33 * m44 - a23 * a34 * m41 - a24 * a31 * m43) / det;
        double b22 = (a11 * a33 * m44 + a13 * a34 * m41 + a14 * a31 * m43 - a11 * a34 * m43 - a13 * a31 * m44 - a14 * a33 * m41) / det;
        double b23 = (a11 * a24 * m43 + a13 * a21 * m44 + a14 * a23 * m41 - a11 * a23 * m44 - a13 * a24 * m41 - a14 * a21 * m43) / det;
        double b24 = (a11 * a23 * a34 + a13 * a24 * a31 + a14 * a21 * a33 - a11 * a24 * a33 - a13 * a21 * a34 - a14 * a23 * a31) / det;
        double b31 = (a21 * a32 * m44 + a22 * a34 * m41 + a24 * a31 * m42 - a21 * a34 * m42 - a22 * a31 * m44 - a24 * a32 * m41) / det;
        double b32 = (a11 * a34 * m42 + a12 * a31 * m44 + a14 * a32 * m41 - a11 * a32 * m44 - a12 * a34 * m41 - a14 * a31 * m42) / det;
        double b33 = (a11 * a22 * m44 + a12 * a24 * m41 + a14 * a21 * m42 - a11 * a24 * m42 - a12 * a21 * m44 - a14 * a22 * m41) / det;
        double b34 = (a11 * a24 * a32 + a12 * a21 * a34 + a14 * a22 * a31 - a11 * a22 * a34 - a12 * a24 * a31 - a14 * a21 * a32) / det;
        double b41 = (a21 * a33 * m42 + a22 * a31 * m43 + a23 * a32 * m41 - a21 * a32 * m43 - a22 * a33 * m41 - a23 * a31 * m42) / det;
        double b42 = (a11 * a32 * m43 + a12 * a33 * m41 + a13 * a31 * m42 - a11 * a33 * m42 - a12 * a31 * m43 - a13 * a32 * m41) / det;
        double b43 = (a11 * a23 * m42 + a12 * a21 * m43 + a13 * a22 * m41 - a11 * a22 * m43 - a12 * a23 * m41 - a13 * a21 * m42) / det;
        double b44 = (a11 * a22 * a33 + a12 * a23 * a31 + a13 * a21 * a32 - a11 * a23 * a32 - a12 * a21 * a33 - a13 * a22 * a31) / det;
        return new CFrame(b14, b24, b34, b11, b12, b13, b21, b22, b23, b31, b32, b33);
    }

    public static double[] quaternionFromCFrame(CFrame a)
    {
        double[] ac = a.components();
        double mx = ac[0], my = ac[1], mz = ac[2], m11 = ac[3], m12 = ac[4], m13 = ac[5], m21 = ac[6], m22 = ac[7], m23 = ac[8], m31 = ac[9], m32 = ac[10], m33 = ac[11];
        double trace = m11 + m22 + m33;
        double w = 1, i = 0, j = 0, k = 0;
        if (trace > 0)
        {
            double s = Math.Sqrt(1 + trace);
            double r = 0.5f / s;
            w = s * 0.5f; i = (m32 - m23) * r; j = (m13 - m31) * r; k = (m21 - m12) * r;
        }
        else
        {
            double big = Math.Max(Math.Max(m11, m22), m33);
            if (big == m11)
            {
                double s = Math.Sqrt(1 + m11 - m22 - m33);
                double r = 0.5f / s;
                w = (m32 - m23) * r; i = 0.5f * s; j = (m21 + m12) * r; k = (m13 + m31) * r;
            }
            else if (big == m22)
            {
                double s = Math.Sqrt(1 - m11 + m22 - m33);
                double r = 0.5f / s;
                w = (m13 - m31) * r; i = (m21 + m12) * r; j = 0.5f * s; k = (m32 + m23) * r;
            }
            else if (big == m33)
            {
                double s = Math.Sqrt(1 - m11 - m22 + m33);
                double r = 0.5f / s;
                w = (m21 - m12) * r; i = (m13 + m31) * r; j = (m32 + m23) * r; k = 0.5f * s;
            }
        }
        return new double[] { w, i, j, k };
    }

    private static CFrame lerpinternal(CFrame a, CFrame b, double t)
    {
        CFrame cf = a.inverse() * b;
        double[] q = quaternionFromCFrame(cf);
        double w = q[0], i = q[1], j = q[2], k = q[3];
        double theta = Math.Acos(w) * 2;
        Vector3 v = new Vector3(i, j, k);
        Vector3 p = a.p.Lerp(b.p, t);
        if (theta != 0)
        {
            CFrame r = a * fromAxisAngle(v, theta * t);
            return (r - r.p) + p;
        }
        else
        {
            return (a - a.p) + p;
        }
    }

    // public static functions

    public static CFrame fromAxisAngle(Vector3 axis, double theta)
    {
        Vector3 r = vectorAxisAngle(axis, RIGHT, theta);
        Vector3 u = vectorAxisAngle(axis, UP, theta);
        Vector3 b = vectorAxisAngle(axis, BACK, theta);
        return new CFrame(0, 0, 0, r.X, u.X, b.X, r.Y, u.Y, b.Y, r.Z, u.Z, b.Z);
    }

    public static CFrame Angles(double x, double y, double z)
    {
        CFrame cfx = fromAxisAngle(RIGHT, x);
        CFrame cfy = fromAxisAngle(UP, y);
        CFrame cfz = fromAxisAngle(BACK, z);
        return cfx * cfy * cfz;
    }

    public static CFrame fromEulerAnglesXYZ(double x, double y, double z)
    {
        return Angles(x, y, z);
    }

    public static Vector3 ToVector(CFrame c)
    {
        return new Vector3(c.X,c.Y,c.Z);
    }

    public static void SetCFrame(Transform t, CFrame c)
    {
        double[] q = quaternionFromCFrame(c);
        t.SetPositionAndRotation(new UnityEngine.Vector3((float)c.X,(float)c.Y,(float)c.Z),new Quaternion((float)q[1],(float)q[2],(float)q[3],(float)q[0]));
    }

    // methods

    public CFrame inverse()
    {
        return invert4x4(this);
    }

    public CFrame lerp(CFrame cf2, double t)
    {
        return lerpinternal(this, cf2, t);
    }

    public CFrame toWorldSpace(CFrame cf2)
    {
        return this * cf2;
    }

    public CFrame toObjectSpace(CFrame cf2)
    {
        return this.inverse() * cf2;
    }

    public Vector3 pointToWorldSpace(Vector3 v)
    {
        return this * v;
    }

    public Vector3 pointToObjectSpace(Vector3 v)
    {
        return this.inverse() * v;
    }

    public Vector3 vectorToWorldSpace(Vector3 v)
    {
        return (this - this.p) * v;
    }

    public Vector3 vectorToObjectSpace(Vector3 v)
    {
        return (this - this.p).inverse() * v;
    }

    public double[] components()
    {
        return new double[] { m14, m24, m34, m11, m12, m13, m21, m22, m23, m31, m32, m33 };
    }

    public double[] toEulerAnglesXYZ()
    {
        double x = Math.Atan2(-m23, m33);
        double y = Math.Asin(m13);
        double z = Math.Atan2(-m12, m11);
        return new double[] { x, y, z };
    }

    public static IEnumerator @new(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        switch (inp.Length)
        {
            case 0:
                Luau.returnToProto(ref dat, new object[1] { new CFrame() });
                yield break;
            case 1:
                Luau.returnToProto(ref dat, new object[1] { new CFrame((Vector3)inp[0]) });
                yield break;
            case 2:
                Luau.returnToProto(ref dat, new object[1] { new CFrame((Vector3)inp[0], (Vector3)inp[1]) });
                yield break;
            case 3:
                Luau.returnToProto(ref dat, new object[1] { new CFrame((double)inp[0], (double)inp[1], (double)inp[2]) });
                yield break;
            case 7:
                Luau.returnToProto(ref dat, new object[1] { new CFrame((double)inp[0], (double)inp[1], (double)inp[2], (double)inp[3], (double)inp[4], (double)inp[5], (double)inp[6]) });
                yield break;
            case 11:
                Luau.returnToProto(ref dat, new object[1] { new CFrame((double)inp[0], (double)inp[1], (double)inp[2], (double)inp[3], (double)inp[4], (double)inp[5], (double)inp[6], (double)inp[7], (double)inp[8], (double)inp[9], (double)inp[10], (double)inp[11]) });
                yield break;
            default:
                Logging.Error($"No constructor found for CFrame with argument count {inp.Length}", "Luauni:CFrame");
                dat.initiator.globalErrored = true;
                yield break;
        }
    }

    public static bool isObject = false;
}