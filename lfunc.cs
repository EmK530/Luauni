#pragma warning disable CS8602

public static class lfunc {
    public static Proto luaF_newproto()
    {
        Proto f = new Proto();
        f.maxstacksize = 0;
        f.numparams = 0;
        f.nups = 0;
        f.is_vararg = 0;
        f.k = null;
        f.sizek = 0;
        f.p = null;
        f.sizep = 0;
        f.code = null;
        f.sizecode = 0;
        f.flags = 0;
        f.codeentry = null;
        f.registers = new object[255];
        f.instpos = -1;
        f.lastReturn = null;
        f.lastReturnCount = 0;
        return f;
    }
}