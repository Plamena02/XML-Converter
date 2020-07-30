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
        public Tag[] File1 { get; set; }
        public Tag[] File2 { get; set; }
        public Tag[] File3 { get; set; }
        public Tag[] File4 { get; set; }
        public Tag[] File5 { get; set; }
        public Tag[] File6 { get; set; }
        public Tag[] File7 { get; set; }
        public Tag[] File8 { get; set; }
        public Tag[] File9 { get; set; }

        public TagStore(string path)
        {
            FileNames = new List<string>();
            FileTags = new List<Tag[]> {File1, File2, File3, File4, File5, File6, File7, File8, File9};
        }

        public bool LoadFile(string path)
        {
            var ret = true;

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


        public int CheckName(string name)
        {  
            var ret = -1;
            for (int i = 0; i < FileNames.Count; i++)
            {
                if (FileNames[i] == name)
                {
                    ret = i;
                }
            }
            return ret;
        } 

        public Tag CheckTag(Tag tag, int index)
        {

            foreach (var t in FileTags[index])
            {
                if (t == tag)
                {
                    return tag;
                }
            }
            // if the tag was found what is expected to return ?
        }

        public void SortTags(string path)
        {
            string[] lines = System.IO.File.ReadAllLines(@path);
            var i = 0;
            var a = 0;

            foreach (var line in lines)
            {
                if (line.Contains(".txt"))
                {
                    var name = line.Substring(6,10);
                    FileNames.Add(name);
                    i++;
                    a = 0;
                }
                else
                {
                    var arr = line.Remove(0,12).Split(';');
                    var tag = new Tag(arr[0], arr[1], arr[2]); 
                    FileTags[i][a] = tag;
                    a++;
                }
            }
        }
    }
}
