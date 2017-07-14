using System.Collections.Generic;

namespace EQEmu.Core.FileSystem
{
    public interface IFileSystem
    {
        IEnumerable<IFileSystemEntry> Entries { get; set; }
        void Open(string filename);
        void SaveAs(string filename);
        void Insert(string filename, string path);
        void Insert(string filename, byte[] data);
        void Remove(string filename);
        void Rename(string src, string dest);
        void Clear();
    }
}
