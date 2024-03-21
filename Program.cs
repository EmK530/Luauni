using System;

class Program
{
    static void Main(string[] args)
    {        
        Console.Clear();
        Logging.ShowDebug = false;
        Logging.Print("Creating Luauni instance...");
        Luauni script = new Luauni("C:\\Users\\emil3\\Desktop\\luau\\out.bin");
        if (script.IsReady())
        {
            script.Step();
        } else
        {
            Logging.Error("Script did not load.");
        }
    }
}