using System.Collections.Generic;
using System.Linq;

public class ByteReader
{
    public ByteReader(byte[] data)
    {
        source = data.Cast<byte>().GetEnumerator();
    }

    private IEnumerator<byte> source;

    public void Skip(int count)
    {
        for (int i = 0; i < count; i++)
            source.MoveNext();
    }

    public byte ReadByte()
    {
        if (source.MoveNext())
            return source.Current;
        Logging.Warn("Something tried to read beyond the end of the enumerator.", "ByteReader");
        return 0;
    }

    public ushort ReadUInt16(Endian e = Endian.Little)
    {
        switch (e)
        {
            case Endian.Little:
                return (ushort)(ReadByte() + (ReadByte() * 256));
            case Endian.Big:
                return (ushort)((ReadByte() * 256) + ReadByte());
        }
        Logging.Warn($"ReadUInt16 received an invalid endian ID: {(int)e}", "ByteReader");
        return 0;
    }

    public uint ReadUInt32(Endian e = Endian.Little)
    {
        switch (e)
        {
            case Endian.Little:
                return (uint)(ReadByte() + (ReadByte() * 256) + (ReadByte() * 65536) + (ReadByte() * 16777216));
            case Endian.Big:
                return (uint)((ReadByte() * 16777216) + (ReadByte() * 65536) + (ReadByte() * 256) + ReadByte());
        }
        Logging.Warn($"ReadUInt32 received an invalid endian ID: {(uint)e}", "ByteReader");
        return 0;
    }

    public byte[] ReadRange(int count)
    {
        byte[] buf = new byte[count];
        for (int i = 0; i < count; i++)
            buf[i] = ReadByte();
        return buf;
    }

    public string ReadRangeStr(int count)
    {
        char[] buf = new char[count];
        for (int i = 0; i < count; i++)
            buf[i] = (char)ReadByte();
        return new string(buf);
    }

    public int ReadVariableLen()
    {
        int result = 0;
        int shift = 0;
        while (true)
        {
            byte b = ReadByte();
            result |= (b & 127) << shift;
            shift += 7;
            if ((b & 128) == 0)
            {
                break;
            }
        }
        return result;
    }
}

public enum Endian
{
    Little = 0,
    Big = 1
}