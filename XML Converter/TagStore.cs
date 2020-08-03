using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

            if (!File.Exists(@FilePath) || new FileInfo(@FilePath).Length == 0) // IVB: check if the file exists before size check
            {
                ret = false;
            }

            string extension = Path.GetExtension(@FilePath);

            if (extension != ".txt")
            {
                ret = false;
            }

            if (ret)
            {
                ReadFile();
            }
            return ret;
        }

        public int CheckFileName(string name)
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

        public Tag CheckTag(int fileIndex, int tagId)
        {
            try
            {
                return FileTags[fileIndex][tagId];
            }
            catch(Exception) { }
            return null;
        }

        private void ReadFile()
        {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(@FilePath);
            var TagList = new List<Tag>();
            while ((line = file.ReadLine()) != null)
            {
                var arr = line.Split('=');
                if (arr.Length > 1) // IVB: skip empty lines
                {
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
                            FileTags.Add(SparseArrayFromList(TagList));
                            TagList = new List<Tag>();
                        }
                    }
                }
            }

            if (TagList.Count != 0)
            {
                FileTags.Add(SparseArrayFromList(TagList));
            }

            file.Close();
        }

        private Tag[] SparseArrayFromList(List<Tag> TagList)
        {
            int length = 0;
            foreach (var tag in TagList)
            {
                length = Math.Max(length, Int32.Parse(tag.Id));
            }

            Tag[] TagArray = new Tag[length + 1];
            for (int i = 0; i < TagList.Count; i++)
            {
                var id = Int32.Parse(TagList[i].Id);
                TagArray[id] = TagList[i];
            }

            return TagArray;
        }

    }
}
