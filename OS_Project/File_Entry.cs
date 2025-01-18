using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;

namespace OS_Project
{
    internal class File_Entry : Directory_Entry
    {
        public string content;
        public Directory parent;


        public File_Entry(string name, byte attr, int size, int firstCluster, string cont = "", Directory par = null) : base(name, attr, size, firstCluster)
        {
            parent = par;
            content = cont;
        }

        public void writeFile()
        {

            int contentLength = content.Length;
            int totalClusters = (int)Math.Ceiling(content.Length / 1024.0);
            int fullClusters = content.Length / 1024;
            int remainder = content.Length % 1024;
            

            int firstCluster;
            if (first_cluster != 0)
                firstCluster = first_cluster;
            else
            {
                firstCluster = MiniFat.Get_Available_Cluster();
                first_cluster = firstCluster;
            }

            int lastCluster = -1;

            for (int i = 0; i < totalClusters; i++)
            {
                byte[] ClusterData = new byte[1024];

                if (i < fullClusters)
                {

                    for (int j = 0; j < 1024; j++)
                    {
                        ClusterData[j] = (byte)content[i * 1024 + j];
                    }
                }
                else
                {

                    int start = 1024 * fullClusters;
                    for (int j = 0; j < 1024; j++)
                    {
                        if (j < remainder)
                        {
                            ClusterData[j] = (byte)content[start + j];
                        }
                        else
                        {
                            ClusterData[j] = (byte)'#';
                        }
                    }
                }
                Virtual_Disk.Write_Cluster(ClusterData, firstCluster);
                MiniFat.Set_Value(-1, firstCluster);
                if (lastCluster != -1)
                {
                    MiniFat.Set_Value(firstCluster, lastCluster);
                }
                lastCluster = firstCluster;
                firstCluster = MiniFat.Get_Available_Cluster();
            }

            MiniFat.WriteMiniFat();

        }


        public void readFile()
        {

            List<byte> data = new List<byte>();
            int firstCluster = first_cluster;
            int nextCluster = MiniFat.Get_Value(firstCluster);
            data.AddRange(Virtual_Disk.Read_Cluster(firstCluster));

            while (nextCluster != -1)
            {
                firstCluster = nextCluster;
                if (first_cluster != -1)
                {
                    data.AddRange(Virtual_Disk.Read_Cluster(firstCluster));
                    nextCluster = MiniFat.Get_Value(firstCluster);
                }
            }


            for (int i = 0; i < data.Count; i++)
            {
                if (data[i] == (byte)'#')
                {

                    break;
                }

                content += (char)data[i];

            }
        }

        public void deleteFile(string name)
        {
            if (first_cluster != 0)
            {
                int firstCluster = first_cluster;
                int next = MiniFat.Get_Value(firstCluster);

                do
                {
                    MiniFat.Set_Value(0, firstCluster);
                    firstCluster = next;
                    if (firstCluster != -1)
                        next = MiniFat.Get_Value(firstCluster);

                } while (firstCluster != -1);

                MiniFat.WriteMiniFat();
            }

            int delIndex = parent.Search(name);
            parent.directoryTable.RemoveAt(delIndex);
            parent.Write_Directory();
        }
    }
}