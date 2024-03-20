#pragma warning disable CS8981

using System;

public static class NMath // native math
{
    public static double ldexp(double x, int i)
    {
        return x * Math.Pow(2, i);
    }
}