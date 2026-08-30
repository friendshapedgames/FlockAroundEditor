using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Godot;

namespace FlockAroundLevelEditor.Scripts.Editor;

public class PckReader : IDisposable
{
    private readonly Dictionary<string, PckFile> _files = new();
    private readonly long _filesBase;
    private readonly Version _godotVersion;
    private readonly int _packFlags;
    private readonly BinaryReader _reader;
    private readonly FileStream _stream;
    private readonly bool _isVerbose;

    /// <summary>
    ///     If only the Godot maintainers would get their heads out of their asses and implement a PckReader instead of
    ///     arguing about it for SIX YEARS!
    /// </summary>
    public PckReader(string pckPath, bool verbose)
    {
        _isVerbose = verbose;
        _stream = File.OpenRead(pckPath);
        _reader = new BinaryReader(_stream);

        var magicNumber = _reader.ReadUInt32();
        if (magicNumber != 0x43504447)
        {
            GD.PrintErr("Could not find magic number in PCK");
        }

        var formatVersion = _reader.ReadInt32();

        if (formatVersion != 3)
        {
            throw new Exception(
                $"This PCK is version {formatVersion}; This reader only handles version {formatVersion}");
        }

        var major = _reader.ReadInt32();
        var minor = _reader.ReadInt32();
        var revision = _reader.ReadInt32();
        _godotVersion = new Version(major, minor, revision);

        _packFlags = _reader.ReadInt32();
        _filesBase = _reader.ReadInt64();
        var directoryBase = _reader.ReadInt64();

        _stream.Seek(directoryBase, SeekOrigin.Begin);
        var fileCount = _reader.ReadInt32();

        for (var i = 0; i < fileCount; i++)
        {
            var file = new PckFile();

            var nameLengthPlusPadding = _reader.ReadInt32();
            var fileName = Encoding.UTF8.GetString(_reader.ReadBytes(nameLengthPlusPadding)).TrimEnd('\0');
            file.Offset = _reader.ReadInt64();
            file.Size = _reader.ReadInt64();
            file.Hash = PckFileHash.FromReader(_reader);
            file.Flags = _reader.ReadInt32();

            if (verbose)
            {
                GD.Print($"Loaded: {fileName}");
            }

            _files.Add(fileName, file);
        }
    }

    public void Dispose()
    {
        _reader.Dispose();
        _stream.Dispose();
    }

    public IEnumerable<string> ListFiles()
    {
        foreach (var fileName in _files.Keys)
        {
            yield return fileName;
        }
    }

    public byte[] ReadFileBytes(string fileName)
    {
        if (!FileExistsExactly(fileName))
        {
            GD.Print(
                $"WARNING: Attempted to read PCK file {fileName}, no such file exists (returning empty byte array)");
            return [];
        }

        var file = GetFileConfident(fileName);

        _stream.Seek(_filesBase, SeekOrigin.Begin);
        _stream.Seek(file.Offset, SeekOrigin.Current);

        if (file.Size > int.MaxValue)
        {
            // kind of a silly limitation, could workaround this by reading it in chunks 
            GD.Print(
                $"WARNING: PCK file {fileName} is more than {int.MaxValue} (max 32 bit int) bytes long, result will be truncated!");
        }

        return _reader.ReadBytes((int)file.Size);
    }

    public string ReadFileString(string fileName)
    {
        return Encoding.UTF8.GetString(ReadFileBytes(fileName)).TrimEnd('\0');
    }

    /// <summary>
    ///     Returns true if the file exists in the PCK, most PCK files are remaps so if you're looking for scenes or resources
    ///     this is not what you want.
    /// </summary>
    public bool FileExistsExactly(string fileName)
    {
        return _files.ContainsKey(NormalizedFileName(fileName));
    }

    /// <summary>
    ///     Returns true if the file exists or if it has a .remap or .import implying that it WILL exist when the PCK is loaded.
    /// </summary>
    public bool FileExistsOrHasRemap(string fileName)
    {
        var normalizedFileName = NormalizedFileName(fileName);
        return _files.ContainsKey(normalizedFileName) || _files.ContainsKey($"{normalizedFileName}.remap") || _files.ContainsKey($"{normalizedFileName}.import");
    }


    private PckFile GetFileConfident(string fileName)
    {
        var normalizedFileName = NormalizedFileName(fileName);
        if (_files.TryGetValue(normalizedFileName, out var file))
        {
            return file;
        }

        throw new Exception($"Pck does not have file matching {fileName}");
    }

    private string NormalizedFileName(string fileName)
    {
        var prefix = "res://";
        if (fileName.StartsWith(prefix))
        {
            return fileName.Substring(prefix.Length, fileName.Length - prefix.Length);
        }

        return fileName;
    }


    private struct PckFile
    {
        public long Offset;
        public long Size;
        public PckFileHash Hash;
        public int Flags;
    }

    private struct PckFileHash
    {
        public byte Byte0;
        public byte Byte1;
        public byte Byte2;
        public byte Byte3;
        public byte Byte4;
        public byte Byte5;
        public byte Byte6;
        public byte Byte7;
        public byte Byte8;
        public byte Byte9;
        public byte Byte10;
        public byte Byte11;
        public byte Byte12;
        public byte Byte13;
        public byte Byte14;
        public byte Byte15;

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            var format = "X";
            stringBuilder.Append(Byte0.ToString(format));
            stringBuilder.Append(Byte1.ToString(format));
            stringBuilder.Append(Byte2.ToString(format));
            stringBuilder.Append(Byte3.ToString(format));
            stringBuilder.Append(Byte4.ToString(format));
            stringBuilder.Append(Byte5.ToString(format));
            stringBuilder.Append(Byte6.ToString(format));
            stringBuilder.Append(Byte7.ToString(format));
            stringBuilder.Append(Byte8.ToString(format));
            stringBuilder.Append(Byte9.ToString(format));
            stringBuilder.Append(Byte10.ToString(format));
            stringBuilder.Append(Byte11.ToString(format));
            stringBuilder.Append(Byte12.ToString(format));
            stringBuilder.Append(Byte13.ToString(format));
            stringBuilder.Append(Byte14.ToString(format));
            stringBuilder.Append(Byte15.ToString(format));

            return stringBuilder.ToString();
        }

        public static PckFileHash FromArray(byte[] array)
        {
            if (array.Length != 16)
            {
                throw new Exception("Expected array of length 16, no more no less");
            }

            return new PckFileHash
            {
                Byte0 = array[0],
                Byte1 = array[1],
                Byte2 = array[2],
                Byte3 = array[3],
                Byte4 = array[4],
                Byte5 = array[5],
                Byte6 = array[6],
                Byte7 = array[7],
                Byte8 = array[8],
                Byte9 = array[9],
                Byte10 = array[10],
                Byte11 = array[11],
                Byte12 = array[12],
                Byte13 = array[13],
                Byte14 = array[14],
                Byte15 = array[15]
            };
        }

        public static PckFileHash FromReader(BinaryReader reader)
        {
            // Not using initializer because we're very sensitive to order here
            // ReSharper disable once UseObjectOrCollectionInitializer
            var hash = new PckFileHash();

            hash.Byte0 = reader.ReadByte();
            hash.Byte1 = reader.ReadByte();
            hash.Byte2 = reader.ReadByte();
            hash.Byte3 = reader.ReadByte();
            hash.Byte4 = reader.ReadByte();
            hash.Byte5 = reader.ReadByte();
            hash.Byte6 = reader.ReadByte();
            hash.Byte7 = reader.ReadByte();
            hash.Byte8 = reader.ReadByte();
            hash.Byte9 = reader.ReadByte();
            hash.Byte10 = reader.ReadByte();
            hash.Byte11 = reader.ReadByte();
            hash.Byte12 = reader.ReadByte();
            hash.Byte13 = reader.ReadByte();
            hash.Byte14 = reader.ReadByte();
            hash.Byte15 = reader.ReadByte();

            return hash;
        }
    }
}