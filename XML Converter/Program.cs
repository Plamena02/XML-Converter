using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Dynamic;
using System.Xml.Linq;

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

            if (!(File.Exists(@archive))||!(File.Exists(@workdir)))
            {
                return (int)ExitCode.EXIT_INVALID_FILES;
            }

            var disk = $"{workdir[0]}{workdir[1]}";
            if (CheckForFreeSpace(@archive, disk) == false)
            {
                return (int)ExitCode.EXIT_INVALID_FILES;
            }

            ZipFile.ExtractToDirectory(@archive, $@"{workdir}\Files");
            string[] dirs = Directory.GetFiles($@"{workdir}\Files\");
            foreach (string dir in dirs)
            {
                if (ServiceAbilityCheck(dir) == false)
                {
                    return (int)ExitCode.EXIT_ERROR;
                }
            }
            
            var dataStores = DataProcessing(dirs, tagStore);
            CreateXMLFiles(dataStores, workdir);

            return (int)ExitCode.EXIT_OK;
        }

        private static void CreateXMLFiles(List<DataStore> dataStores, string workdir)
        {
            foreach (var dataStore in dataStores)
            {
                XDocument XMLFile = new XDocument();
                XMLFile.Add(new XComment($"<table name=\"{dataStore.FileName.Replace(".txt","")}\""));
                var a = 1;
                foreach (var dataList in dataStore.FileData)
                {
                    XMLFile.Add(new XComment($"<record id=\"{a}\">"));
                    foreach (var data in dataList)
                    {
                        if (data.Definition != "")
                        {
                            var definition = data.Definition;
                            var value = data.Value;
                            XMLFile.Add(new XComment($"<{definition}>{value}</{definition}>"));
                        }  
                    }

                    a++;
                    XMLFile.Add(new XComment("</record>"));
                }

                XMLFile.Add(new XComment("</table>"));
                XMLFile.Save(Path.ChangeExtension($@"{workdir}\XML Files", ".xml"));
            }
           
        }

        private static List<DataStore> DataProcessing(string[] dirs, TagStore tagStore)
        {
            var List = new List<DataStore>();
            foreach (string dir in dirs)
            {
                DataStore dataStore = new DataStore(dir);
                var fileNumber = tagStore.CheckFileName(dataStore.FileName);
                if (fileNumber == -1)
                {
                    Console.WriteLine($"This file {dataStore.FileName} was not found.");
                }
                else
                {
                    foreach (var dataList in dataStore.FileData)
                    {
                        foreach (var item in dataList)
                        {
                            var tag = tagStore.CheckTag(fileNumber, Int32.Parse(item.Id));
                            if (tag != null && tag.Length >= item.Value.Length)
                            {
                                item.Definition = tag.Definition;
                            }
                        }
                    }
                }

                List.Add(dataStore);  
            }
            
            return List;
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

            if (new FileInfo(@path).Length == 0)
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
