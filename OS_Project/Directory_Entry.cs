using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Project
{
    internal class Directory_Entry
    {
        public char[] name = new char[11];
        public byte attribute;
        public byte[] empty = new byte[12];
        public int size;
        public int first_cluster;


        public Directory_Entry()
        {

        }
        public Directory_Entry(string n, byte attr, int sz, int fc)
        {
            attribute = attr;
            
            if (attribute == 0)
            {
                if (n.Length > 11)
                {
                    name = (n.Substring(7) + n.Substring(n.Length - 4)).ToCharArray();
                }
                else
                {
                    name = n.ToCharArray();
                }
            }
            else
            {
                name = n.Substring(0, Math.Min(11, n.Length)).ToCharArray();
            }
            name = CleanName(new string(name)).ToArray();
            size = sz;

            first_cluster = fc;
        }

        public static string CleanName(string name)
        {
            string legalCharaters = "abcdefghijklmnopqrstuvwxyz0123456789.";
            if (name != "O:")
            {
                foreach (char c in name)
                {
                    if (!legalCharaters.Contains(c.ToString().ToLower()))
                    {
                        throw new ArgumentException("Invalid File Name");
                    }
                }
            }
            return name;
        }

        public byte[] Convert_Directory_Entry()
        {
            byte[] data = new byte[32];
            for (int i = 0; i < name.Length; i++)
            {
                data[i] = Convert.ToByte(name[i]);
            }
            data[11] = attribute;
            for (int i = 0; i < 12; i++)
            {
                data[i + 12] = empty[i];

            }

            byte[] temp = new byte[4];
            temp = BitConverter.GetBytes(size);
            for (int i = 0; i < 4; i++)
            {
                data[i + 24] = temp[i];

            }

            temp = BitConverter.GetBytes(first_cluster);
            for (int i = 0; i < 4; i++)
            {
                data[i + 28] = temp[i];
            }

            return data;
        }

        public Directory_Entry Get_Directory_Entry(byte[] data)
        {
            Directory_Entry de = new Directory_Entry();
            for (int i = 0; i < 11; i++)
            {
                de.name[i] = Convert.ToChar(data[i]);
            }
            de.attribute = data[11];
            for (int i = 0; i < 12; i++)
            {
                de.empty[i] = data[i + 12];

            }
            byte[] temp = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                temp[i] = data[i + 24];

            }
            de.size = BitConverter.ToInt32(temp, 0);
            for (int i = 0; i < 4; i++)
            {
                temp[i] = data[i + 28];
            }
            de.first_cluster = BitConverter.ToInt32(temp, 0);
            return de;
        }

        public Directory_Entry Get_Directory_Entry()
        {
            Directory_Entry d = new Directory_Entry();
            d.name = name;
            d.attribute = attribute;
            d.empty = empty;
            d.first_cluster = first_cluster;
            d.size = size;

            return d;
        }
    }
}
