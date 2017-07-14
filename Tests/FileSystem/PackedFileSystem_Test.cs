using Shouldly;
using System.Linq;
using System.Text;
using Xunit;

namespace Tests.FileSystem
{
    public class PackedFileSystem_Test
    {
        [Fact]
        public void Should_Open_Simple_Archive()
        {
            var archive = EQEmu.Core.FileSystem.PackedFileSystem.Create();
            archive.Open("test.eqg");
            archive.Entries.Count().ShouldBe(4);
            archive.Entries.Count(t => t.FileName == "test1.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test2.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test3.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test4.txt").ShouldBe(1);

            var test1 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test1.txt").Inflate());
            var test2 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test2.txt").Inflate());
            var test3 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test3.txt").Inflate());
            var test4 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test4.txt").Inflate());

            test1.ShouldNotBeNull();
            test1.ShouldNotBeEmpty();

            test2.ShouldNotBeNull();
            test2.ShouldNotBeEmpty();

            test3.ShouldNotBeNull();
            test3.ShouldNotBeEmpty();

            test4.ShouldNotBeNull();
            test4.ShouldNotBeEmpty();
        }

        [Fact]
        public void Should_Save_As()
        {
            var archive = EQEmu.Core.FileSystem.PackedFileSystem.Create();
            archive.Open("test.eqg");
            archive.SaveAs("saved.eqg");
            archive.Open("saved.eqg");

            archive.Entries.Count().ShouldBe(4);
            archive.Entries.Count(t => t.FileName == "test1.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test2.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test3.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test4.txt").ShouldBe(1);

            var test1 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test1.txt").Inflate());
            var test2 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test2.txt").Inflate());
            var test3 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test3.txt").Inflate());
            var test4 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test4.txt").Inflate());

            test1.ShouldNotBeNull();
            test1.ShouldNotBeEmpty();

            test2.ShouldNotBeNull();
            test2.ShouldNotBeEmpty();

            test3.ShouldNotBeNull();
            test3.ShouldNotBeEmpty();

            test4.ShouldNotBeNull();
            test4.ShouldNotBeEmpty();
        }

        [Fact]
        public void Should_Add_Files()
        {
            var archive = EQEmu.Core.FileSystem.PackedFileSystem.Create();
            archive.Open("test.eqg");
            archive.Insert("TestFile1.txt", "TestFile1.txt");
            archive.Insert("TestFile2.txt", "TestFile2.txt");

            archive.SaveAs("added.eqg");
            archive.Open("added.eqg");

            archive.Entries.Count().ShouldBe(6);
            archive.Entries.Count(t => t.FileName == "test1.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test2.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test3.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test4.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "testfile1.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "testfile2.txt").ShouldBe(1);

            var test1 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test1.txt").Inflate());
            var test2 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test2.txt").Inflate());
            var test3 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test3.txt").Inflate());
            var test4 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test4.txt").Inflate());
            var testfile1Data = archive.Entries.First(t => t.FileName == "testfile1.txt").Inflate();
            var testfile1 = Encoding.ASCII.GetString(testfile1Data);
            var testfile2Data = archive.Entries.First(t => t.FileName == "testfile2.txt").Inflate();
            var testfile2 = Encoding.ASCII.GetString(testfile2Data);

            test1.ShouldNotBeNull();
            test1.ShouldNotBeEmpty();

            test2.ShouldNotBeNull();
            test2.ShouldNotBeEmpty();

            test3.ShouldNotBeNull();
            test3.ShouldNotBeEmpty();

            test4.ShouldNotBeNull();
            test4.ShouldNotBeEmpty();

            testfile1.ShouldNotBeNull();
            testfile1.ShouldNotBeEmpty();
            testfile1.Contains("Test File 1").ShouldBe(true);
            testfile1Data.Length.ShouldBe(11);

            testfile2.ShouldNotBeNull();
            testfile2.ShouldNotBeEmpty();
            testfile2.Contains("Test File 2").ShouldBe(true);
            testfile2Data.Length.ShouldBe(9358);
        }

        [Fact]
        public void Should_Replace_Files()
        {
            var archive = EQEmu.Core.FileSystem.PackedFileSystem.Create();
            archive.Open("test.eqg");
            archive.Insert("TestFile1.txt", "TestFile1.txt");
            archive.Insert("TestFile2.txt", "TestFile2.txt");
            archive.Insert("TestFile1.txt", "TestFile2.txt");

            archive.SaveAs("replace.eqg");
            archive.Open("replace.eqg");

            archive.Entries.Count().ShouldBe(6);
            archive.Entries.Count(t => t.FileName == "test1.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test2.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test3.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test4.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "testfile1.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "testfile2.txt").ShouldBe(1);

            var test1 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test1.txt").Inflate());
            var test2 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test2.txt").Inflate());
            var test3 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test3.txt").Inflate());
            var test4 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test4.txt").Inflate());
            var testfile1Data = archive.Entries.First(t => t.FileName == "testfile1.txt").Inflate();
            var testfile1 = Encoding.ASCII.GetString(testfile1Data);
            var testfile2Data = archive.Entries.First(t => t.FileName == "testfile2.txt").Inflate();
            var testfile2 = Encoding.ASCII.GetString(testfile2Data);

            test1.ShouldNotBeNull();
            test1.ShouldNotBeEmpty();

            test2.ShouldNotBeNull();
            test2.ShouldNotBeEmpty();

            test3.ShouldNotBeNull();
            test3.ShouldNotBeEmpty();

            test4.ShouldNotBeNull();
            test4.ShouldNotBeEmpty();

            testfile1.ShouldNotBeNull();
            testfile1.ShouldNotBeEmpty();
            testfile1.Contains("Test File 2").ShouldBe(true);
            testfile1Data.Length.ShouldBe(9358);

            testfile2.ShouldNotBeNull();
            testfile2.ShouldNotBeEmpty();
            testfile2.Contains("Test File 2").ShouldBe(true);
            testfile2Data.Length.ShouldBe(9358);
        }

        [Fact]
        public void Should_Rename()
        {
            var archive = EQEmu.Core.FileSystem.PackedFileSystem.Create();
            archive.Open("test.eqg");
            archive.Rename("test1.txt", "test5.txt");
            archive.Rename("test2.txt", "test6.txt");
            archive.Rename("test3.txt", "test7.txt");
            archive.Rename("test4.txt", "test8.txt");

            archive.SaveAs("rename.eqg");
            archive.Open("rename.eqg");

            archive.Entries.Count().ShouldBe(4);
            archive.Entries.Count(t => t.FileName == "test5.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test6.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test7.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test8.txt").ShouldBe(1);

            var test1 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test5.txt").Inflate());
            var test2 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test6.txt").Inflate());
            var test3 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test7.txt").Inflate());
            var test4 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test8.txt").Inflate());

            test1.ShouldNotBeNull();
            test1.ShouldNotBeEmpty();

            test2.ShouldNotBeNull();
            test2.ShouldNotBeEmpty();

            test3.ShouldNotBeNull();
            test3.ShouldNotBeEmpty();

            test4.ShouldNotBeNull();
            test4.ShouldNotBeEmpty();
        }

        [Fact]
        public void Should_Delete()
        {
            var archive = EQEmu.Core.FileSystem.PackedFileSystem.Create();
            archive.Open("test.eqg");
            archive.Remove("test1.txt");

            archive.SaveAs("delete.eqg");
            archive.Open("delete.eqg");

            archive.Entries.Count().ShouldBe(3);
            archive.Entries.Count(t => t.FileName == "test2.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test3.txt").ShouldBe(1);
            archive.Entries.Count(t => t.FileName == "test4.txt").ShouldBe(1);

            var test2 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test2.txt").Inflate());
            var test3 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test3.txt").Inflate());
            var test4 = Encoding.ASCII.GetString(archive.Entries.First(t => t.FileName == "test4.txt").Inflate());

            test2.ShouldNotBeNull();
            test2.ShouldNotBeEmpty();

            test3.ShouldNotBeNull();
            test3.ShouldNotBeEmpty();

            test4.ShouldNotBeNull();
            test4.ShouldNotBeEmpty();
        }
    }
}
