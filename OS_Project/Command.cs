using System;
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
            { "cd", "Displays the name of or changes the current directory.\nUsage:\n  cd [directory]\n  - If no directory is specified, it displays the current directory.\n  - Use '..' to move up one directory level." }, 
            // Done
            { "cls", "Clears the console screen.\nUsage:\n  cls\n  - This command clears all text from the console window." }, 
            // Done
            { "dir", "Displays a list of files and subdirectories in a directory.\nUsage:\n  dir [directory]\n  - If no directory is specified, it lists the contents of the current directory.\n  - Use a file path to list the contents of a specific directory." }, 
            // Done
            { "exit", "Quits the CMD.EXE program (command interpreter).\nUsage:\n  exit\n  - This command closes the console window or terminates the current session." },
            // Done
            { "copy", "Copies one or more files to another location.\nUsage:\n  copy [source] [destination]\n  - [source]: The file(s) to copy.\n  - [destination]: The location where the file(s) will be copied.\n  - Example: copy file.txt C:\\Backup" },
            // 
            { "del", "Deletes one or more files.\nUsage:\n  del [file]\n  - [file]: The file(s) to delete.\n  - Example: del file.txt\n  - Use caution as deleted files cannot be recovered." }, 
            // Done
            { "help", "Displays help for all commands or a specific command.\nUsage:\n  help [command]\n  - If no command is specified, it lists all available commands.\n  - Example: help cd (provides details about the 'cd' command)." }, 
            // Done
            { "md", "Creates a directory.\nUsage:\n  md [directory_name]\n  - [directory_name]: The name of the directory to create.\n  - Example: md NewFolder" }, 
            // Done
            { "rd", "Removes a directory.\nUsage:\n  rd [directory_name]\n  - [directory_name]: The name of the directory to remove.\n  - Example: rd OldFolder\n  - Note: The directory must be empty before it can be removed." }, 
            // Done
            { "rename", "Renames a file or files.\nUsage:\n  rename [old_name] [new_name]\n  - [old_name]: The current name of the file.\n  - [new_name]: The new name for the file.\n  - Example: rename file1.txt file2.txt" }, 
            // Done
            { "type", "Displays the contents of a text file.\nUsage:\n  type [file]\n  - [file]: The text file to display.\n  - Example: type file.txt" }, 
            // Done
            { "import", "Imports text file(s) from your computer.\nUsage:\n  import [file_path]\n  - [file_path]: The path of the file(s) to import.\n  - Example: import C:\\Documents\\file.txt" }, 
            // 
            { "export", "Exports text file(s) to your computer.\nUsage:\n  export [file_path]\n  - [file_path]: The path where the file(s) will be exported.\n  - Example: export C:\\Backup\\file.txt" },
            //
        };
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

        public static void Make_Directory(List<List<string>> paths)
        {
            foreach (var pathParts in paths)
            {
                try
                {
                    string name = pathParts.Last();
                    var parentPath = pathParts;
                    parentPath.RemoveAt(parentPath.Count - 1);
                    var parentDir = getDirectory(parentPath);
                    int index = parentDir.Search(name);
                    var path = $"{string.Join("\\", pathParts)}\\{name}";

                    if (index != -1)
                    {
                        throw new Exception($"The directory '{path}' is already exists.");
                    }
                    Directory_Entry newDir = new Directory_Entry(name, 1, 0, 0);
                    parentDir.directoryTable.Add(newDir);
                    parentDir.Write_Directory();

                    if (parentDir.parent != null)
                    {
                        parentDir.parent.Update_Content(parentDir.Get_Directory_Entry());
                    }
                    Console.WriteLine($"Directory '{path}' created successfully.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }



        public static void Remove_Directory(List<List<string>> paths)
        {
            foreach (var pathParts in paths)
            {
                try
                {
                    var name = pathParts.Last();
                    var parentPath = pathParts;
                    parentPath.RemoveAt(pathParts.Count - 1);
                    var parentDir = getDirectory(parentPath);
                    var index = parentDir.Search(name);
                    var path = $"{string.Join("\\", pathParts)}\\{name}";
                    if (index == -1)
                    {
                        throw new Exception($"This Directory '{path}' is not found");
                    }
                    Directory_Entry entry = parentDir.Get_Directory_Entry();
                    if (entry.attribute == 1)
                    {
                        pathParts.Add(name);
                        Directory newDir = getDirectory(pathParts);

                        if (newDir.size > 0)
                        {
                            throw new Exception($"Cannot remove this directory '{path}' is not empty");
                        }
                        var answer = "";
                        do
                        {
                            Console.Write($"Are you sure to delete '{path}' [Y/N]?");
                            answer = Console.ReadLine();
                        } while (answer.ToLower() != "y" && answer.ToLower() != "n");
                        if (answer.ToLower() == "n") continue;
                        newDir.Delete_Directory(name);
                        parentDir.Write_Directory();
                        Console.WriteLine($"Directory '{path}' deleted successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Error: The specified name is not a directory.");
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
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


        public static void Rename(List<string> filePath, string newName)
        {
            try
            {
                Directory_Entry.CleanName(newName);
                var oldName = filePath.Last();

                var parentPath = filePath;
                parentPath.RemoveAt(filePath.Count - 1);
                var parent = getDirectory(parentPath);
                int index_oldName = parent.Search(oldName);
                int index_newName = parent.Search(newName);
                if (index_oldName != -1)
                {
                    if (parent.directoryTable[index_oldName].attribute == 1)
                    {
                        throw new Exception("Cannot rename a directory");
                    }
                    if (index_newName == -1)
                    {
                        Directory_Entry entry = parent.directoryTable[index_oldName];

                        entry.name = newName.ToCharArray();
                        parent.Write_Directory();
                        Console.WriteLine($"Directory '{oldName}' renamed to '{newName}' successfully.");

                    }
                    else
                    {
                        Console.WriteLine($"'{newName}' is duplicated file name");
                    }
                }
                else
                {
                    Console.WriteLine("The specified name is not a file.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void Type(List<List<string>> paths)
        {
            foreach (var pathParts in paths)
            {
                try
                {
                    var name = pathParts.Last();
                    var parentPath = pathParts;
                    parentPath.RemoveAt(pathParts.Count - 1);
                    var parentDir = getDirectory(parentPath);
                    Console.WriteLine("---------");
                    Console.WriteLine(string.Join("\\", pathParts) + "\\" + name);
                    Console.WriteLine("=========");
                    int index = parentDir.Search(name);
                    if (index != -1)
                    {
                        int fc = parentDir.directoryTable[index].first_cluster;
                        int sz = parentDir.directoryTable[index].size;
                        File_Entry f = new File_Entry(name, 0, sz, fc, "", parentDir);
                        f.Read_File();

                        Console.WriteLine(f.content);
                    }
                    else
                    {
                        Console.WriteLine($"Failed: There is no a file named '{name}'");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }

        public static void Delete_File(List<List<string>> paths)
        {
            foreach (var pathParts in paths)
            {
                try
                {
                    var name = pathParts.Last();
                    var parentPath = pathParts;
                    parentPath.RemoveAt(pathParts.Count - 1);
                    var parentDir = getDirectory(parentPath);
                    var index = parentDir.Search(name);
                    var path = $"{string.Join("\\", pathParts)}\\{name}";
                    if (index == -1)
                    {
                        throw new Exception($"This Directory '{path}' is not found");
                    }
                    Directory_Entry entry = parentDir.Get_Directory_Entry();

                    pathParts.Add(name);

                    var answer = "";
                    do
                    {
                        Console.Write($"Are you sure to delete '{path}' [Y/N]?");
                        answer = Console.ReadLine();
                    } while (answer.ToLower() != "y" && answer.ToLower() != "n");
                    if (answer.ToLower() == "n") continue;
                    bool isDir = parentDir.directoryTable[index].attribute == 1;
                    if (isDir)
                    {
                        Directory newDir = getDirectory(pathParts);
                        newDir.Delete_Directory(name);
                    }
                    else
                    {
                        parentDir.directoryTable.RemoveAt(index);
                    }
                    parentDir.Write_Directory();
                    Console.WriteLine($"{(isDir? "Directory": "File")} '{path}' deleted successfully.");

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }



        public static void Change_Directory(List<string> pathParts)
        {
            try
            {
                if (pathParts.Count == 0)
                {
                    Console.WriteLine("=> " + Program.currentDirectory.GetCurrentPath());
                }
                else if (!(pathParts.Count == 1 && pathParts[0] == "."))
                {
                    Program.currentDirectory = getDirectory(pathParts);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
            var isAbsPath = path[0] == "O:";
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
                        return current;
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
                    throw new Exception($"Invalid Path");
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