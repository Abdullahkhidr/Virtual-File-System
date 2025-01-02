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

        public static void Main(string[] args)
        {
            Virtual_Disk.Initialize();

            while (true)
            {

                Console.Write(path + "\\> ");
                string input = Console.ReadLine().ToLower().Trim();
                string[] commandParts = input.Split(' ');
                var pathParts = commandParts.Length < 2 ? new List<string>() : commandParts[1].Split('\\').ToList();
                string command = commandParts.Length > 0 ? commandParts[0] : "";


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


                    case "md":
                        if (commandParts.Length == 2)
                        {
                            string directoryName = commandParts[1];
                            Command.Make_Directory(directoryName);
                        }
                        else
                        {
                            Console.WriteLine("Usage: md <directory_name>");
                        }
                        break;


                    case "rd":
                        if (commandParts.Length == 2)
                        {
                            string directoryName = commandParts[1];
                            Command.Remove_Directory(directoryName);
                        }
                        else
                        {
                            Console.WriteLine("Usage: rd <directory_name>");
                        }
                        break;


                    case "dir":
                        Command.Display_Directory(pathParts);
                        break;


                    case "rename":
                        if (commandParts.Length == 3)
                        {
                            Command.Rename(commandParts[1], commandParts[2]);
                        }
                        else
                        {
                            Console.WriteLine("Usage: rename <oldName> <newName>");
                        }
                        break;


                    case "type":
                        if (commandParts.Length == 2)
                        {
                            Command.Type(commandParts[1]);
                        }
                        else
                        {
                            Console.WriteLine("Usage: type <fileName>");
                        }
                        break;


                    case "cd":
                        if (commandParts.Length == 1)
                        {
                            Console.WriteLine(currentDirectory + "\n");
                        }
                        else if (commandParts.Length == 2)
                        {
                            Command.Change_Directory(commandParts[1]);
                        }
                        break;



                    case "copy":
                        if (commandParts.Length == 3)
                        {
                            string src = commandParts[1];
                            string dest = commandParts[2];

                            Command.Copy(src, dest);
                        }
                        else
                        {
                            Console.WriteLine("Usage: copy <source_file> <destination_file>");
                        }
                        break;

                    case "del":
                        if (commandParts.Length == 2)
                        {
                            string fileName = commandParts[1];
                            Command.Delete_File(fileName);
                        }
                        else
                        {
                            Console.WriteLine("Usage: del <file_name>");
                        }
                        break;


                    case "import":
                        if (commandParts.Length == 2)
                        {
                            Command.Import(commandParts[1]);
                        }
                        else
                        {
                            Console.WriteLine("Usage: type <path>");
                        }
                        break;


                    case "export":
                        if (commandParts.Length == 3)
                        {
                            Command.Export(commandParts[1], commandParts[2]);
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
