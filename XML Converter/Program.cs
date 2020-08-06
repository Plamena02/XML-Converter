﻿using System;
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
            //string[] ar = Console.ReadLine().Split();

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

            workdir = Path.GetFullPath(workdir); // IVB: fixup for relative paths
            var disk = $"{workdir[0]}{workdir[1]}{workdir[2]}";

            if (CheckForFreeSpace(@archive, disk) == false)
            {
                // IVB: TODO - message
                return (int)ExitCode.EXIT_ERROR;
            }

            // IVB: TODO - start info: start time, definition file, input archive, output folder

            // IVB: TODO - skip use of Files subfolder
            ZipFile.ExtractToDirectory(@archive, $@"{workdir}\Files");
            string[] dirs = Directory.GetFiles($@"{workdir}\Files\");
            foreach (string dir in dirs)
            {
                // IVB: TODO - check if the unzipped file is in correct format as stated in documentation - start / end lines
                if (ServiceAbilityCheck(dir) == false)
                {
                    // IVB: TODO - message
                    return (int)ExitCode.EXIT_INVALID_FILES;
                }
                else
                {
                    var DataFileName = dir.Split("Files").Last().Substring(1); // IVB: use Path.GetFileName instead
                    var fileNumber = tagStore.CheckFileName(DataFileName);
                    if (fileNumber == -1)
                    {
                        // IVB: TODO - message for unknown file, return code EXIT_WARNINGS
                        Console.WriteLine($"This file {DataFileName} was not found."); continue;
                    }

                    string line;
                    System.IO.StreamReader file = new System.IO.StreamReader(@dir);

                    // create Xml File
                    var name = DataFileName.Replace(".txt", "");
                    XmlWriter xmlWriter = XmlWriter.Create(name + ".xml");
                    xmlWriter.WriteStartDocument();

                    xmlWriter.WriteStartElement("table");
                    xmlWriter.WriteAttributeString("name", name);
                    
                    var a = 1;
                    while ((line = file.ReadLine()) != null)
                    {
                        var arr = line.Split('|');
                       

                        if (arr.Length > 1)
                        {
                            xmlWriter.WriteStartElement("record");
                            xmlWriter.WriteAttributeString("id", a.ToString());

                            for (int i = 0; i < arr.Length; i++)
                            {
                                var element = arr[i].Split('#');
                                if (element.Length == 2)
                                {
                                    var id = element[0];
                                    var value = element[1];
                                    var tag = tagStore.CheckTag(fileNumber, Int32.Parse(id));

                                    // IVB: TODO - message for unknown tag (or wrong value length) - return code EXIT_WARNINGS
                                    if (tag != null)
                                    {
                                        if (tag.Length >= value.Length) // IVB: warning only, do process tag
                                        {
                                            xmlWriter.WriteStartElement(tag.Definition);
                                            xmlWriter.WriteString(value);
                                            xmlWriter.WriteEndElement();
                                        }
                                    }
                                }
                            }                            
                            xmlWriter.WriteEndElement();
                            a++;
                        }
                    }

                    xmlWriter.WriteEndElement(); 
                    xmlWriter.WriteEndDocument(); 
                 
                    xmlWriter.Close();                   
                    file.Close();

                    // IVB: TODO - stats - lines/records processed in NNN seconds, input filesize, output filesize
                }
            }

            // IVB: TODO - stats - end time, N files processed in K min L seconds
            return (int)ExitCode.EXIT_OK; // IVB: ... or EXIT_WARNINGS
        }

        private static bool CheckForFreeSpace(string zipFile, string driveName)
        {
            long fileLengths = 0;
            using (ZipArchive zip = ZipFile.Open(zipFile, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    fileLengths += entry.Length;
                }            
            }

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    var freeSpace = drive.TotalFreeSpace;
                    if (freeSpace >= fileLengths)
                    {
                        return true;
                    }
                }                  
            }
            return false;  
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
