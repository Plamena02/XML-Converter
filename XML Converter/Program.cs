using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;

namespace XML_Converter
{
    class Program
    {

        public static int ExitNumber = 0;

        static int Main(string[] args)
        {
            // get parameters from command line (IVB: only as a test, then replace with string[] args)
            //var arr = new string[] { Console.ReadLine() };
            var parameters = GetParameters(args);

            // the file with data - IVB: check if parameter is available
            string path1 = @parameters[0];

            // check if everything is all right with the file
            ServiceabilityCheck(path1);

            // the file to be processed - IVB: check if parameter is available
            string path2 = @parameters[1];

            // the place for extraction
            string path3 = "";

            if (parameters.Count == 3)
            {
                path3 = @parameters[2];
            }
            else
            {
                path3 = Directory.GetCurrentDirectory();    
            }

            var disk = $"{path3[0]}{path3[1]}";
            CheckForFreeSpace(@path2, disk);
            // check if everything is all right with the file
            ZipFile.ExtractToDirectory(@path2, $@"{path3}\Files");
            string[] dirs = Directory.GetFiles($@"{path3}\Files\");
            foreach (string dir in dirs)
            {
                ServiceabilityCheck(dir);
            }

            return ExitNumber; // tool must return a value
        }

        private static void CheckForFreeSpace(string filePath, string driveName)
        {
            long fileLength = new System.IO.FileInfo(filePath).Length;

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                try
                {
                    if (drive.IsReady && drive.Name == driveName)
                    {
                        var freeSpace = drive.TotalFreeSpace;
                        if (freeSpace < fileLength)
                        {
                            throw new Exception("There is not enough disk space.");
                        }
                    }
                    else
                    {
                        throw new Exception("The disk was not found");
                    }
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }
        }

        private static List<string> GetParameters(string[] args)
        {
            if (args.Length < 2 || args[0] == "/?" || args[0] == "-?") 
            {
                HelpCommand();
            }

            var separators = new string[] { "/config", "/input", "/output", "/c", "/i", "/o", "\"", " " };
            var a = string.Join("", args);
            var list = a.Split( separators, StringSplitOptions.RemoveEmptyEntries).ToList();

            try
            {   
                if (list.Count < 2)
                {
                    ExitNumber = 1;
                    throw new Exception("Parameters are missing. Please enter at least two parameters.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
             
            return list;
        }

        // IVB: could be placed in parameter controller class
        private static void HelpCommand()
        {
            string[] info =
            {
                "Syntax: XML_Converter /parameter value...",
                "\t/config, /c\tconfiguration file, mandatory",
                "\t/input, /i\tinput archive file, mandatory",
                "\t/output, /o\toutput folder, defaults to current folder"
            };
            foreach(string ln in info)
            {
                Console.WriteLine(ln);
            }
        }

        private static void ServiceabilityCheck(string path)
        {
            try
            {
                if (!(File.Exists(@path)))
                {
                    ExitNumber = 1;
                    throw new Exception($"The file with directory {path} does not exist.");
                }

                if (new FileInfo(@path).Length == 0)
                {
                    ExitNumber = 2;
                    throw new Exception($"The file with directory {path} is empty.");
                }

                string extension = Path.GetExtension(@path);
            
                if (extension != ".txt")
                {
                    ExitNumber = 2;
                    throw new Exception($"The file with directory {path} is not in the correct format.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }             
        }
    }
}
