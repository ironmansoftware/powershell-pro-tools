using System;
using System.Collections.Generic;
using System.Text;

namespace PowerShellProTools.Host
{
    public class AstNode
    {
        public string AstType { get; set; }
        public int EndOffset { get; set; }
        public int StartOffset { get; set; }
        public string AstContent { get; set;  }
        public int HashCode { get; set; }
    }
}
