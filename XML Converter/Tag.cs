using System;
using System.Collections.Generic;
using System.Text;

namespace XML_Converter
{
    public class Tag
    {
        public string Id { get; set; }
        public string Definition { get; set; }
        public string Type { get; set; }

        public Tag(string id, string definition, string type)
        {
            Id = id;
            Definition = definition;
            Type = type;
        }
    }
}
