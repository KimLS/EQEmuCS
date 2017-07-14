using System;

namespace EQEmu.Core.FileSystem
{
    public class PackedFileSystemEntry : IFileSystemEntry
    {
        public string FileName { get; set; }
        public int Length { get; set; }

        public PackedFileSystem Archive { get; set; }
        public Int32 CRC { get; set; }
        public byte[] Block { get; set; }


        public byte[] Inflate()
        {
            return Archive.InflateEntry(this);
        }
    }
}
