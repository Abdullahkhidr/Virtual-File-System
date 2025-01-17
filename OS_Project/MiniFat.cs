using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OS_Project
{
    internal class MiniFat
    {
        private static int[] FAT = new int[1024];

        public static void Initialize()
        {
            FAT[0] = -1;
            for (int i = 1; i < 4; i++)
            {
                FAT[i] = i + 1;
            }

            FAT[4] = -1;
            for (int i = 5; i < 1023; i++)
            {
                FAT[i] = 0;
            }
        }


        public static void WriteMiniFat()
        {
            byte[] buffer = new byte[Virtual_Disk.clusterSize * 4];
            Buffer.BlockCopy(FAT, 0, buffer, 0, FAT.Length);

            using (FileStream disk = new FileStream(Virtual_Disk.fileName, FileMode.Open, FileAccess.ReadWrite))
            {
                disk.Seek(1024, SeekOrigin.Begin);
                disk.Write(buffer, 0, buffer.Length);
            }
        }



        public static void ReadMiniFat()
        {
            byte[] buffer = new byte[Virtual_Disk.clusterSize * 4];

            using (FileStream disk = new FileStream(Virtual_Disk.fileName, FileMode.Open, FileAccess.ReadWrite))
            {
                disk.Seek(1024, SeekOrigin.Begin);

                disk.Read(buffer, 0, buffer.Length);
            }
            Buffer.BlockCopy(buffer, 0, FAT, 0, buffer.Length);
        }



        public static void PrintMiniFat()
        {
            Console.WriteLine("Fat Table: ");
            for (int i = 0; i < FAT.Length; i++)
            {
                Console.WriteLine($"Block {i}: {FAT[i]}");
            }
        }


        public static int Get_Available_Block()
        {
            for (int i = 0; i < FAT.Length; i++)
            {
                if (FAT[i] == 0)
                {
                    return i;
                }
            }
            return -1;
        }


        public static int Get_Value(int index)
        {
            return FAT[index];
        }


        public static void Set_Value(int value, int index)
        {
            FAT[index] = value;
        }

        public static int Get_Number_Of_Free_Blocks()
        {
            int count = 0;
            foreach (int f in FAT)
            {
                if (f == 0)
                {
                    count++;
                }
            }
            return count;
        }

        public static int Get_Free_Space()
        {
            return Get_Number_Of_Free_Blocks() * 1024;
        }

    }
}
