using System;
using System.IO;
using System.IO.Compression;

namespace XML_Converter
{
    class Program
    {
        static void Main(string[] args)
        {
            // the file with data
            var path1 = Console.ReadLine();

            // check if everything is all right with the file
            ServiceabilityCheck(path1);

            // the file to be processed
            var path2 = Console.ReadLine();

            // check if everything is all right with the file
            
            ZipFile.ExtractToDirectory(@path2, @"C:\Users\User\Desktop\Files");
            string[] dirs = Directory.GetFiles(@"C:\Users\User\Desktop\Files\");
            foreach (string dir in dirs)
            {
                ServiceabilityCheck(dir);
            }
        }

        private static void ServiceabilityCheck(string path)
        {
            if (!(File.Exists(@path)))
            {
                throw new Exception($"The file with directory {path} does not exist.");
            }

            if (new FileInfo(@path).Length == 0)
            {
                throw new Exception($"The file with directory {path} is empty.");
            }

            string extension = Path.GetExtension(@path);
            
            if (extension != ".txt")
            {
                throw new Exception($"The file with directory {path} is not in the correct format.");
            }

        }
    }
}
