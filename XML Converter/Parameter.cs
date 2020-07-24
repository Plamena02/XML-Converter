using System;
using System.Collections.Generic;
using System.Text;

namespace XML_Converter
{
    public class Parameter
    {
        public enum Names { config, input, output };

        public string Name { get; set; }

        public string Value { get; set; }

        public Parameter(int num, string value)
        {
            Name = Enum.GetName(typeof(Names), num);
            Value = value;
        }

        // IVB: could be replaced with the following, where calling could be both "new Parameter(<enum>, <value>)"
        //                                            and "new Parameter((Names)<int-number>, <value>)"
        public Parameter(Names type, string value)
        {
            Name = type.ToString();
            Value = value;
        }

    }
}
