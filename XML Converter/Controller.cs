using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace XML_Converter
{
    public class Controller
    {
        private string[] args;
        private List<Parameter> Parameters;

        public Controller(string[] args)
        {
            this.args = args;
            Parameters = new List<Parameter>();
        }

        public bool NeedHelp()
        {
            return args.Length < 2 || args[0] == "/?" || args[0] == "-?";
        }

        public void ShowHelp()
        {
            string[] info =
            {
                "Syntax: XML_Converter /parameter value...",
                "\t/config, /c\tconfiguration file, mandatory",
                "\t/input, /i\tinput archive file, mandatory",
                "\t/output, /o\toutput folder, defaults to current folder"
            };
            foreach (string ln in info)
            {
                Console.WriteLine(ln);
            }
        }

        public bool LoadParameters()
        {
            Parameters.Add(new Parameter(ParamNames.output, Directory.GetCurrentDirectory()));
            for (int i = 0; i < args.Length/2; i++)
            {
                ParamNames type;
                string name = args[2 * i];
                bool failed = name[0] != '/';
                if (!failed)
                {
                    name = name.Substring(1); // trim first '/'
                }
                if (failed || !Enum.TryParse(name, false, out type))
                {
                    ParamShortNames sh;
                    if (failed || !Enum.TryParse(name, false, out sh))
                    {
                        Console.WriteLine($"Invalid parameter passed - {name}");
                        return false;
                    }
                    type = (ParamNames)(int)sh;
                }
                Parameter prm = FindParam(type);
                string val = args[2 * i + 1];
                if (prm == null)
                {
                    Parameters.Add(new Parameter(type, val));
                } else
                {
                    prm.Value = val;
                }
            }

            bool ret = true;
            foreach(ParamNames m in new ParamNames[] { ParamNames.config, ParamNames.input })
            {
                if (FindParam(m) == null)
                {
                    Console.WriteLine($"Missing mandatory parameter - {m.ToString()}");
                    ret = false;
                }
            }
            return ret;
        }

        public string GetParam(ParamNames type)
        {
            Parameter prm = FindParam(type);
            return prm != null ? prm.Value : null;
        }

        private Parameter FindParam(ParamNames name)
        {
            return Parameters.Find(n => n.Name.Equals(name.ToString()));
        }
    }
}
