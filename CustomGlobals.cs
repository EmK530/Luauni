public static class CG
{
    public static void print(params object[] args)
    {
        string output = "";
        bool first = true;
        foreach (var a in args)
        {
            if (!first)
            {
                output += " ";
            }
            output += a.ToString();
            first = false;
        }
        Console.WriteLine("\x1b[7;30;47m[Bytecode] " + output + "\x1b[0m ");
    }
    public static class math
    {
        public static object[] abs(params object[] inp)
        {
            return new object[1] { Math.Abs(safeNum(inp[0])) };
        }
        public static object[] acos(params object[] inp)
        {
            return new object[1] { Math.Acos(safeNum(inp[0])) };
        }
        public static object[] asin(params object[] inp)
        {
            return new object[1] { Math.Asin(safeNum(inp[0])) };
        }
        public static object[] atan(params object[] inp)
        {
            return new object[1] { Math.Atan(safeNum(inp[0])) };
        }
        public static object[] atan2(params object[] inp)
        {
            return new object[1] { Math.Atan2(safeNum(inp[0]), safeNum(inp[1])) };
        }
        public static object[] ceil(params object[] inp)
        {
            return new object[1] { Math.Ceiling(safeNum(inp[0])) };
        }
        public static object[] clamp(params object[] inp)
        {
            return new object[1] { Math.Clamp(safeNum(inp[0]), safeNum(inp[1]), safeNum(inp[2])) };
        }
        public static object[] cos(params object[] inp)
        {
            return new object[1] { Math.Cos(safeNum(inp[0])) };
        }
        public static object[] deg(params object[] inp)
        {
            return new object[1] { (180d/Math.PI)*safeNum(inp[0]) };
        }
        public static object[] exp(params object[] inp)
        {
            return new object[1] { Math.Pow(Math.E,safeNum(inp[0])) };
        }
        public static object[] floor(params object[] inp)
        {
            return new object[1] { Math.Floor(safeNum(inp[0])) };
        }
        public static object[] fmod(params object[] inp)
        {
            Luauni.error("math.fmod is not implemented! Returning 0.");
            return new object[1] { 0d };
        }
        public static object[] frexp(params object[] inp)
        {
            Luauni.error("math.frexp is not implemented! Returning 0.");
            return new object[1] { 0d };
        }
        public static object[] ldexp(params object[] inp)
        {
            return new object[1] { (double)inp[0] * Math.Pow(2, Convert.ToInt64(inp[1])) };
        }
        public static object[] log(params object[] inp)
        {
            return new object[1] { Math.Log(safeNum(inp[0])) };
        }
        public static object[] log10(params object[] inp)
        {
            return new object[1] { Math.Log10(safeNum(inp[0])) };
        }
        public static object[] max(params object[] inp)
        {
            return new object[1] { Math.Max(safeNum(inp[0]), safeNum(inp[1])) };
        }
        public static object[] min(params object[] inp)
        {
            return new object[1] { Math.Min(safeNum(inp[0]), safeNum(inp[1])) };
        }
        public static object[] modf(params object[] inp)
        {
            Luauni.error("math.modf is not implemented! Returning 0.");
            return new object[2] { 0d, 0d };
        }
        public static object[] noise(params object[] inp)
        {
            Luauni.error("math.noise is not implemented! Returning 0.");
            return new object[1] { 0d };
        }
        public static object[] pow(params object[] inp)
        {
            return new object[1] { Math.Pow(safeNum(inp[0]), safeNum(inp[1])) };
        }
        public static object[] rad(params object[] inp)
        {
            return new object[1] { Math.Pow(safeNum(inp[0]), safeNum(inp[1])) };
        }
        public static object[] random(params object[] inp)
        {
            switch (inp.Length)
            {
                case 0:
                    {
                        return new object[1] { Math2.ldexp(Luau.pcg32_random() | ((ulong)Luau.pcg32_random() << 32), -64) };
                    }
                case 1:
                    {
                        return new object[1] { (int)(1 + ((Convert.ToUInt64(inp[0]) * Luau.pcg32_random()) >> 32)) };
                    }
                case 2:
                    {
                        int l = Convert.ToInt32((double)inp[0]);
                        int u = Convert.ToInt32((double)inp[1]);
                        return new object[1] { l + (int)((((uint)u - (uint)l + 1UL) * Luau.pcg32_random()) >> 32) };
                    }
                default:
                    Luauni.error("Unsupported number of math.random arguments! Returning 0.");
                    return new object[1] { 0d };
            }
        }
        public static object[] randomseed(params object[] inp)
        {
            Luau.pcg32_seed(Convert.ToUInt64((double)inp[0]));
            return new object[0];
        }
        public static object[] round(params object[] inp)
        {
            return new object[1] { Math.Round(safeNum(inp[0])) };
        }
        public static object[] sign(params object[] inp)
        {
            return new object[1] { Math.Sign(safeNum(inp[0])) };
        }
        public static object[] sin(params object[] inp)
        {
            return new object[1] { Math.Sin(safeNum(inp[0])) };
        }
        public static object[] sinh(params object[] inp)
        {
            return new object[1] { Math.Sinh(safeNum(inp[0])) };
        }
        public static object[] sqrt(params object[] inp)
        {
            return new object[1] { Math.Sqrt(safeNum(inp[0])) };
        }
        public static object[] tan(params object[] inp)
        {
            return new object[1] { Math.Tan(safeNum(inp[0])) };
        }
        public static object[] tanh(params object[] inp)
        {
            return new object[1] { Math.Tanh(safeNum(inp[0])) };
        }
    }
    private static double safeNum(object inp)
    {
        if (inp == null)
        {
            return 0d;
        } else
        {
            return (double)inp;
        }
    }
}
public static class Math2
{
    public static double ldexp(double x, int i)
    {
        return x * Math.Pow(2, i);
    }
}