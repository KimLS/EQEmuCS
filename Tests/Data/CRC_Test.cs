using Shouldly;
using Xunit;

namespace Tests.Data
{
    public class CRC_Test
    {
        [Fact]
        public void Should_Generate_Correct_Value_Str1()
        {
            var crc = new EQEmu.Core.Data.CRC();
            crc.Get("TestStringOne").ShouldBe(-2069382315);
        }

        [Fact]
        public void Should_Generate_Correct_Value_Str2()
        {
            var crc = new EQEmu.Core.Data.CRC();
            crc.Get("TestStringTwo").ShouldBe(-771530496);
        }
    }
}
