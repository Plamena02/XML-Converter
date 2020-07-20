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

        static void Main(string[] args)
        {
            // get parameters from command line
            var parameters = GetParameters(args);

            // the file with data
            string path1 = @parameters[0];

            // check if everything is all right with the file
            ServiceabilityCheck(path1);

            // the file to be processed
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

            // check if everything is all right with the file
            ZipFile.ExtractToDirectory(@path2, $@"{path3}\Files");
            string[] dirs = Directory.GetFiles($@"{path3}\Files\");
            foreach (string dir in dirs)
            {
                ServiceabilityCheck(dir);
            }
        }

        private static List<string> GetParameters(string[] args)
        {
            if (args[0] == "dir/?" || args[0] == "dir-?")
            {
                HelpCommand();
            }

            try
            {   
                if (args.Length < 4)
                {
                    ExitNumber = 1;
                    throw new Exception("Parameters are missing. Please enter at least two parameters.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            var list = new List<string> { };

            for (int i = 0; i < args.Length; i++)
            {
                if (i%2!=0)
                {
                    list.Add(args[i]);
                }
            }

            return list;
        }

        private static void HelpCommand()
        {
            throw new NotImplementedException();
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
