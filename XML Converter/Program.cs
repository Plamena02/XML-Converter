using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Dynamic;
using System.Xml.Linq;
using System.Xml;
using System.Globalization;
using System.Diagnostics;

namespace XML_Converter
{
    class Program
    {
        public enum ExitCode {
            EXIT_OK = 0,
            EXIT_NO_PARAMETERS,
            EXIT_INVALID_FILES,
            EXIT_ERROR,
            EXIT_WARNINGS
        };

        public static bool Warnings = false;

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

            if (!(File.Exists(@archive))||!(Directory.Exists(@workdir))) 
            {
                return (int)ExitCode.EXIT_INVALID_FILES;
            }

            workdir = Path.GetFullPath(workdir); 
            var disk = $"{workdir[0]}{workdir[1]}{workdir[2]}";

            if (CheckForFreeSpace(@archive, disk) == false)
            {
                Console.WriteLine($"There is not enough space on the drive ({disk}).");
                return (int)ExitCode.EXIT_ERROR;
            }

            var StartTime = DateTime.Now;
            Console.WriteLine($"Start Time:{ StartTime.ToString("h:mm:ss tt") } Config name:{Path.GetFileName(config)} Input file name:{Path.GetFileName(archive)} Output directory:{workdir}");

            try
            {
                ZipFile.ExtractToDirectory(@archive, $@"{workdir}");                
            }
            catch (Exception) { Console.WriteLine("Тhe archive files already exist in the given directory."); Warnings = true; }
            
            string[] dirs = Directory.GetFiles($@"{workdir}", "*.txt");            
            var files = 0;

            foreach (string dir in dirs)
            {
                var FileName = "";
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                var lines = 1;

                if (ServiceAbilityCheck(dir) == false)
                {
                    Console.WriteLine($"File {dir} does not exist or is not in correct format.");
                    return (int)ExitCode.EXIT_INVALID_FILES;
                }
                else
                {
                    var DataFileName = Path.GetFileName(dir); 
                    var fileNumber = tagStore.CheckFileName(DataFileName);
                    if (fileNumber == -1)
                    {
                        Console.WriteLine($"This file {DataFileName} was not found.");
                        Warnings = true;
                    }

                    string line;
                    System.IO.StreamReader file = new System.IO.StreamReader(@dir);

                    // create Xml File
                    var name = DataFileName.Replace(".txt", "");
                    FileName = name + ".xml";
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = "\t";
                    XmlWriter xmlWriter = XmlWriter.Create($@"{workdir}\{FileName}", settings);
                    xmlWriter.WriteStartDocument();

                    xmlWriter.WriteStartElement("table");
                    xmlWriter.WriteAttributeString("name", name);
                                        
                    while ((line = file.ReadLine()) != null)
                    {
                        /*if (line.Contains("�"))
                        {
                            Console.WriteLine($"Unknown symbol was found on file/line {name}.txt/{a}");
                            Warnings = true;
                        }*/
                        var arr = line.Split('|');
                       
                        if (arr.Length > 1)
                        {
                            xmlWriter.WriteStartElement("record");
                            xmlWriter.WriteAttributeString("id", lines.ToString());

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
                                            xmlWriter.WriteStartElement(tag.Definition);
                                            xmlWriter.WriteString(value);
                                            xmlWriter.WriteEndElement();
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Wrong value length. Expected length up to {tag.Length} characters, but was {value.Length}.");
                                            Warnings = true;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Tag was not found on line{lines} with id {id}.");
                                        Warnings = true;
                                    }
                                }
                            }                            
                            xmlWriter.WriteEndElement();
                            lines++;
                        }
                    }

                    xmlWriter.WriteEndElement(); 
                    xmlWriter.WriteEndDocument(); 
                 
                    xmlWriter.Close();                   
                    file.Close(); files++;                    
                }
                File.Delete(dir);

                long InputLength = new System.IO.FileInfo(@dir).Length;
                long OutputLength = new System.IO.FileInfo(Path.GetFullPath(FileName)).Length;
                stopWatch.Stop();
                var sec = stopWatch.Elapsed;

                Console.WriteLine($"{lines}lines processed in {Math.Round(sec.TotalSeconds,2)} seconds  Input file size: {ConvertBytesToMegabytes(InputLength)}MB  Output file size: {ConvertBytesToMegabytes(OutputLength)}MB");
            }

            var EndTime = DateTime.Now;
            TimeSpan span = (EndTime - StartTime);

            Console.WriteLine($"End time:{EndTime.ToString("h:mm:ss tt")} Files {files} processed in {span.Minutes} min {span.Seconds} seconds");

            if (!Warnings)
            {
                return (int)ExitCode.EXIT_OK; 
            }

            return (int)ExitCode.EXIT_WARNINGS;
        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
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

            string format = "yyyy-MM-dd HH:mm:ss"; 
            var date = File.ReadLines(@path).First();
            var endLine = File.ReadLines(@path).Last();

            try
            {
               DateTime dt = DateTime.ParseExact(date, format, CultureInfo.InvariantCulture);
            }
            catch (Exception) { ret = false; }

            if (endLine.Contains("Datensaetze:") == false)
            {
                ret = false;
            }

            return ret;
        }
    }
}
