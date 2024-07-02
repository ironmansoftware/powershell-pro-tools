using System.Collections.Generic;

namespace PowerShellToolsPro.Cmdlets.Profiling
{
    public class SequencePoint
    {
        public string ModuleName { get; set; }
        public string CommandName { get; set; }
        public string PipelineMethod { get; set; }

        public string FileName { get; set; }
        public int StartOffset { get; set; }
        public int EndOffset { get; set; }

        public bool FromAst => string.IsNullOrEmpty(CommandName);
        public bool Root { get; set; }

        public SequencePoint() { }

        public SequencePoint(string moduleName, string commandName, string pipelineMethod)
        {
            ModuleName = moduleName;
            CommandName = commandName;
            PipelineMethod = pipelineMethod;
        }

        public SequencePoint(string fileName, int startOffset, int endOffset)
        {
            FileName = fileName;
            StartOffset = startOffset;
            EndOffset = endOffset;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(ModuleName))
            {
                return $"{FileName}:{StartOffset}:{EndOffset}";
            }
            else
            {
                return $"{ModuleName}:{CommandName}:{PipelineMethod}";
            }
        }

        public override bool Equals(object obj)
        {
            var sequencePoint = obj as SequencePoint;
            if (sequencePoint == null) return false;

            return StartOffset == sequencePoint.StartOffset && EndOffset == sequencePoint.EndOffset && FileName == sequencePoint.FileName && ModuleName == sequencePoint.ModuleName && CommandName == sequencePoint.CommandName && PipelineMethod == sequencePoint.PipelineMethod;
        }

        public override int GetHashCode()
        {
            var hashCode = -492400078;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ModuleName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CommandName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PipelineMethod);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileName);
            hashCode = hashCode * -1521134295 + StartOffset.GetHashCode();
            hashCode = hashCode * -1521134295 + EndOffset.GetHashCode();
            return hashCode;
        }
    }
}
