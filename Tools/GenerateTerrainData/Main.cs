using System;
using System.IO;

namespace GenerateTerrainData
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            ushort[,] height = new ushort[1024, 1024];

            for (int x=0; x<512; x++)
            {
                for (int y=0; y<512; y++)
                {
                    double dx = x / 512.0 - 1.0;
                    double dy = y / 512.0 - 1.0;
                    double h = Math.Sqrt(dx * dx + dy * dy);
                    ushort sh = (ushort)(Math.Max(0.0, Math.Min(h, 1.0)) * 0xFFFF);
                    height [x, y] = sh;
                    height [x, 1023 - y] = sh;
                    height [1023 - x, y] = sh;
                    height [1023 - x, 1023 - y] = sh;
                }
            }

            using (BinaryWriter writer = new BinaryWriter(File.Open("foo.raw", FileMode.Create)))
            {
                for (int x=0; x<1024; x++)
                {
                    for (int y=0; y<1024; y++)
                    {
                        writer.Write(height [x, y]);
                    }
                }
            }
        }
    }
}
