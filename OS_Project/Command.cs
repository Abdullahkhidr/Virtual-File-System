﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace OS_Project
{
    internal class Command
    {
        static Dictionary<string, string> commands = new Dictionary<string, string> {
            { "cd", "Displays the name of or changes the current directory" },
            { "cls", "Clear the console screen" },
            { "dir", "Displays a list of files and subdirectories in a directory." },
            { "exit", "Quits the CMD.EXE program (command interpreter)." },
            { "copy", "Copies one or more files to another location." },
            { "del", "Deletes one or more files." },
            { "help", "Display help for all commands or a specific command" },
            { "md", "Creates a directory." },
            { "rd", "Removes a directory." },
            { "rename", "Renames a file or files." },
            { "type", "Displays the contents of a text file." },
            { "import", "Import text file(s) from your computer." },
            { "export", "Export text file(s) to your computer." },  };
        public static void DisplayAllCommandsHelp()
        {
            foreach (var command in commands)
            {
                Console.WriteLine($"{command.Key}\t{command.Value}\n");
            }
        }

        public static void DisplayCommandHelp(string specificCommand)
        {
            if (commands.ContainsKey(specificCommand))
            {
                Console.WriteLine($"{specificCommand}\t\t{commands[specificCommand]}");
            }
            else
            {
                Console.WriteLine($"Command '{specificCommand}' is not supported.");
            }
        }

        public static void Cls()
        {
            Console.Clear();
        }

        public static void Exit()
        {
            Environment.Exit(0);
        }

        public static void Make_Directory(string name)
        {
            int index = Program.currentDirectory.Search(name);
            if (index != -1)
            {
                Console.WriteLine($"The directory '{name}' is already exists.");
            }
            else
            {
                Directory_Entry newDir = new Directory_Entry(name, 1, 0, 0);
                Program.currentDirectory.directoryTable.Add(newDir);
                Program.currentDirectory.Write_Directory();

                if (Program.currentDirectory.parent != null)
                {
                    Program.currentDirectory.parent.Update_Content(Program.currentDirectory.Get_Directory_Entry());
                }

                Console.WriteLine("Directory created successfully.");
            }
        }



        public static void Remove_Directory(string name)
        {
            int index = Program.currentDirectory.Search(name);
            if (index == -1)
            {
                Console.WriteLine("The directory is not found.");
            }
            else
            {
                Directory_Entry entry = Program.currentDirectory.directoryTable[index];
                if (entry.attribute == 1)
                {

                    Directory newDir = new Directory(name, 1, 0, entry.first_cluster, Program.currentDirectory);
                    newDir.Delete_Directory(name);
                    Program.currentDirectory.Write_Directory();
                    Console.WriteLine("Directory deleted successfully.");
                }
                else
                {
                    Console.WriteLine("Error: The specified name is not a directory.");
                }
            }
        }



        public static void Display_Directory(List<string> pathParts)
        {
            try
            {
                var directory = getDirectory(pathParts);
                Console.WriteLine($"\nDirectory of {directory.GetCurrentPath()}");
                Console.WriteLine("============");
                int numOfFiles = 0, numOfDirs = 0;
                foreach (var entry in directory.directoryTable)
                {
                    if (entry.attribute == 0)
                    {
                        numOfFiles++;
                    }
                    else
                    {
                        numOfDirs++;
                    }
                    Console.WriteLine($"{(entry.attribute == 0 ? entry.size.ToString() : "<DIR>")}\t{new string(entry.name)}");
                }
                Console.WriteLine("-------------------");
                Console.WriteLine($"{numOfFiles} File(s)\t {directory.size} bytes");
                Console.WriteLine($"{numOfDirs} Dir(s)\t {MiniFat.Get_Free_Space()} bytes free\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("Invalid Path Directory");
            }
        }


        public static void Rename(string oldName, string newName)
        {
            int index_oldName = Program.currentDirectory.Search(oldName);
            int index_newName = Program.currentDirectory.Search(newName);
            if (index_oldName != -1)
            {
                if (index_newName == -1)
                {
                    Directory_Entry entry = Program.currentDirectory.directoryTable[index_oldName];

                    entry.name = newName.ToCharArray();
                    Program.currentDirectory.Write_Directory();
                    Console.WriteLine($"Directory '{oldName}' renamed to '{newName}' successfully.");

                }
                else
                {
                    Console.WriteLine($"'{newName}' is the same as the old name.");
                }
            }
            else
            {
                Console.WriteLine("The specified name is not a file.");
            }
        }

        public static void Type(string name)
        {
            int index = Program.currentDirectory.Search(name);
            if (index != -1)
            {
                int fc = Program.currentDirectory.directoryTable[index].first_cluster;
                int sz = Program.currentDirectory.directoryTable[index].size;
                File_Entry f = new File_Entry(name, 0, sz, fc, "", Program.currentDirectory);
                f.Read_File();

                Console.WriteLine(f.content);
            }
            else
            {
                Console.WriteLine($"Failed: There is no a file named '{name}'");
            }

        }

        public static void Delete_File(string name)
        {

            int index = Program.currentDirectory.Search(name);
            if (index != -1)
            {
                int fc = Program.currentDirectory.directoryTable[index].first_cluster;
                int sz = Program.currentDirectory.directoryTable[index].size;
                File_Entry f = new File_Entry(name, 0, sz, fc, "", Program.currentDirectory);
                f.Delete_File(name);
                Console.WriteLine("File deleted successfully.");
            }

            else
            {
                Console.WriteLine("Error: The specified name is not a file.");
            }


        }



        public static void Change_Directory(string name)
        {
            if (name == "..")
            {
                if (Program.currentDirectory.parent != null)
                {
                    Program.currentDirectory = Program.currentDirectory.parent;
                    int index_slash = Program.path.LastIndexOf("\\");
                    Program.path = Program.path.Substring(0, index_slash);
                    Console.WriteLine("Changed to parent directory: " + new string(Program.currentDirectory.name));
                }
                else
                {
                    Console.WriteLine("Already in the root directory.");
                }
            }
            else
            {
                int index = Program.currentDirectory.Search(name);
                if (index != -1)
                {
                    Directory_Entry entry = Program.currentDirectory.directoryTable[index];
                    Directory newDir = new Directory(name, 1, 0, entry.first_cluster, Program.currentDirectory);
                    Program.currentDirectory = newDir;
                    Program.path += "\\" + name;
                    Program.currentDirectory.Read_Directory();

                }
                else
                {
                    Console.WriteLine($"Error: Directory '{name}' not found.");
                }
            }
        }




        public static void Import(string path)
        {
            if (System.IO.File.Exists(path))
            {
                string fileName = path.Split('\\').Last();
                string fileContent = System.IO.File.ReadAllText(path);
                int size = fileContent.Length;
                int index = Program.currentDirectory.Search(fileName);
                int first_cluster = 0;

                if (index == -1)
                {
                    File_Entry f = new File_Entry(fileName, 0, size, first_cluster, fileContent, Program.currentDirectory);
                    f.Write_File();
                    Directory_Entry d = new Directory_Entry(fileName, 0, size, f.first_cluster);
                    Program.currentDirectory.directoryTable.Add(d);
                    Program.currentDirectory.Write_Directory();

                    if (Program.currentDirectory.parent != null)
                    {
                        Program.currentDirectory.parent.Update_Content(Program.currentDirectory.Get_Directory_Entry());
                    }
                }
                else
                {
                    Console.WriteLine("Error: File with the same name already exists.");
                }
            }
            else
            {
                Console.WriteLine("Error: The specified name is not a file.");
            }

        }


        public static void Export(string name, string dest)
        {

            int index = Program.currentDirectory.Search(name);
            if (index != -1)
            {

                if (System.IO.Directory.Exists(dest))
                {
                    int fc = Program.currentDirectory.directoryTable[index].first_cluster;
                    int sz = Program.currentDirectory.directoryTable[index].size;
                    File_Entry f = new File_Entry(name, 0, sz, fc, "", Program.currentDirectory);
                    f.Read_File();
                    using (StreamWriter sw = new StreamWriter(dest + '\\' + name))
                    {
                        sw.WriteLine(f.content);
                    }
                }
                else
                {
                    Console.WriteLine("File not found at the specified path.");
                }

            }
            else
            {
                Console.WriteLine("Error: The specified name is not a file.");
            }
        }


        public static void Copy(string src, string dest)
        {
            string[] pathParts = dest.Split('\\');
            int index_src = Program.currentDirectory.Search(src);
            int index_dest = Program.currentDirectory.Search(pathParts[0]);
            if (index_src == -1)
            {
                Console.WriteLine($"Error: Source file '{src}' not found.");
                return;
            }

            if (index_dest == -1)
            {
                Console.WriteLine($"Error: Destination directory '{dest}' not found.");
                return;
            }
            if (index_src != -1)
            {
                int fc = Program.currentDirectory.directoryTable[index_dest].first_cluster;
                Directory d = new Directory(dest, 1, 0, fc, Program.currentDirectory);
                d.Read_Directory();
                Directory_Entry de = Program.currentDirectory.directoryTable[index_src];
                int indx3 = d.Search(src);
                if (indx3 != -1)
                {
                    Console.WriteLine($"Error: File '{src}' already exists in '{dest}'.");
                    return;
                }
                d.directoryTable.Add(de);
                d.Write_Directory();
                if (d.parent != null)
                {
                    d.parent.Update_Content(d.Get_Directory_Entry());
                }
            }
            else
            {
                Console.WriteLine("Error: Destination directory not found.");
            }
        }

        static private Directory getDirectory(List<string> path)
        {
            Directory current = Program.currentDirectory;
            current.Read_Directory();
            if (path.Count == 0)
            {
                return current;
            }
            var isAbsPath = path[0] == "o:";
            if (isAbsPath)
            {
                current = Program.Root;
                current.Read_Directory();
            }
            for (int i = isAbsPath ? 1 : 0; i < path.Count; i++)
            {
                if (path[i] == "..")
                {
                    if (current.parent == null)
                    {
                        throw new Exception($"Invalid Name Dir {path[i]}");
                    }
                    else
                    {
                        current = current.parent;
                        continue;
                    }
                }
                var index = current.Search(path[i]);
                if (index == -1)
                {
                    throw new Exception($"Invalid Name Dir {path[i]}");
                }
                else
                {
                    var dirEntry = current.directoryTable[index];
                    current = new Directory(path[i], 1, 0, dirEntry.first_cluster, current);
                }
                current.Read_Directory();
            }

            return current;
        }
    }

}