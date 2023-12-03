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
}