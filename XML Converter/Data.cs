using System;
using System.Collections.Generic;
using System.Text;

namespace XML_Converter
{
    public class Data
    {
        public string Id { get; set; }
        public string Value { get; set; }

        public Data(string id, string value)
        {
            Id = id;
            Value = value;
        }
    }
}
