using System;
using System.Collections.Generic;
using System.Text;

namespace XML_Converter
{
    public class Parameter
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public Parameter(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
