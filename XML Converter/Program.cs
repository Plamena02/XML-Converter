using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http.Headers;

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
            if (!(File.Exists(@config)) || ServiceAbilityCheck(config) == false)
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

            return (int)ExitCode.EXIT_OK;
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
