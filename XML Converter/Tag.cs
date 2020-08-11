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
        public int Length { get; set; } // IVB: string length

        public Tag(string id, string definition, string type)
        {
            Id = id;
            Definition = definition;
            Type = type;

            string[] parts = type.Split(new char[] {'(',')',','}, StringSplitOptions.RemoveEmptyEntries); // IVB: split "<type>(<len>)" to "<type>", "<len>", ""
            if (parts.Length > 1)
            {
                Type = parts[0];
                try // IVB: avoid exception on "Dec(<L>,<D>)" types
                {
                    Length = int.Parse(parts[1]);

                    if (parts[2] != " ")
                    {
                        Length += int.Parse(parts[2]); //now it works
                    }   
                }
                catch (Exception) { }
            }
        }
    }
}
