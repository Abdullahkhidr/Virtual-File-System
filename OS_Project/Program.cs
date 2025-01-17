using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace OS_Project
{
    internal class Program
    {
        public static Directory currentDirectory = new Directory();
        public static Directory Root = new Directory("O:", 1, 0, 5, null);
        public static string path = "";
        internal static object current;

        static List<string> splitPath(string path)
        {
            return path.Split('\\').ToList();
        }
        static List<List<string>> extractPaths(string paths)
        {
            var splittedPaths = paths.Split(' ').ToList();
            var result = new List<List<string>>();
            foreach (var path in splittedPaths)
            {
                if (path.Trim().Count() == 0) continue;
                result.Add(splitPath(path));
            }
            return result;
        }

        public static void Main(string[] args)
        {
            Virtual_Disk.Initialize();

            while (true)
            {
                Console.Write(currentDirectory.GetCurrentPath() + "\\> ");
                string input = Console.ReadLine().Trim();
                string[] commandParts = input.Split(' ');
                string command = commandParts.Length > 0 ? commandParts[0] : "";

                var paths = extractPaths(commandParts.Length > 1 ? string.Join(" ", commandParts.ToList().GetRange(1, commandParts.Length - 1)) : "");

                switch (command)
                {
                    case "help":
                        if (commandParts.Length > 1)
                        {
                            Command.DisplayCommandHelp(commandParts[1]);
                        }
                        else
                        {
                            Command.DisplayAllCommandsHelp();
                        }
                        break;


                    case "cls":
                        if (commandParts.Length > 1)
                        {
                            Console.WriteLine("Invalid syntax");
                        }
                        else
                        {
                            Command.Cls();
                        }
                        break;


                    case "exit":
                    case "quit":
                        if (commandParts.Length > 1)
                        {
                            Console.WriteLine("Invalid syntax");
                        }
                        else
                        {
                            Command.Exit();
                        }
                        break;

                    case "mkdir":
                    case "md":
                        if (paths.Count > 0)
                        {
                            Command.Make_Directory(paths);
                        }
                        else
                        {
                            Console.WriteLine("Usage: md <directory_name>");
                        }
                        break;

                    case "rm":
                    case "rd":
                        if (paths.Count > 0)
                        {
                            Command.Remove_Directory(paths);
                        }
                        else
                        {
                            Console.WriteLine("Usage: rd <directory_name>");
                        }
                        break;

                    case "ls":
                    case "dir":
                        Command.Display_Directory(paths.Count == 0 ? new List<string>() : paths.First());
                        break;


                    case "rename":
                        if (commandParts.Length == 3)
                        {
                            Command.Rename(splitPath(commandParts[1]), commandParts[2]);
                        }
                        else
                        {
                            Console.WriteLine("Usage: rename <oldName> <newName>");
                        }
                        break;


                    case "type":
                        if (paths.Count > 0)
                        {
                            Command.Type(paths);
                        }
                        else
                        {
                            Console.WriteLine("Usage: type <fileName>");
                        }
                        break;


                    case "cd":
                        Command.Change_Directory(paths.Count == 0 ? new List<string>() : paths.First());
                        break;



                    case "copy":
                        if (commandParts.Length >= 2)
                        {
                            Command.Copy(paths[0], paths.Count > 1 ? paths[1] : new List<string>());
                        }
                        else
                        {
                            Console.WriteLine("Usage: copy <source_file> [destination_file]");
                        }
                        break;

                    case "del":
                        if (commandParts.Length >= 2)
                        {
                            Command.Delete_File(paths);
                        }
                        else
                        {
                            Console.WriteLine("Usage: del <file_name>");
                        }
                        break;


                    case "import":
                        if (commandParts.Length >= 2)
                        {
                            Command.Import(commandParts[1], commandParts.Length > 2 ? splitPath(commandParts[2]) : new List<string>());
                        }
                        else
                        {
                            Console.WriteLine("Usage: type <path> [destination]");
                        }
                        break;


                    case "export":
                        if (commandParts.Length > 1)
                        {
                            Command.Export(paths[0], ((commandParts.Length > 2) ? commandParts[2] : ""));
                        }
                        else
                        {
                            Console.WriteLine("Usage: export <source> <destination>");
                        }
                        break;


                    default:

                        Console.WriteLine("Invalid command.");
                        break;
                }
            }
        }

    }
}
