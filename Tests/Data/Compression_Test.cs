using Shouldly;
using System.Text;
using Xunit;

namespace Tests.Data
{
    public class Compression_Test
    {
        [Fact]
        public void Should_Decompress_Static_String()
        {
            byte[] input = { 120, 94, 99, 96, 96, 96, 96, 100, 98, 102, 97, 5, 2, 32, 147, 1, 0, 0, 246, 0, 31, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            var output = EQEmu.Core.Data.Compression.Inflate(input);
            output.Length.ShouldBe(16);
            output[0].ShouldBe((byte)0);
            output[1].ShouldBe((byte)0);
            output[2].ShouldBe((byte)0);
            output[3].ShouldBe((byte)0);
            output[4].ShouldBe((byte)1);
            output[5].ShouldBe((byte)2);
            output[6].ShouldBe((byte)3);
            output[7].ShouldBe((byte)4);
            output[8].ShouldBe((byte)5);
            output[9].ShouldBe((byte)5);
            output[10].ShouldBe((byte)5);
            output[11].ShouldBe((byte)5);
            output[12].ShouldBe((byte)0);
            output[13].ShouldBe((byte)0);
            output[14].ShouldBe((byte)0);
            output[15].ShouldBe((byte)0);
        }

        [Fact]
        public void Should_Compress_And_Decompress_Data()
        {
            string inputStr = "TestString";
            var inputData = Encoding.ASCII.GetBytes(inputStr);
            var data = EQEmu.Core.Data.Compression.Deflate(inputData);
            data.ShouldNotBeNull();

            var outputData = EQEmu.Core.Data.Compression.Inflate(data);
            outputData.ShouldNotBeNull();

            var outputString = Encoding.ASCII.GetString(outputData);

            outputString.ShouldBe(inputStr);
        }
    }
}
