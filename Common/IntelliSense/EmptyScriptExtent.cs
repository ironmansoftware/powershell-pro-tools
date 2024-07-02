using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;

namespace PowerShellTools.Common.IntelliSense
{
    [Serializable]
    internal sealed class EmptyScriptExtent : IScriptExtent
    {
        public string File
        {
            get
            {
                return null;
            }
        }
        public IScriptPosition StartScriptPosition
        {
            get
            {
                return new EmptyScriptPosition();
            }
        }
        public IScriptPosition EndScriptPosition
        {
            get
            {
                return new EmptyScriptPosition();
            }
        }
        public int StartLineNumber
        {
            get
            {
                return 0;
            }
        }
        public int StartColumnNumber
        {
            get
            {
                return 0;
            }
        }
        public int EndLineNumber
        {
            get
            {
                return 0;
            }
        }
        public int EndColumnNumber
        {
            get
            {
                return 0;
            }
        }
        public int StartOffset
        {
            get
            {
                return 0;
            }
        }
        public int EndOffset
        {
            get
            {
                return 0;
            }
        }
        public string Text
        {
            get
            {
                return "";
            }
        }

        public override int GetHashCode()
        {
            return File.GetHashCode() ^ 
                   StartLineNumber.GetHashCode() ^ 
                   StartColumnNumber.GetHashCode() ^ 
                   EndLineNumber.GetHashCode() ^ 
                   EndColumnNumber.GetHashCode() ^ 
                   Text.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var scriptExtent = obj as IScriptExtent;
            return scriptExtent != null 
                && string.IsNullOrEmpty(scriptExtent.File) 
                && scriptExtent.StartLineNumber == StartLineNumber 
                && scriptExtent.StartColumnNumber == StartColumnNumber 
                && scriptExtent.EndLineNumber == EndLineNumber 
                && scriptExtent.EndColumnNumber == EndColumnNumber 
                && string.IsNullOrEmpty(scriptExtent.Text);
        }
    }
}
