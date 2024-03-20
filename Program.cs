using System;

class Program
{
    static void Main(string[] args)
    {        
        Console.Clear();
        Logging.ShowDebug = true;
        Luauni script = new Luauni("C:\\Users\\emil3\\Desktop\\luau\\out.bin");
        script.Step();
    }
}