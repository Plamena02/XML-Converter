using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XML_Converter
{
    public class TagStore
    {
        public List<string> FileNames;
        public List<Tag[]> FileTags;
        public string FilePath;

        public TagStore(string path)
        {
            FileNames = new List<string>();
            FileTags = new List<Tag[]> ();
            FilePath = path;
        }

        public bool LoadFile()
        {
            var ret = true;

            if (new FileInfo(@FilePath).Length == 0)
            {
                ret = false;
            }

            string extension = Path.GetExtension(@FilePath);

            if (extension != ".txt")
            {
                ret = false;
            }
            return ret;
        }

        public void ReadFile()
        {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(@FilePath);
            var TagList = new List<Tag>();
            while ((line = file.ReadLine()) != null)
            {
                var arr = line.Split('=');
                if (arr[0].Contains("-"))
                {
                    var value = arr[1].Split(';');
                    TagList.Add(new Tag(value[0], value[1], value[2]));
                }
                else
                {
                    FileNames.Add(arr[1]);
                    if (TagList.Count != 0)
                    {
                        int length = 0;
                        foreach (var tag in TagList)
                        {
                            var id = Int32.Parse(tag.Id);
                            if (id > length)
                            {
                                length = id;
                            }
                        }

                        var TagArray = new Tag[length + 1];
                        for (int i = 0; i < TagList.Count; i++)
                        {
                            var id = Int32.Parse(TagList[i].Id);
                            TagArray[id] = TagList[i];
                        }
                        FileTags.Add(TagArray);
                        TagList = new List<Tag>();
                    }
                }
            }

            file.Close();
        }

        public int CheckName(string name)
        {
            int index;
            try
            {
               index = FileNames.IndexOf(name);
            }
            catch (Exception)
            {
                return -1;
            }
            return index;
        } 

        public Tag CheckTag(int index)
        {
            foreach (var t in FileTags)
            {
                if (t[index] != null)
                {
                    return t[index];
                }
            }
            return null;
        }
    }
}
