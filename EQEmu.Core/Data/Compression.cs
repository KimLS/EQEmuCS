using Ionic.Zlib;
using System;
using System.IO;

namespace EQEmu.Core.Data
{
    public static class Compression
    {
        public static byte[] Deflate(byte[] input)
        {
            int outputSize = 8192;
            byte[] output = new Byte[outputSize];
            int lengthToCompress = input.Length;

            using (MemoryStream ms = new MemoryStream())
            {
                ZlibCodec compressor = new ZlibCodec();
                compressor.InitializeDeflate(Ionic.Zlib.CompressionLevel.BestCompression, true);

                compressor.InputBuffer = input;
                compressor.AvailableBytesIn = lengthToCompress;
                compressor.NextIn = 0;
                compressor.OutputBuffer = output;

                foreach (var f in new FlushType[] { FlushType.None, FlushType.Finish })
                {
                    int bytesToWrite = 0;
                    do
                    {
                        compressor.AvailableBytesOut = outputSize;
                        compressor.NextOut = 0;
                        compressor.Deflate(f);

                        bytesToWrite = outputSize - compressor.AvailableBytesOut;
                        if (bytesToWrite > 0)
                            ms.Write(output, 0, bytesToWrite);
                    }
                    while (bytesToWrite != 0);
                }

                compressor.EndDeflate();

                ms.Flush();
                return ms.ToArray();
            }
        }

        public static byte[] Inflate(byte[] input)
        {
            int outputSize = 8192;
            byte[] output = new Byte[outputSize];

            using (MemoryStream ms = new MemoryStream())
            {
                ZlibCodec compressor = new ZlibCodec();
                compressor.InitializeInflate(true);

                compressor.InputBuffer = input;
                compressor.AvailableBytesIn = input.Length;
                compressor.NextIn = 0;
                compressor.OutputBuffer = output;

                foreach (var f in new FlushType[] { FlushType.None, FlushType.Finish })
                {
                    int bytesToWrite = 0;
                    do
                    {
                        compressor.AvailableBytesOut = outputSize;
                        compressor.NextOut = 0;
                        compressor.Inflate(f);

                        bytesToWrite = outputSize - compressor.AvailableBytesOut;
                        if (bytesToWrite > 0)
                            ms.Write(output, 0, bytesToWrite);
                    }
                    while (bytesToWrite != 0);
                }

                compressor.EndInflate();
                return ms.ToArray();
            }
        }
    }
}
