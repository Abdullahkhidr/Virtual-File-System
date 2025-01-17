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
        public const string fileName = "file-system.txt";
        public const int clusterSize = 1024;
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
                    for (int i = 0; i < clusterSize; i++)
                    {
                        disk.WriteByte(superBlock);
                    }
                   
                    for (int j = 0; j < clusterSize * 4; j++)
                    {
                        disk.WriteByte(FAT);
                    }

                    for (int i = 0; i < (clusterSize - 5) * clusterSize; i++)
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
            byte[] data = new byte[clusterSize];
            using (FileStream disk = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
            {
                disk.Seek(1024 * index, SeekOrigin.Begin);
                disk.Read(data, 0, data.Length);
            }
            return data;
        }
    }
}
