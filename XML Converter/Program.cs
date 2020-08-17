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
        public enum ExitCode
        {
            EXIT_OK = 0,
            EXIT_NO_PARAMETERS,
            EXIT_INVALID_FILES,
            EXIT_ERROR,
            EXIT_WARNINGS
        };

        public static bool Warnings = false;
        public static Dictionary<string, int> unknownTag = new Dictionary<string, int>();

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

            if (!(File.Exists(@archive)) || !(Directory.Exists(@workdir)))
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
            Console.WriteLine($"Config name:{config} {Path.GetFileName(config)}\nInput file name:{archive} {Path.GetFileName(archive)}\nOutput directory:{workdir}\nStart Time:{ StartTime.ToString("HH:mm:ss") }");

            Stopwatch stopWatch1 = new Stopwatch();
            stopWatch1.Start();

            ZipFile.ExtractToDirectory(@archive, $@"{workdir}", true);
            string[] dirs = Directory.GetFiles($@"{workdir}", "*.txt");
            var files = 0;

            stopWatch1.Stop();
            var seconds = stopWatch1.Elapsed;
            Console.WriteLine($"The {dirs.Length} files were unzipped in {Math.Round(seconds.TotalSeconds, 2)} seconds");

            foreach (string dir in dirs)
            {
                var FileName = "";
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                var lines = 1;
                unknownTag = new Dictionary<string, int>();

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
                        Console.WriteLine($"WARNING: This file {DataFileName} was not found.");
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
                    var endLine = "";

                    while ((line = file.ReadLine()) != null)
                    {
                        endLine = line;
                        if (lines == 1)
                        {
                            string format = "yyyy-MM-dd HH:mm:ss";

                            try
                            {
                                DateTime dt = DateTime.ParseExact(line, format, CultureInfo.InvariantCulture);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine($"WARNING: First line is not in the correct format.");
                                Warnings = true;
                            }
                        }

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
                                            try
                                            {
                                                xmlWriter.WriteStartElement(tag.Definition);
                                                xmlWriter.WriteString(value);
                                                xmlWriter.WriteEndElement();
                                            }
                                            catch (Exception)
                                            {
                                                Console.WriteLine($"Unknown symbol was found on line {lines}");
                                            }

                                        }
                                        else
                                        {
                                            Console.WriteLine($"WARNING: Wrong value length. Expected length up to {tag.Length} characters, but was {value.Length}.");
                                            Warnings = true;
                                        }
                                    }
                                    else
                                    {
                                        if (!unknownTag.ContainsKey(id))
                                        {
                                            unknownTag.Add(id, 0);
                                        }

                                        unknownTag[id] += 1;
                                        Warnings = true;
                                    }
                                }
                            }
                            xmlWriter.WriteEndElement();
                            
                        }lines++;
                    }

                    if (endLine.Contains("Datensaetze:") == false)
                    {
                        Console.WriteLine($"WARNING: Last line is not in the correct format.");
                        Warnings = true;
                    }

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();

                    xmlWriter.Close();
                    file.Close(); files++;
                }

                long InputLength = new System.IO.FileInfo(@dir).Length;
                long OutputLength = new System.IO.FileInfo($@"{workdir}\{FileName}").Length;
                stopWatch.Stop();
                var sec = stopWatch.Elapsed;

                foreach (var item in unknownTag)
                {
                    Console.WriteLine($"WARNING: Unknown tag {item.Key}, found {item.Value} times.");
                }

                Console.WriteLine($"{lines} lines processed in {Math.Round(sec.TotalSeconds, 2)} seconds  Input file size: {ConvertSize(InputLength)}  Output file size: {ConvertSize(OutputLength)}");
                File.Delete(dir);
            }

            var EndTime = DateTime.Now;
            TimeSpan span = (EndTime - StartTime);

            Console.WriteLine($"{files} files processed in {span.Minutes} min {span.Seconds} seconds\nEnd time:{EndTime.ToString("HH:mm:ss")}");

            if (!Warnings)
            {
                return (int)ExitCode.EXIT_OK;
            }

            return (int)ExitCode.EXIT_WARNINGS;
        }

        static string ConvertSize(long bytes)
        {   
            if (ConvertBytesToMegabytes(bytes) < 0.99)
            {
                return $"{ConvertBytesToKilobytes(bytes)} KB";
            }
            if (ConvertBytesToGigabytes(bytes) < 0.99)
            {
                return $"{ConvertBytesToMegabytes(bytes)} MB";
            }
            
            return $"{ConvertBytesToGigabytes(bytes)} GB";
        }

        static double ConvertBytesToKilobytes(long bytes)
        {
            return Math.Round((bytes / 1024f), 2);
        }
        static double ConvertBytesToMegabytes(long bytes)
        {
            return Math.Round(((bytes / 1024f) / 1024f), 2);
        }

        static double ConvertBytesToGigabytes(long bytes)
        {
            return Math.Round(((bytes / 1024f) / 1024f / 1024f), 2);
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
            if (!File.Exists(@path) || new FileInfo(@path).Length == 0)
            {
                return false;
            }

            return true;
        }
    }
}