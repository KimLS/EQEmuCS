using EQEmu.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EQEmu.Core.FileSystem
{
    public class PackedFileSystem : IFileSystem
    {
        private PackedFileSystemEntry FileNames { get; set; }

        private List<PackedFileSystemEntry> _Entries;
        public IEnumerable<IFileSystemEntry> Entries {
            get => _Entries;
            set => throw new NotImplementedException();
        }

        public bool FooterEnabled { get; set; }
        public DateTime FooterTimestamp { get; set; }

        public static IFileSystem Create()
        {
            return new PackedFileSystem();
        }

        private PackedFileSystem()
        {
            Clear();
        }

        public void Clear()
        {
            FileNames = new PackedFileSystemEntry();
            _Entries = new List<PackedFileSystemEntry>();
            FooterEnabled = false;
            FooterTimestamp = DateTime.UtcNow;
        }

        public void Insert(string filename, string path)
        {
            Insert(filename, File.ReadAllBytes(path));
        }

        public void Insert(string filename, byte[] data)
        {
            if (_Entries.Exists(t => t.FileName == filename.ToLower()))
            {
                var replaceEntry = _Entries.First(t => t.FileName == filename.ToLower());
                var replaceBlock = DeflateBlock(data);
                replaceEntry.Block = replaceBlock.Item1;
                replaceEntry.Length = replaceBlock.Item2;
                return;
            }

            var entry = new PackedFileSystemEntry();
            entry.CRC = new CRC().Get(filename.ToLower());
            entry.Archive = this;
            entry.FileName = filename.ToLower();

            var block = DeflateBlock(data);
            entry.Block = block.Item1;
            entry.Length = block.Item2;

            _Entries.Add(entry);
        }

        public void Open(string filename)
        {
            Clear();

            var data = File.ReadAllBytes(filename);

            using (var s = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(s))
                {
                    var dirOffset = reader.ReadInt32();
                    var magic = reader.ReadChars(4);
                    if (magic[0] != 'P' || magic[1] != 'F' || magic[2] != 'S' || magic[3] != ' ')
                    {
                        throw new Exception("Not a valid PFS file.");
                    }

                    s.Seek(dirOffset, SeekOrigin.Begin);
                    var dirCount = reader.ReadInt32();
                    for (var i = 0; i < dirCount; ++i)
                    {
                        var crc = reader.ReadInt32();
                        var offset = reader.ReadInt32();
                        var length = reader.ReadInt32();

                        if (crc == 0x61580ac9)
                        {
                            FileNames.Archive = this;
                            FileNames.CRC = crc;
                            FileNames.Length = length;
                            FileNames.Block = GetBlock(data, offset, length);
                        }
                        else
                        {
                            var entry = new PackedFileSystemEntry();
                            entry.Archive = this;
                            entry.CRC = crc;
                            entry.Block = GetBlock(data, offset, length);
                            entry.Length = length;
                            _Entries.Add(entry);
                        }
                    }

                    if (s.Position != s.Length)
                    {
                        FooterEnabled = true;
                        s.Seek(5, SeekOrigin.Current);
                        var ts = reader.ReadInt32();
                        FooterTimestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(ts);
                    }
                }
            }

            PopulateFileNames();
        }

        public void Remove(string filename)
        {
            var entry = _Entries.First(t => t.FileName == filename.ToLower());
            _Entries.Remove(entry);
        }

        public void Rename(string src, string dest)
        {
            var entry = _Entries.First(t => t.FileName == src.ToLower());
            entry.CRC = new CRC().Get(dest.ToLower());
            entry.FileName = dest.ToLower();
        }

        public void SaveAs(string filename)
        {
            RecalcFilenames();
            using (var fs = File.Open(filename, FileMode.OpenOrCreate))
            {
                fs.SetLength(0);
                using (var writer = new BinaryWriter(fs))
                {
                    //Write header
                    writer.Write((Int32)0);
                    writer.Write('P');
                    writer.Write('F');
                    writer.Write('S');
                    writer.Write(' ');
                    writer.Write((Int32)131072);

                    int[] Offsets = new int[_Entries.Count + 1];
                    var i = 0;

                    //Write all Our Blocks
                    foreach (var entry in _Entries)
                    {
                        Offsets[i++] = (int)fs.Position;
                        writer.Write(entry.Block);
                    }

                    //Write Filenames Block
                    Offsets[i++] = (int)fs.Position;
                    writer.Write(FileNames.Block);

                    var dirOffset = (Int32)fs.Position;
                    i = 0;
                    var crc = new CRC();
                    writer.Write((Int32)_Entries.Count + 1);

                    foreach (var entry in _Entries)
                    {
                        //Calc CRC
                        writer.Write((Int32)crc.Get(entry.FileName.ToLower()));
                        writer.Write(Offsets[i++]);
                        writer.Write(entry.Length);
                    }

                    writer.Write((Int32)0x61580ac9);
                    writer.Write(Offsets[i++]);
                    writer.Write(FileNames.Length);

                    fs.Seek(0, SeekOrigin.Begin);
                    writer.Write(dirOffset);

                    if (FooterEnabled)
                    {
                        fs.Seek(0, SeekOrigin.End);
                        writer.Write('S');
                        writer.Write('T');
                        writer.Write('E');
                        writer.Write('V');
                        writer.Write('E');
                        var ts = (int)FooterTimestamp.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                        writer.Write(ts);
                    }
                }
            }
        }

        public byte[] InflateEntry(PackedFileSystemEntry entry)
        {
            using (var s = new MemoryStream(entry.Block))
            {
                using (BinaryReader reader = new BinaryReader(s))
                {
                    byte[] ret = null;
                    int inflate = 0;
                    while (inflate < entry.Length)
                    {
                        var deflate_length = reader.ReadInt32();
                        var inflate_length = reader.ReadInt32();
                        inflate += inflate_length;
                        var block = reader.ReadBytes(deflate_length);

                        if (ret == null)
                        {
                            ret = Compression.Inflate(block);
                        }
                        else
                        {
                            var decompressed = Compression.Inflate(block);
                            byte[] temp = new byte[ret.Length + decompressed.Length];
                            Buffer.BlockCopy(ret, 0, temp, 0, ret.Length);
                            Buffer.BlockCopy(decompressed, 0, temp, ret.Length, decompressed.Length);
                            ret = temp;
                        }
                    }

                    return ret;
                }
            }
        }

        private Tuple<byte[], int> DeflateBlock(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                int totalLength = 0;
                using (var writer = new BinaryWriter(ms))
                {
                    int pos = 0;
                    int remain = data.Length;
                    while (remain > 0)
                    {
                        int sz = 0;
                        if (remain >= 8192)
                        {
                            sz = 8192;
                            remain -= 8192;
                        }
                        else
                        {
                            sz = remain;
                            remain = 0;
                        }

                        var blockData = new byte[sz];
                        Buffer.BlockCopy(data, pos, blockData, 0, sz);
                        var deflatedBlock = Compression.Deflate(blockData);

                        totalLength += sz;
                        pos += sz;
                        writer.Write((Int32)deflatedBlock.Length);
                        writer.Write((Int32)sz);
                        writer.Write(deflatedBlock);
                    }
                }

                return Tuple.Create(ms.ToArray(), totalLength);
            }
        }

        private void RecalcFilenames()
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write((Int32)_Entries.Count);
                    foreach (var entry in _Entries)
                    {
                        writer.Write((Int32)entry.FileName.Length + 1);
                        writer.Write(Encoding.ASCII.GetBytes(entry.FileName.ToLower()));
                        writer.Write('\0');
                    }

                    var block = DeflateBlock(ms.ToArray());
                    FileNames.Block = block.Item1;
                    FileNames.Length = block.Item2;
                }
            }
        }

        private void PopulateFileNames()
        {
            var fn = FileNames.Inflate();
            using (var s = new MemoryStream(fn))
            {
                using (BinaryReader reader = new BinaryReader(s))
                {
                    var crc = new CRC();
                    var count = reader.ReadInt32();
                    for (var i = 0; i < count; ++i)
                    {
                        var length = reader.ReadInt32();
                        var bytes = reader.ReadBytes(length - 1);
                        reader.ReadByte();
                        var name = Encoding.ASCII.GetString(bytes).ToLower();
                        var current_crc = crc.Get(name);

                        var e = _Entries.FirstOrDefault(t => t.CRC == current_crc);
                        if (e != null && e.CRC != 0)
                        {
                            e.FileName = name;
                        }
                    }
                }
            }
        }

        private byte[] GetBlock(byte[] data, int offset, int length)
        {
            using (var s = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(s))
                {
                    s.Seek(offset, SeekOrigin.Begin);
                    int inflate = 0;
                    int total_len = 0;
                    while (inflate < length)
                    {
                        var deflate_length = reader.ReadInt32();
                        var inflate_length = reader.ReadInt32();
                        total_len += 8;
                        total_len += deflate_length;
                        inflate += inflate_length;

                        s.Seek(deflate_length, SeekOrigin.Current);
                    }

                    s.Seek(offset, SeekOrigin.Begin);
                    inflate = 0;
                    int ret_offset = 0;
                    byte[] ret = new byte[total_len];
                    while (inflate < length)
                    {
                        var deflate_length = reader.ReadInt32();
                        var inflate_length = reader.ReadInt32();
                        s.Seek(-8, SeekOrigin.Current);

                        var block_part = reader.ReadBytes(deflate_length + 8);

                        Buffer.BlockCopy(block_part, 0, ret, ret_offset, block_part.Length);
                        ret_offset += block_part.Length;
                        inflate += inflate_length;
                    }

                    return ret;
                }
            }
        }


    }
}
