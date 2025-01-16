using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;

namespace OS_Project
{
    internal class Virtual_Disk
    {
        public const string fileName = "Disk.txt";
        public const int blockSize = 1024;
        public const int totalBlocks = 1024 * 1024;
        private const byte superBlock = (byte)'0';
        private const byte FAT = (byte)'*';
        private const byte freeBlock = (byte)'#';

        public static void Initialize()
        {

            Directory Root = new Directory("O:",1, 0, 5, null);
            Program.currentDirectory = Root;
            Program.path = new string(Root.name);
            if (!File.Exists(fileName))
            {
                using (FileStream disk = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    for (int i = 0; i < blockSize; i++)
                    {
                        disk.WriteByte(superBlock);
                    }
                   
                    for (int j = 0; j < blockSize * 4; j++)
                    {
                        disk.WriteByte(FAT);
                    }

                    for (int i = 0; i < (blockSize - 5) * blockSize; i++)
                    {
                      
                        disk.WriteByte(freeBlock);
                        
                    }
                }

                MiniFat.Initialize();

                Root.Write_Directory();
                MiniFat.WriteMiniFat();
            }
            else
            {
                MiniFat.ReadMiniFat();
                Root.Read_Directory();
            }
        }

        public static void Write_Block(byte[] data, int index)
        {
            using (FileStream disk = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
            {
                disk.Seek(1024 * index, SeekOrigin.Begin);
                disk.Write(data, 0, data.Length);
            }
        }

        public static byte[] Read_Block(int index)
        {
            byte[] data = new byte[blockSize];
            using (FileStream disk = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
            {
                disk.Seek(1024 * index, SeekOrigin.Begin);
                disk.Read(data, 0, data.Length);
            }
            return data;
        }
    }
}
