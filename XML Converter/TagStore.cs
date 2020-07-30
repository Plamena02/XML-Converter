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
            FileTags = new List<Tag[]>();
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

            for (int i = 0; i < FileTags.Count; i++)
            {
                if (i == index)
                {
                    var arr = FileTags[i];
                }
            }
            // to do 
            return tag;
        }
    }
}
