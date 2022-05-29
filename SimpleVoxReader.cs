using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class SimpleVoxReader
{
    public struct ChunkHeader
    {
        public string chunkId;
        public int contentBytes;
        public int childBytes;

        public override string ToString()
        {
            return $"Id:{chunkId}\nContent Len:{contentBytes}\nChild Len(Bytes):{childBytes}";
        }
    }
    public struct Voxel
    {
        public byte x, y, z, i;
        public override string ToString()
        {
            return $"({x},{y},{z},{i})";
        }
    }
    public struct RGBA : IEquatable<RGBA>
    {
        public static readonly RGBA DEFAULT = new RGBA(75, 75, 75, 255);
        public byte r, g, b, a;

        public RGBA(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public bool Equals(RGBA other)
        {
            return r == other.r && g == other.g && b == other.b && a == other.a;
        }

        public override string ToString()
        {
            return $"({r},{g},{b},{a})";
        }
    }
    public struct Size
    {
        public int x, y, z;
        public override string ToString()
        {
            return $"({x},{y},{z})";
        }
    }
    public Stream BaseStream { get => reader.BaseStream; }
    private BinaryReader reader;

    public SimpleVoxReader(Stream stream)
    {
        reader = new BinaryReader(stream);
        var voxMagic = Encoding.ASCII.GetString(reader.ReadBytes(4));
        var version = reader.ReadInt32();
    }

    public ChunkHeader ReadHeader()
    {
        ChunkHeader header = new ChunkHeader();
        header.chunkId = Encoding.ASCII.GetString(reader.ReadBytes(4));
        header.contentBytes = reader.ReadInt32();
        header.childBytes = reader.ReadInt32();
        return header;
    }

    public void SkipChunk(ChunkHeader header)
    {
        reader.BaseStream.Seek(header.childBytes + header.contentBytes, SeekOrigin.Current);
    }

    public int ReadInt() => reader.ReadInt32();
    public Voxel ReadVoxel()
    {
        Voxel voxel = new Voxel();
        voxel.x = reader.ReadByte();
        voxel.y = reader.ReadByte();
        voxel.z = reader.ReadByte();
        voxel.i = reader.ReadByte();
        return voxel;
    }

    public IEnumerable<Voxel> ReadVoxels()
    {
        int voxelCount = reader.ReadInt32();
        for (int i = 0; i < voxelCount; i++)
        {
            yield return ReadVoxel();
        }
    }

    public RGBA ReadColor()
    {
        RGBA color = new RGBA();
        color.r = reader.ReadByte();
        color.g = reader.ReadByte();
        color.b = reader.ReadByte();
        color.a = reader.ReadByte();
        return color;
    }

    public IEnumerable<RGBA> ReadPalette()
    {
        for (int i = 0; i < 256; i++)
        {
            RGBA color = ReadColor();
            if (color.Equals(RGBA.DEFAULT)) break;
            yield return color;
        }
    }

    public bool IsEnd()
    {
        return reader.BaseStream.Position == reader.BaseStream.Length;
    }

    public Size ReadSize()
    {
        Size size = new Size();
        size.x = reader.ReadInt32();
        size.y = reader.ReadInt32();
        size.z = reader.ReadInt32();
        return size;
    }
}