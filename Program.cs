public static class Program
{
    public static Luauni? luauni;
    public static void Main()
    {
        Console.Clear();
        Essentials.system("cls");
        Luauni.db = false;
        luauni = new Luauni("C:\\Users\\emil3\\Desktop\\luau\\errorlog.bin");
        luauni.Parse();
        luauni.Execute();
        Console.WriteLine();
        Luauni.print("Luauni finished executing the bytecode.");
        Console.ReadLine();
    }
}