using System;
using System.Text;

namespace EQEmu.Core.Data
{
    public class CRC
    {
        private Int32[] crcTable = null;
        private const Int32 polynomial = 0x04C11DB7;

        public CRC()
        {
            GenerateCRCTable();
        }

        private void GenerateCRCTable()
        {
            crcTable = new Int32[256];
            Int32 i = 0, j = 0, crcAccum = 0;
            for (i = 0; i < 256; ++i)
            {
                crcAccum = i << 24;
                for (j = 0; j < 8; ++j)
                {
                    if ((crcAccum & 0x80000000) != 0)
                    {
                        crcAccum = (crcAccum << 1) ^ polynomial;
                    }
                    else
                    {
                        crcAccum = crcAccum << 1;
                    }
                }
                crcTable[i] = crcAccum;
            }
        }

        public Int32 Update(Int32 crc, byte[] data, int length)
        {
            Int32 i = 0;
            Int32 idx = 0;
            while (length > 0)
            {
                i = ((crc >> 24) ^ data[idx]) & 0xFF;
                idx += 1;
                crc = (crc << 8) ^ crcTable[i];
                length -= 1;
            }

            return crc;
        }

        public Int32 Get(string input)
        {
            if (input.Length == 0)
            {
                return 0;
            }

            byte[] buf = new byte[input.Length + 1];
            var stringBuffer = Encoding.ASCII.GetBytes(input);
            Buffer.BlockCopy(stringBuffer, 0, buf, 0, stringBuffer.Length);

            return Update(0, buf, buf.Length);
        }

        public Int32 Get(byte[] input)
        {
            if (input.Length == 0)
            {
                return 0;
            }

            return Update(0, input, input.Length);
        }
    }
}
