using System;
using System.Collections.Generic;
using System.Text;

namespace XML_Converter
{
    public enum ParamNames { config, input, output };
    public enum ParamShortNames { c, i, o };

    public class Parameter
    {

        public string Name { get; set; }

        public string Value { get; set; }

        // IVB: could be replaced with the following, where calling could be both "new Parameter(<enum>, <value>)"
        //                                            and "new Parameter((Names)<int-number>, <value>)"
        public Parameter(ParamNames type, string value)
        {
            Name = type.ToString();
            Value = value;
        }

        public Parameter(int num, string value)
        {
            Name = Enum.GetName(typeof(ParamNames), num);
            Value = value;
        }

        public Parameter(ParamShortNames type, string value)
        {
            Name = type.ToString();
            Value = value;
        }
    }
}
