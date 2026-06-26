using System;
using System.Collections.Generic;
using System.Text;

namespace Builder.Dialog
{
    public class DialogLine
    {
        public string name { get; set; }
        public string line { get; set; }

        public DialogLine(string name, string line)
        {
            this.name = name;
            this.line = line;
        }
    }
}
