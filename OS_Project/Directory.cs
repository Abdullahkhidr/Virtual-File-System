﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Project
{
    internal class Directory : Directory_Entry
    {
        public List<Directory_Entry> directoryTable = new List<Directory_Entry>();
        public Directory parent;


        public Directory() : base() { }
        public Directory(string n, byte bt, int sz, int fc, Directory pt) : base(n, bt, sz, fc)
        {
            parent = pt;
        }

        public void Write_Directory()
        {
            byte[] directory_table = new byte[32 * directoryTable.Count];
            byte[] directory_entry = new byte[32];


            for (int i = 0; i < directoryTable.Count; i++)
            {
                directory_entry = directoryTable[i].Convert_Directory_Entry();
                for (int j = i * 32; j < (i + 1) * 32; j++)
                {
                    directory_table[j] = directory_entry[j % 32];
                }
            }

            int mx = Math.Max(directory_table.Length, 1);
            int totalBlocks = (int)Math.Ceiling(mx / 1024.0);
            int fullBlocks = directory_table.Length / 1024;
            int remainder = directory_table.Length % 1024;

            int fc;
            if (first_cluster != 0)
                fc = first_cluster;
            else
            {
                fc = MiniFat.Get_Available_Block();
                first_cluster = fc;
            }

            int lc = -1;

            for (int i = 0; i < totalBlocks; i++)
            {
                byte[] blockData = new byte[1024];
                if (i < fullBlocks)
                {
                    for (int j = 0; j < 1024; j++)
                    {
                        blockData[j] = directory_table[i * 1024 + j];
                    }
                }
                else
                {
                    int indx = 1024 * fullBlocks;
                    for (int j = 0; j < 1024; j++)
                    {
                        if (j < remainder)
                        {
                            blockData[j] = directory_table[indx + j];
                        }
                        else
                        {
                            blockData[j] = (byte)'#';
                        }

                    }
                }

                Virtual_Disk.Write_Block(blockData, fc);
                MiniFat.Set_Value(-1, fc);
                if (lc != -1)
                {
                    MiniFat.Set_Value(fc, lc);
                }
                lc = fc;
                fc = MiniFat.Get_Available_Block();

            }

            MiniFat.WriteMiniFat();
        }


        public void Read_Directory()
        {
            if (first_cluster != 0)
            {
                List<byte> data = new List<byte>();
                List<Directory_Entry> directory_table = new List<Directory_Entry>();
                int fc = first_cluster;
                int nc = MiniFat.Get_Value(fc);
                data.AddRange(Virtual_Disk.Read_Block(fc));

                while (nc != -1)
                {
                    fc = nc;
                    if (first_cluster != -1)
                    {
                        data.AddRange(Virtual_Disk.Read_Block(fc));
                        nc = MiniFat.Get_Value(fc);
                    }
                }

                bool flag = false;
                for (int i = 0; i < data.Count / 32; i++)
                {
                    byte[] temp = new byte[32];
                    for (int j = 0; j < 32; j++)
                    {
                        size = i * 32 + j;
                        if (data[i * 32 + j] == (byte)'#')
                        {
                            flag = true;
                            break;
                        }
                        temp[j] = data[i * 32 + j];
                    }
                    if (flag)
                    {
                        break;
                    }
                    directory_table.Add(Get_Directory_Entry(temp));
                }
                directoryTable = directory_table;
            }
        }

        public int Search(string n)
        {
            string s;
            for (int i = 0; i < directoryTable.Count; i++)
            {
                s = new string(directoryTable[i].name).TrimEnd('\0');
                if (s == n.TrimEnd('\0'))
                {
                    return i;
                }
            }
            return -1;

        }

        public void Delete_Directory(string name)
        {
            if (first_cluster != 0)
            {
                int fc = first_cluster;
                int next = MiniFat.Get_Value(fc);

                do
                {
                    MiniFat.Set_Value(0, fc);
                    fc = next;
                    if (fc != -1)
                        next = MiniFat.Get_Value(fc);

                } while (fc != -1);

                MiniFat.WriteMiniFat();
            }

            int y = parent.Search(name);
            parent.directoryTable.RemoveAt(y);
            parent.Write_Directory();
        }



        public void Update_Content(Directory_Entry d)
        {
            string file_name = new string(d.name);
            Read_Directory();
            int index = Search(file_name);
            if (index != -1)
            {
                directoryTable.RemoveAt(index);
                directoryTable.Insert(index, d);
            }
            Write_Directory();
        }
        public string GetCurrentPath()
        {
            List<string> pathParts = new List<string>();
            Directory current = this;

            while (current != null)
            {
                pathParts.Insert(0, new string(current.name));
                current = current.parent;
            }

            return string.Join("\\", pathParts);
        }
    }
}
