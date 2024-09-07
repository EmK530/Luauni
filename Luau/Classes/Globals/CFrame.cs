using System;
using System.Collections;
using UnityEngine;

public class CFrame
{
    public readonly string ClassName = "CFrame";
    public static bool isObject = false;

    private readonly float m14, m24, m34;

    private readonly float m11 = 1, m12, m13;
    private readonly float m21, m22 = 1, m23;
    private readonly float m31, m32, m33 = 1;

    private const float m41 = 0, m42 = 0, m43 = 0, m44 = 1;

    public float X => m14;
    public float Y => m24;
    public float Z => m34;

    public Vector3 Position => new Vector3(X, Y, Z);
    public Vector3 p => new Vector3(X, Y, Z);
    public CFrame Rotation => (this - Position);

    public Vector3 XVector => new Vector3(m11, m21, m31);
    public Vector3 YVector => new Vector3(m12, m22, m32);
    public Vector3 ZVector => new Vector3(m13, m23, m33);

    public Vector3 RightVector => XVector;
    public Vector3 rightVector => XVector;
    public Vector3 UpVector => YVector;
    public Vector3 upVector => YVector;
    public Vector3 LookVector => -ZVector;
    public Vector3 lookVector => -ZVector;

    public Vector3 ColumnX => new Vector3(m11, m12, m13);
    public Vector3 ColumnY => new Vector3(m21, m22, m23);
    public Vector3 ColumnZ => new Vector3(m31, m32, m33);

    public static readonly CFrame identity = new CFrame();

    public override int GetHashCode()
    {
        var components = GetComponents();
        int hashCode = 0;

        foreach (float component in components)
            hashCode ^= component.GetHashCode();

        return hashCode;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is CFrame other))
            return false;

        var compA = GetComponents();
        var compB = other.GetComponents();

        for (int i = 0; i < 12; i++)
        {
            float a = compA[i],
                  b = compB[i];

            if (a.Equals(b))
                continue;

            return false;
        }

        return true;
    }

    public CFrame()
    {
        m14 = 0;
        m24 = 0;
        m34 = 0;
    }

    public CFrame(Vector3 pos)
    {
        m14 = pos.X;
        m24 = pos.Y;
        m34 = pos.Z;
    }

    public CFrame(float nx = 0, float ny = 0, float nz = 0)
    {
        m14 = nx;
        m24 = ny;
        m34 = nz;
    }

    public CFrame(Vector3 eye, Vector3 look)
    {
        Vector3 zAxis = (eye - look).Unit,
                xAxis = Vector3.yAxis.Cross(zAxis),
                yAxis = zAxis.Cross(xAxis);

        if (xAxis.Magnitude == 0)
        {
            xAxis = Vector3.zAxis;
            yAxis = Vector3.xAxis;
            zAxis = Vector3.yAxis;
        }

        m11 = xAxis.X; m12 = yAxis.X; m13 = zAxis.X; m14 = eye.X;
        m21 = xAxis.Y; m22 = yAxis.Y; m23 = zAxis.Y; m24 = eye.Y;
        m31 = xAxis.Z; m32 = yAxis.Z; m33 = zAxis.Z; m34 = eye.Z;
    }

    public CFrame(float nx, float ny, float nz, float i, float j, float k, float w)
    {
        float ii = i * i,
              jj = j * j,
              kk = k * k;

        m14 = nx;
        m24 = ny;
        m34 = nz;

        m11 = 1 - 2 * jj - 2 * kk;
        m12 = 2 * (i * j - k * w);
        m13 = 2 * (i * k + j * w);

        m21 = 2 * (i * j + k * w);
        m22 = 1 - 2 * ii - 2 * kk;
        m23 = 2 * (j * k - i * w);

        m31 = 2 * (i * k - j * w);
        m32 = 2 * (j * k + i * w);
        m33 = 1 - 2 * ii - 2 * jj;
    }

    public CFrame(float n14, float n24, float n34, float n11, float n12, float n13, float n21, float n22, float n23, float n31, float n32, float n33)
    {
        m14 = n14; m24 = n24; m34 = n34;
        m11 = n11; m12 = n12; m13 = n13;
        m21 = n21; m22 = n22; m23 = n23;
        m31 = n31; m32 = n32; m33 = n33;
    }

    public CFrame(params float[] comp)
    {
        m14 = comp[0]; m24 = comp[1]; m34 = comp[2];
        m11 = comp[3]; m12 = comp[4]; m13 = comp[5];
        m21 = comp[6]; m22 = comp[7]; m23 = comp[8];
        m31 = comp[9]; m32 = comp[10]; m33 = comp[11];
    }

    public CFrame(Vector3 pos, Vector3 vX, Vector3 vY, Vector3 vZ = null)
    {
        if (vZ == null)
            vZ = vX.Cross(vY);
        m14 = pos.X; m24 = pos.Y; m34 = pos.Z;
        m11 = vX.X; m12 = vX.Y; m13 = vX.Z;
        m21 = vY.X; m22 = vY.Y; m23 = vY.Z;
        m31 = vZ.X; m32 = vZ.Y; m33 = vZ.Z;
    }

    public static CFrame operator +(CFrame a, Vector3 b)
    {
        float[] ac = a.GetComponents();
        float x = ac[0], y = ac[1], z = ac[2],
              m11 = ac[3], m12 = ac[4], m13 = ac[5],
              m21 = ac[6], m22 = ac[7], m23 = ac[8],
              m31 = ac[9], m32 = ac[10], m33 = ac[11];
        return new CFrame(x + b.X, y + b.Y, z + b.Z, m11, m12, m13, m21, m22, m23, m31, m32, m33);
    }

    public static CFrame operator -(CFrame a, Vector3 b)
    {
        float[] ac = a.GetComponents();
        float x = ac[0], y = ac[1], z = ac[2],
              m11 = ac[3], m12 = ac[4], m13 = ac[5],
              m21 = ac[6], m22 = ac[7], m23 = ac[8],
              m31 = ac[9], m32 = ac[10], m33 = ac[11];
        return new CFrame(x - b.X, y - b.Y, z - b.Z, m11, m12, m13, m21, m22, m23, m31, m32, m33);
    }

    public static Vector3 operator *(CFrame a, Vector3 b)
    {
        float[] ac = a.GetComponents();
        float m11 = ac[3], m12 = ac[4], m13 = ac[5],
              m21 = ac[6], m22 = ac[7], m23 = ac[8],
              m31 = ac[9], m32 = ac[10], m33 = ac[11];
        var up = new Vector3(m12, m22, m32);
        var back = new Vector3(m13, m23, m33);
        var right = new Vector3(m11, m21, m31);
        return a.Position + b.X * right + b.Y * up + b.Z * back;
    }

    public static CFrame operator *(CFrame a, CFrame b)
    {
        float[] ac = a.GetComponents();
        float[] bc = b.GetComponents();
        float a14 = ac[0], a24 = ac[1], a34 = ac[2],
              a11 = ac[3], a12 = ac[4], a13 = ac[5],
              a21 = ac[6], a22 = ac[7], a23 = ac[8],
              a31 = ac[9], a32 = ac[10], a33 = ac[11];
        float b14 = bc[0], b24 = bc[1], b34 = bc[2],
              b11 = bc[3], b12 = bc[4], b13 = bc[5],
              b21 = bc[6], b22 = bc[7], b23 = bc[8],
              b31 = bc[9], b32 = bc[10], b33 = bc[11];
        float n11 = a11 * b11 + a12 * b21 + a13 * b31 + a14 * m41;
        float n12 = a11 * b12 + a12 * b22 + a13 * b32 + a14 * m42;
        float n13 = a11 * b13 + a12 * b23 + a13 * b33 + a14 * m43;
        float n14 = a11 * b14 + a12 * b24 + a13 * b34 + a14 * m44;
        float n21 = a21 * b11 + a22 * b21 + a23 * b31 + a24 * m41;
        float n22 = a21 * b12 + a22 * b22 + a23 * b32 + a24 * m42;
        float n23 = a21 * b13 + a22 * b23 + a23 * b33 + a24 * m43;
        float n24 = a21 * b14 + a22 * b24 + a23 * b34 + a24 * m44;
        float n31 = a31 * b11 + a32 * b21 + a33 * b31 + a34 * m41;
        float n32 = a31 * b12 + a32 * b22 + a33 * b32 + a34 * m42;
        float n33 = a31 * b13 + a32 * b23 + a33 * b33 + a34 * m43;
        float n34 = a31 * b14 + a32 * b24 + a33 * b34 + a34 * m44;
        return new CFrame(n14, n24, n34, n11, n12, n13, n21, n22, n23, n31, n32, n33);
    }

    public override string ToString()
    {
        return string.Join(", ", GetComponents());
    }

    private static Vector3 VectorAxisAngle(Vector3 vec, Vector3 axis, float theta)
    {
        Vector3 unit = vec.Unit;
        float cosAng = (float)Math.Cos(theta);
        float sinAng = (float)Math.Sin(theta);
        return axis * cosAng + axis._dot(unit) * unit * (1 - cosAng) + unit.Cross(axis) * sinAng;
    }

    public CFrame Inverse()
    {
        float[] ac = GetComponents();
        float a14 = ac[0], a24 = ac[1], a34 = ac[2],
              a11 = ac[3], a12 = ac[4], a13 = ac[5],
              a21 = ac[6], a22 = ac[7], a23 = ac[8],
              a31 = ac[9], a32 = ac[10], a33 = ac[11];
        float det = (a11 * a22 * a33 * m44 + a11 * a23 * a34 * m42 + a11 * a24 * a32 * m43
                    + a12 * a21 * a34 * m43 + a12 * a23 * a31 * m44 + a12 * a24 * a33 * m41
                    + a13 * a21 * a32 * m44 + a13 * a22 * a34 * m41 + a13 * a24 * a31 * m42
                    + a14 * a21 * a33 * m42 + a14 * a22 * a31 * m43 + a14 * a23 * a32 * m41
                    - a11 * a22 * a34 * m43 - a11 * a23 * a32 * m44 - a11 * a24 * a33 * m42
                    - a12 * a21 * a33 * m44 - a12 * a23 * a34 * m41 - a12 * a24 * a31 * m43
                    - a13 * a21 * a34 * m42 - a13 * a22 * a31 * m44 - a13 * a24 * a32 * m41
                    - a14 * a21 * a32 * m43 - a14 * a22 * a33 * m41 - a14 * a23 * a31 * m42);
        if (det == 0)
            return this;
        float b11 = (a22 * a33 * m44 + a23 * a34 * m42 + a24 * a32 * m43 - a22 * a34 * m43 - a23 * a32 * m44 - a24 * a33 * m42) / det;
        float b12 = (a12 * a34 * m43 + a13 * a32 * m44 + a14 * a33 * m42 - a12 * a33 * m44 - a13 * a34 * m42 - a14 * a32 * m43) / det;
        float b13 = (a12 * a23 * m44 + a13 * a24 * m42 + a14 * a22 * m43 - a12 * a24 * m43 - a13 * a22 * m44 - a14 * a23 * m42) / det;
        float b14 = (a12 * a24 * a33 + a13 * a22 * a34 + a14 * a23 * a32 - a12 * a23 * a34 - a13 * a24 * a32 - a14 * a22 * a33) / det;
        float b21 = (a21 * a34 * m43 + a23 * a31 * m44 + a24 * a33 * m41 - a21 * a33 * m44 - a23 * a34 * m41 - a24 * a31 * m43) / det;
        float b22 = (a11 * a33 * m44 + a13 * a34 * m41 + a14 * a31 * m43 - a11 * a34 * m43 - a13 * a31 * m44 - a14 * a33 * m41) / det;
        float b23 = (a11 * a24 * m43 + a13 * a21 * m44 + a14 * a23 * m41 - a11 * a23 * m44 - a13 * a24 * m41 - a14 * a21 * m43) / det;
        float b24 = (a11 * a23 * a34 + a13 * a24 * a31 + a14 * a21 * a33 - a11 * a24 * a33 - a13 * a21 * a34 - a14 * a23 * a31) / det;
        float b31 = (a21 * a32 * m44 + a22 * a34 * m41 + a24 * a31 * m42 - a21 * a34 * m42 - a22 * a31 * m44 - a24 * a32 * m41) / det;
        float b32 = (a11 * a34 * m42 + a12 * a31 * m44 + a14 * a32 * m41 - a11 * a32 * m44 - a12 * a34 * m41 - a14 * a31 * m42) / det;
        float b33 = (a11 * a22 * m44 + a12 * a24 * m41 + a14 * a21 * m42 - a11 * a24 * m42 - a12 * a21 * m44 - a14 * a22 * m41) / det;
        float b34 = (a11 * a24 * a32 + a12 * a21 * a34 + a14 * a22 * a31 - a11 * a22 * a34 - a12 * a24 * a31 - a14 * a21 * a32) / det;
        return new CFrame(b14, b24, b34, b11, b12, b13, b21, b22, b23, b31, b32, b33);
    }

    public static CFrame FromAxisAngle(Vector3 axis, float theta)
    {
        Vector3 r = VectorAxisAngle(axis, Vector3.xAxis, theta),
                u = VectorAxisAngle(axis, Vector3.yAxis, theta),
                b = VectorAxisAngle(axis, Vector3.zAxis, theta);
        return new CFrame(0, 0, 0, r.X, u.X, b.X, r.Y, u.Y, b.Y, r.Z, u.Z, b.Z);
    }

    public static CFrame FromEulerAnglesXYZ(float x, float y, float z)
    {
        CFrame cfx = FromAxisAngle(Vector3.xAxis, x),
               cfy = FromAxisAngle(Vector3.yAxis, y),
               cfz = FromAxisAngle(Vector3.zAxis, z);
        return cfx * cfy * cfz;
    }

    public static CFrame FromEulerAnglesXYZ(params float[] angles)
    {
        float x = angles[0],
              y = angles[1],
              z = angles[2];
        return FromEulerAnglesXYZ(x, y, z);
    }

    public static CFrame _angles(float x, float y, float z) => FromEulerAnglesXYZ(x, y, z);
    public static CFrame _angles(params float[] angles) => FromEulerAnglesXYZ(angles);

    public CFrame _lerp(CFrame other, float t)
    {
        if (t == 0f)
            return this;
        else if (t == 1f)
            return other;
        var q1 = new Quaternion(this);
        var q2 = new Quaternion(other);
        CFrame rot = q1.Slerp(q2, t).ToCFrame();
        Vector3 pos = Position.Lerp(other.Position, t);
        return new CFrame(pos) * rot;
    }

    public static IEnumerator Lerp(CallData dat)
    {
        object[] inp = Luau.getAllArgs(ref dat);
        CFrame cf1 = (CFrame)inp[0];
        CFrame cf2 = (CFrame)inp[1];
        double lerp = (double)inp[2];
        if (lerp == 0d)
            Luau.returnToProto(ref dat, new object[1] { cf1 });
        else if (lerp == 1d)
            Luau.returnToProto(ref dat, new object[1] { cf2 });
        var q1 = new Quaternion(cf1);
        var q2 = new Quaternion(cf2);
        CFrame rot = q1.Slerp(q2, (float)lerp).ToCFrame();
        Vector3 pos = cf1.Position.Lerp(cf2.Position, (float)lerp);
        Luau.returnToProto(ref dat, new object[1] { new CFrame(pos) * rot });
        yield break;
    }

    public CFrame ToWorldSpace(CFrame cf2)
    {
        return this * cf2;
    }

    public CFrame ToObjectSpace(CFrame other)
    {
        return Inverse() * other;
    }

    public Vector3 PointToWorldSpace(Vector3 v)
    {
        return this * v;
    }

    public Vector3 PointToObjectSpace(Vector3 v)
    {
        return Inverse() * v;
    }

    public Vector3 VectorToWorldSpace(Vector3 v)
    {
        return (this - Position) * v;
    }

    public Vector3 VectorToObjectSpace(Vector3 v)
    {
        return (this - Position).Inverse() * v;
    }

    public float[] GetComponents()
    {
        return new float[]
        {
                m14, m24, m34,
                m11, m12, m13,
                m21, m22, m23,
                m31, m32, m33
        };
    }

    public EulerAngles ToEulerAngles() => new EulerAngles
    {
        Yaw = (float)Math.Asin(m13),
        Pitch = (float)Math.Atan2(-m23, m33),
        Roll = (float)Math.Atan2(-m12, m11),
    };

    [Obsolete]
    public float[] ToEulerAnglesXYZ()
    {
        var result = ToEulerAngles();

        return new float[]
        {
                result.Pitch,
                result.Yaw,
                result.Roll
        };
    }

    public bool IsAxisAligned()
    {
        var tests = new float[3]
        {
                XVector._dot(Vector3.xAxis),
                YVector._dot(Vector3.yAxis),
                ZVector._dot(Vector3.zAxis)
        };

        foreach (var test in tests)
        {
            float dot = Math.Abs(test);

            if (dot.FuzzyEquals(1))
                continue;

            if (dot.FuzzyEquals(0))
                continue;

            return false;
        }

        return true;
    }

    private static bool IsLegalOrientId(int orientId)
    {
        int xOrientId = (orientId / 6) % 3;
        int yOrientId = orientId % 3;

        return (xOrientId != yOrientId);
    }

    public static IEnumerator Angles(CallData dat)
    {
        dynamic[] inp = Luau.getAllArgs(ref dat);
        float x = 0f; float y = 0f; float z = 0f;
        switch (inp.Length)
        {
            case 1:
                x = Convert.ToSingle(inp[0]);
                break;
            case 2:
                x = Convert.ToSingle(inp[0]); y = Convert.ToSingle(inp[1]);
                break;
            case 3:
                x = Convert.ToSingle(inp[0]); y = Convert.ToSingle(inp[1]); z = Convert.ToSingle(inp[2]);
                break;
        }
        CFrame t = FromEulerAnglesXYZ(x, y, z);
        Luau.returnToProto(ref dat, new object[1] { t });
        yield break;
    }

    public static IEnumerator @new(CallData dat)
    {
        dynamic[] inp = Luau.getAllArgs(ref dat);
        switch (inp.Length)
        {
            case 0:
                Luau.returnToProto(ref dat, new object[1] { new CFrame() });
                yield break;
            case 1:
                Luau.returnToProto(ref dat, new object[1] { new CFrame(inp[0]) });
                yield break;
            case 2:
                Luau.returnToProto(ref dat, new object[1] { new CFrame(inp[0], inp[1]) });
                yield break;
            case 3:
                Luau.returnToProto(ref dat, new object[1] { new CFrame((float)inp[0], (float)inp[1], (float)inp[2]) });
                yield break;
            case 7:
                Luau.returnToProto(ref dat, new object[1] { new CFrame((float)inp[0], (float)inp[1], (float)inp[2], (float)inp[3], (float)inp[4], (float)inp[5], (float)inp[6]) });
                yield break;
            case 11:
                Luau.returnToProto(ref dat, new object[1] { new CFrame((float)inp[0], (float)inp[1], (float)inp[2], (float)inp[3], (float)inp[4], (float)inp[5], (float)inp[6], (float)inp[7], (float)inp[8], (float)inp[9], (float)inp[10], (float)inp[11]) });
                yield break;
            default:
                Logging.Error($"No constructor found for CFrame with argument count {inp.Length}", "Luauni:CFrame");
                dat.initiator.globalErrored = true;
                yield break;
        }
    }
}

public class Quaternion
{
    public readonly float X, Y, Z, W;
    public override string ToString() => $"{X}, {Y}, {Z}, {W}";

    public static implicit operator UnityEngine.Quaternion(Quaternion obj)
    {
        return new UnityEngine.Quaternion(obj.X, obj.Y, obj.Z, obj.W);
    }

    public Quaternion(UnityEngine.Quaternion quat)
    {
        X = quat.x;
        Y = quat.y;
        Z = quat.z;
        W = quat.w;
    }

    public float Magnitude
    {
        get
        {
            float squared = Dot(this);
            double magnitude = Math.Sqrt(squared);
            return (float)magnitude;
        }
    }

    public Quaternion(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Quaternion(Vector3 qv, float qw)
    {
        X = qv.X;
        Y = qv.Y;
        Z = qv.Z;
        W = qw;
    }

    public Quaternion(CFrame cf)
    {
        float[] ac = cf.GetComponents();
        float m11 = ac[3], m12 = ac[4], m13 = ac[5],
              m21 = ac[6], m22 = ac[7], m23 = ac[8],
              m31 = ac[9], m32 = ac[10], m33 = ac[11];
        float trace = m11 + m22 + m33;
        if (trace > 0)
        {
            float s = (float)Math.Sqrt(1 + trace);
            float r = 0.5f / s;
            W = s * 0.5f;
            X = (m32 - m23) * r;
            Y = (m13 - m31) * r;
            Z = (m21 - m12) * r;
        }
        else
        {
            float big = Math.Max(Math.Max(m11, m22), m33);
            if (big == m11)
            {
                float s = (float)Math.Sqrt(1 + m11 - m22 - m33);
                float r = 0.5f / s;
                W = (m32 - m23) * r;
                X = 0.5f * s;
                Y = (m21 + m12) * r;
                Z = (m13 + m31) * r;
            }
            else if (big == m22)
            {
                float s = (float)Math.Sqrt(1 - m11 + m22 - m33);
                float r = 0.5f / s;
                W = (m13 - m31) * r;
                X = (m21 + m12) * r;
                Y = 0.5f * s;
                Z = (m32 + m23) * r;
            }
            else if (big == m33)
            {
                float s = (float)Math.Sqrt(1 - m11 - m22 + m33);
                float r = 0.5f / s;
                W = (m21 - m12) * r;
                X = (m13 + m31) * r;
                Y = (m32 + m23) * r;
                Z = 0.5f * s;
            }
        }
    }

    public float Dot(Quaternion other)
    {
        return (X * other.X) + (Y * other.Y) + (Z * other.Z) + (W * other.W);
    }

    public Quaternion Lerp(Quaternion other, float alpha)
    {
        Quaternion result = this * (1.0f - alpha) + other * alpha;
        return result / result.Magnitude;
    }

    public Quaternion Slerp(Quaternion other, float alpha)
    {
        float cosAng = Dot(other);
        if (cosAng < 0)
        {
            other = -other;
            cosAng = -cosAng;
        }
        double ang = Math.Acos(cosAng);
        if (ang >= 0.05f)
        {
            float scale0 = (float)Math.Sin((1.0f - alpha) * ang);
            float scale1 = (float)Math.Sin(alpha * ang);
            float denom = (float)Math.Sin(ang);

            return ((this * scale0) + (other * scale1)) / denom;
        }
        else
        {
            return Lerp(other, alpha);
        }
    }

    public CFrame ToCFrame()
    {
        float xc = X * 2f,
              yc = Y * 2f,
              zc = Z * 2f;
        float xx = X * xc,
              xy = X * yc,
              xz = X * zc;
        float wx = W * xc,
              wy = W * yc,
              wz = W * zc;
        float yy = Y * yc,
              yz = Y * zc,
              zz = Z * zc;
        return new CFrame
        (
            0, 0, 0,
            1f - (yy + zz),
            xy - wz,
            xz + wy,
            xy + wz,
            1f - (xx + zz),
            yz - wx,
            xz - wy,
            yz + wx,
            1f - (xx + yy)
        );
    }

    public static Quaternion operator +(Quaternion a, Quaternion b)
    {
        return new Quaternion(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    }

    public static Quaternion operator -(Quaternion a, Quaternion b)
    {
        return new Quaternion(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
    }

    public static Quaternion operator *(Quaternion a, float f)
    {
        return new Quaternion(a.X * f, a.Y * f, a.Z * f, a.W * f);
    }

    public static Quaternion operator /(Quaternion a, float f)
    {
        return new Quaternion(a.X / f, a.Y / f, a.Z / f, a.W / f);
    }

    public static Quaternion operator -(Quaternion a)
    {
        return new Quaternion(-a.X, -a.Y, -a.Z, -a.W);
    }

    public static Quaternion operator *(Quaternion a, Quaternion b)
    {
        Vector3 v1 = new Vector3(a.X, a.Y, a.Z),
                v2 = new Vector3(b.X, b.Y, b.Z);
        float s1 = a.W,
              s2 = b.W;
        return new Quaternion(s1 * v2 + s2 * v1 + v1.Cross(v2), s1 * s2 - v1._dot(v2));
    }

    public EulerAngles ToEulerAngles()
    {
        var angles = new EulerAngles();
        double sinr_cosp = 2 * (W * X + Y * Z);
        double cosr_cosp = 1 - 2 * (X * X + Y * Y);
        angles.Roll = (float)Math.Atan2(sinr_cosp, cosr_cosp);
        double sinp = 2 * (W * Y - Z * X);
        angles.Pitch = (float)Math.Asin(sinp);
        double siny_cosp = 2 * (W * Z + X * Y);
        double cosy_cosp = 1 - 2 * (Y * Y + Z * Z);
        angles.Yaw = (float)Math.Atan2(siny_cosp, cosy_cosp);
        return angles;
    }

    public override int GetHashCode()
    {
        int hash = X.GetHashCode()
                 ^ Y.GetHashCode()
                 ^ Z.GetHashCode()
                 ^ W.GetHashCode();
        return hash;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Quaternion other))
            return false;
        if (!X.Equals(other.X))
            return false;
        if (!Y.Equals(other.Y))
            return false;
        if (!Z.Equals(other.Z))
            return false;
        if (!W.Equals(other.W))
            return false;
        return true;
    }
}

internal static class Formatting
{
    public static bool FuzzyEquals(this float a, float b, float epsilon = 10e-5f)
    {
        return Math.Abs(a - b) < epsilon;
    }
}

public struct EulerAngles
{
    public float Yaw;
    public float Pitch;
    public float Roll;
}