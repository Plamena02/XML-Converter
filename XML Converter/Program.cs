using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Dynamic;
using System.Xml.Linq;
using System.Xml;

namespace XML_Converter
{
    class Program
    {
        public enum ExitCode {
            EXIT_OK = 0,
            EXIT_NO_PARAMETERS,
            EXIT_INVALID_FILES,
            EXIT_ERROR
        };

        static int Main(string[] args)
        {
            //string[] a = Console.ReadLine().Split();

            Controller paramControl = new Controller(args);
            if (paramControl.NeedHelp())
            {
                paramControl.ShowHelp();
                return (int)ExitCode.EXIT_NO_PARAMETERS;
            }

            if (!paramControl.LoadParameters())
            {
                return (int)ExitCode.EXIT_NO_PARAMETERS;
            }

            string config = paramControl.GetParam(ParamNames.config);
            TagStore tagStore = new TagStore(config);
            if (tagStore.LoadFile() == false)           
            {
                return (int)ExitCode.EXIT_INVALID_FILES;
            }

            string archive = paramControl.GetParam(ParamNames.input);
            string workdir = paramControl.GetParam(ParamNames.output);

            if (!(File.Exists(@archive))||!(Directory.Exists(@workdir))) 
            {
                return (int)ExitCode.EXIT_INVALID_FILES;
            }

            /*
            // for "/output ..\..\..\..\data" disk is ".." - wrong, use Path.GetFullPath
            var disk = $"{workdir[0]}{workdir[1]}";
            if (CheckForFreeSpace(@archive, disk) == false)
            {
                return (int)ExitCode.EXIT_INVALID_FILES;
            }
            */
            ZipFile.ExtractToDirectory(@archive, $@"{workdir}\Files");
            string[] dirs = Directory.GetFiles($@"{workdir}\Files");
            foreach (string dir in dirs)
            {
                if (ServiceAbilityCheck(dir) == false)
                {
                    return (int)ExitCode.EXIT_INVALID_FILES;
                }
                else
                {
                    var DataFileName = dir.Split('\t').Last();
                    var fileNumber = tagStore.CheckFileName(DataFileName);
                    if (fileNumber == -1)
                    {
                        Console.WriteLine($"This file {DataFileName} was not found."); continue;
                    }
                    
                    ConvertFileToXML(dir, fileNumber, tagStore); 
                }
            }

            return (int)ExitCode.EXIT_OK;
        }

        private static void ConvertFileToXML(string dir, int fileNumber, TagStore tagStore)
        {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(@dir);
            // create Xml File
            //XDocument XMLFile = new XmlDocument();
            //XMLFile.Add(new XElement($"<table name=\"{dataStore.FileName.Replace(".txt","")}\""));

            while ((line = file.ReadLine()) != null)
            {
                var arr = line.Split('|');
                var a = 1;

                if (arr.Length > 1)
                {
                    //XMLFile.Add(new XElement($"<record id=\"{a}\">"));
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var element = arr[i].Split('#');
                        if (element.Length == 2)
                        {
                            var id = element[0];
                            var value = element[1];
                            var tag = tagStore.CheckTag(fileNumber, Int32.Parse(id));

                            if (tag != null)
                            {                               
                                if (tag.Length >= value.Length)
                                {
                                    // add XElement to Xml File 
                                    //XMLFile.Add(new XElement($"<{definition}>{value}</{definition}>"));
                                }
                            }                            
                        }                        
                    }  
                    a++;
                    //XMLFile.Add(new XElement("</record>"));
                }
            }

            //XMLFile.Add(new XElement("</table>"));

            file.Close();
        }

        private static bool CheckForFreeSpace(string filePath, string driveName)
        {
            bool ret = true;
            long fileLength = new System.IO.FileInfo(filePath).Length;

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    var freeSpace = drive.TotalFreeSpace;
                    if (freeSpace < fileLength)
                    {
                        ret = false;
                    }
                }
                else
                {
                    ret = false;
                }                    
            }
            return ret;  
        }

        private static bool ServiceAbilityCheck(string path)
        {
            bool ret = true;

            if (!File.Exists(@path) || new FileInfo(@path).Length == 0)
            {
                ret = false;
            }

            string extension = Path.GetExtension(@path);

            if (extension != ".txt")
            {
                ret = false;
            }

            return ret;
        }
    }
}
