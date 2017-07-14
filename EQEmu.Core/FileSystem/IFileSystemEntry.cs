namespace EQEmu.Core.FileSystem
{
    public interface IFileSystemEntry
    {
        string FileName { get; set; }
        int Length { get; set; }
        byte[] Inflate();
    }
}
