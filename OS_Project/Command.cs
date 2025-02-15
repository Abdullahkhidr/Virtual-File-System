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
            { "cd", "Displays the name of or changes the current directory.\nUsage:\n  cd [directory]\n  - If no directory is specified, it displays the current directory.\n  - Use '..' to move up one directory level." }, 
            // Done
            { "cls", "Clears the console screen.\nUsage:\n  cls\n  - This command clears all text from the console window." }, 
            // Done
            { "dir", "Displays a list of files and subdirectories in a directory.\nUsage:\n  dir [directory]\n  - If no directory is specified, it lists the contents of the current directory.\n  - Use a file path to list the contents of a specific directory." }, 
            // Done
            { "exit", "Quits the CMD.EXE program (command interpreter).\nUsage:\n  exit\n  - This command closes the console window or terminates the current session." },
            // Done
            { "copy", "Copies one or more files to another location.\nUsage:\n  copy [source] [destination]\n  - [source]: The file(s) to copy.\n  - [destination]: The location where the file(s) will be copied.\n  - Example: copy file.txt C:\\Backup" },
            // Done
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
            // Done
            { "export", "Exports text file(s) to your computer.\nUsage:\n  export [file_path]\n  - [file_path]: The path where the file(s) will be exported.\n  - Example: export C:\\Backup\\file.txt" },
            // Done
        };
        public static void DisplayAllCommandsHelp()
        {
            foreach (var command in commands)
            {
                String shortDesc = command.Value.Substring(0, command.Value.IndexOf('.') + 1);
                Console.WriteLine($"{command.Key}\t{shortDesc}\n");
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
                    if (!name.Contains('.'))
                    {
                        Console.WriteLine($"Error: maybe this \"{name}\" is not a file name or access is denied.");
                        return;
                    }
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
                        f.readFile();

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
                    Console.WriteLine($"{(isDir ? "Directory" : "File")} '{path}' deleted successfully.");

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




        public static void Import(string path, List<string> des)
        {
            try
            {
                if (!System.IO.File.Exists(path))
                {
                    Console.WriteLine($"Error: Source file '{path}' does not exist.");
                    return;
                }

                try
                {
                    FileInfo fileInfo = new FileInfo(path);
                    if (fileInfo.Length == 0)
                    {
                        Console.WriteLine("Error: Cannot import empty file.");
                        return;
                    }

                    Directory parentDir = Program.currentDirectory;
                    string fileName = Path.GetFileName(path);

                    if (des.Count > 0)
                    {
                        var parentPath = new List<string>(des);
                        if (des.Last().Contains('.'))
                        {
                            fileName = des.Last();
                            parentPath.RemoveAt(parentPath.Count - 1);
                        }

                        try
                        {
                            parentDir = getDirectory(parentPath);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"Error: Invalid destination path '{string.Join("\\", parentPath)}'");
                            return;
                        }
                    }
                    try
                    {
                        Directory_Entry.CleanName(fileName);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine($"Error: Invalid filename '{fileName}'");
                        return;
                    }

                    string fileContent;
                    try
                    {
                        fileContent = System.IO.File.ReadAllText(path);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading file: {ex.Message}");
                        return;
                    }

                    int size = fileContent.Length;
                    int index = parentDir.Search(fileName);

                    if (size > MiniFat.Get_Free_Space())
                    {
                        Console.WriteLine($"Error: Not enough space to import file. Required: {size} bytes, Available: {MiniFat.Get_Free_Space()} bytes");
                        return;
                    }

                    if (index != -1)
                    {
                        var answer = "";
                        do
                        {
                            Console.Write($"File '{fileName}' already exists. Overwrite? [Y/N]: ");
                            answer = Console.ReadLine()?.ToLower();
                        } while (answer != "y" && answer != "n");

                        if (answer == "n")
                        {
                            Console.WriteLine("Import cancelled.");
                            return;
                        }

                        parentDir.directoryTable.RemoveAt(index);
                    }

                    File_Entry newFile = new File_Entry(fileName, 0, size, 0, fileContent, parentDir);
                    newFile.writeFile();

                    Directory_Entry dirEntry = new Directory_Entry(fileName, 0, size, newFile.first_cluster);
                    parentDir.directoryTable.Add(dirEntry);
                    parentDir.Write_Directory();

                    if (parentDir.parent != null)
                    {
                        parentDir.parent.Update_Content(parentDir.Get_Directory_Entry());
                    }

                    Console.WriteLine($"Successfully imported '{path}' to '{parentDir.GetCurrentPath()}\\{fileName}'");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"IO Error: {ex.Message}");
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Error: Access denied to source file.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during import: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error: {ex.Message}");
            }
        }


        public static void Export(List<string> pathParts, string des)
        {
            try
            {
                var name = pathParts.Last();
                var parentPath = pathParts;
                parentPath.RemoveAt(pathParts.Count - 1);
                var parentDir = getDirectory(parentPath);
                int index = parentDir.Search(name);
                if (des.Contains('.'))
                {
                    name = des.Split('\\').Last();
                    des = des.Substring(0, des.LastIndexOf('\\') + 1);
                }
                if (index != -1)
                {
                    int fc = parentDir.directoryTable[index].first_cluster;
                    int sz = parentDir.directoryTable[index].size;
                    File_Entry f = new File_Entry(name, 0, sz, fc, "", parentDir);
                    f.readFile();
                    using (StreamWriter sw = new StreamWriter(des + name))
                    {
                        sw.WriteLine(f.content);
                    }
                }
                else
                {
                    Console.WriteLine("Error: The specified name is not a file.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        public static void Copy(List<string> src, List<string> dest)
        {
            try
            {
                var name_source = src.Last();
                src.RemoveAt(src.Count - 1);

                var name_dest = name_source; 
                if (dest.Count > 0 && dest.Last().Contains('.'))
                {
                    name_dest = dest.Last(); 
                    dest.RemoveAt(dest.Count - 1);
                }

                var parentSrc = getDirectory(src);
                var parentDest = getDirectory(dest);
                int index_src = parentSrc.Search(name_source);
                if (index_src == -1)
                {
                    Console.WriteLine($"Error: Source file '{name_source}' not found.");
                    return;
                }

                int index_dest = parentDest.Search(name_dest);
                if (index_dest != -1)
                {
                    Console.WriteLine($"Error: Destination file '{name_dest}' already exists.");
                    return;
                }

                var sourceFile = parentSrc.directoryTable[index_src];

                if (sourceFile.size > MiniFat.Get_Free_Space())
                {
                    Console.WriteLine("Error: Not enough free space to copy the file.");
                    return;
                }

                File_Entry sourceFileEntry = new File_Entry(
                    name_source,
                    sourceFile.attribute,
                    sourceFile.size,
                    sourceFile.first_cluster,
                    "",
                    parentSrc
                );
                sourceFileEntry.readFile();

                File_Entry destFileEntry = new File_Entry(
                    name_dest,
                    sourceFile.attribute,
                    sourceFile.size,
                    0, 
                    sourceFileEntry.content,
                    parentDest
                );
                destFileEntry.writeFile();

                Directory_Entry destDirEntry = new Directory_Entry(
                    name_dest,
                    sourceFile.attribute,
                    sourceFile.size,
                    destFileEntry.first_cluster
                );
                parentDest.directoryTable.Add(destDirEntry);
                parentDest.Write_Directory();
                if (parentDest.parent != null)
                {
                    parentDest.parent.Update_Content(parentDest.Get_Directory_Entry());
                }

                Console.WriteLine($"File '{name_source}' successfully copied to '{string.Join("\\", dest)}\\{name_dest}'.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        static private Directory getDirectory(List<string> path)
        {
            Directory current = Program.currentDirectory;
            current.Read_Directory();
            if (path.Count == 0 || (path.Count == 1 && path[0] == "."))
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
        public static void DisplayDirectoryTree(List<string> pathParts, string indent = "")
        {
            try
            {
                var directory = getDirectory(pathParts);

                if (string.IsNullOrEmpty(indent))
                {
                    Console.WriteLine($"\nDirectory tree of {directory.GetCurrentPath()}");
                    Console.WriteLine("=".PadRight(50, '='));
                }

                long totalSize = 0;
                int totalFiles = 0;
                int totalDirs = 0;

                var sortedEntries = directory.directoryTable
                    .OrderBy(entry => entry.attribute == 0) 
                    .ThenBy(entry => new string(entry.name).Trim()) 
                    .ToList();

                foreach (var entry in sortedEntries)
                {
                    string name = new string(entry.name).Trim();
                    bool isDirectory = !entry.name.Contains('.');

                    if (name == "." || name == "..")
                        continue;

                    if (isDirectory)
                        totalDirs++;
                    else
                    {
                        totalFiles++;
                        totalSize += entry.size;
                    }

                    string marker = isDirectory ? "📁" : "📄";
                    string size = isDirectory ? "<DIR>" : $"{entry.size} bytes";
                    Console.WriteLine($"{indent}├─{marker} {name.PadRight(30)} {size}");

                    if (isDirectory)
                    {
                        var newPath = new List<string>(pathParts);
                        newPath.Add(name);
                        DisplayDirectoryTree(newPath, indent + "│ ");
                    }
                }

                if (string.IsNullOrEmpty(indent))
                {
                    Console.WriteLine("\nSummary:");
                    Console.WriteLine("-".PadRight(50, '-'));
                    Console.WriteLine($"Total Files: {totalFiles}");
                    Console.WriteLine($"Total Directories: {totalDirs}");
                    Console.WriteLine($"Total Size: {totalSize} bytes");
                    Console.WriteLine($"Free Space: {MiniFat.Get_Free_Space()} bytes");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error accessing directory: {e.Message}");
            }
        }
    }
    }



