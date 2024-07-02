using PowerShellTools.Common.ServiceManagement.DebuggingContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace PowerShellToolsPro.Cmdlets.VSCode
{
    public class CompletionItem
    {
        public CompletionItem(FileInfo fileInfo)
        {
            InsertText = fileInfo.Name;
            CompletionKind = CompletionKind.File;
        }

        public CompletionItem(DirectoryInfo info)
        {
            InsertText = info.Name;
            CompletionKind = CompletionKind.Folder;
        }

        public CompletionItem(string name, CompletionKind kind)
        {
            InsertText = name;
            CompletionKind = kind;
        }

        public CompletionItem(CommandInfo info)
        {
            InsertText = info.Name;
            CompletionKind = CompletionKind.Function;
        }

        public CompletionItem(ParameterMetadata info)
        {
            InsertText = "-" + info.Name;
            CompletionKind = CompletionKind.Property;
        }

        public CompletionItem(Variable info)
        {
            InsertText = "$" + info.VarName;
            CompletionKind = CompletionKind.Variable;
        }

        public CompletionItem(Type info)
        {
            InsertText = info.FullName;
            CompletionKind = CompletionKind.Class;
        }

        public CompletionItem(PSObject obj)
        {
            if (obj.Properties.Any(m => m.Name == "InsertText"))
            {
                InsertText = obj.Properties["InsertText"].Value.ToString();
            }

            if (obj.Properties.Any(m => m.Name == "Name"))
            {
                InsertText = obj.Properties["Name"].Value.ToString();
            }

            if (obj.Properties.Any(m => m.Name == "CompletionKind"))
            {
                CompletionKind = (CompletionKind)Enum.Parse(typeof(CompletionKind), obj.Properties["CompletionKind"].Value.ToString());
            }

            if (obj.Properties.Any(m => m.Name == "MemberType"))
            {
                CompletionKind kind;
                var type = (PSMemberTypes)Enum.Parse(typeof(PSMemberTypes), obj.Properties["MemberType"].Value.ToString());
                switch(type)
                {
                    case PSMemberTypes.Method:
                    case PSMemberTypes.CodeMethod:
                    case PSMemberTypes.ScriptMethod:
                        kind = CompletionKind.Method;
                        break;
                    default:
                        kind = CompletionKind.Property;
                        break;
                }

                CompletionKind = kind;
            }
        }

        public string InsertText { get; set; }
        public CompletionKind CompletionKind { get; set; }
    }

    public enum CompletionKind
    {
        Text = 0,
        Method = 1,
        Function = 2,
        Constructor = 3,
        Field = 4,
        Variable = 5,
        Class = 6,
        Interface = 7,
        Module = 8,
        Property = 9,
        Unit = 10,
        Value = 11,
        Enum = 12,
        Keyword = 13,
        Snippet = 14,
        Color = 15,
        Reference = 17,
        File = 16,
        Folder = 18,
        EnumMember = 19,
        Constant = 20,
        Struct = 21,
        Event = 22,
        Operator = 23,
        TypeParameter = 24
    }
}
