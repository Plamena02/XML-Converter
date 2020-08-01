using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XML_Converter
{
    public class DataStore
    {
        public string FileName;
        public List<List<Data>> FileData;
        public string Path;

        public DataStore(string path)
        {
            FileData = new List<List<Data>>();
            Path = path;
            ReadFile();
        }

        public void ReadFile()
        {
            var name = Path.Split('\t').Last();
            FileName = name;

            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(@Path);
            var DataList = new List<Data>();
            while ((line = file.ReadLine()) != null)
            {
                var arr = line.Split('|');
                if (arr.Length > 1)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        var value = arr[i].Split('#');
                        if (value.Length == 2)
                        {
                            DataList.Add(new Data(value[0], value[1]));
                        }
                    }
                    FileData.Add(DataList);
                }
            }

            file.Close();
        }

    }
}
