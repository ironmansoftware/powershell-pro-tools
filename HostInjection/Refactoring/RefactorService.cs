using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace PowerShellProTools.Host.Refactoring
{
    public static class RefactorService
    {
        private static List<IRefactoring> _refactorings;

        static RefactorService()
        {
            _refactorings = new List<IRefactoring>
            {
                new ExtractFile(),
                new ExportModuleMember(),
                new ConvertToSplat(),
                new ExtractFunction(),
                new ConvertToMultiLineCommand(),
                new GenerateFunctionFromUsage(),
                new Reorder(),
                new SplitPipe(),
                new IntroduceUsing(),
                new ConvertToPsItem(),
                new ConvertToDollarUnder(),
                new GenerateProxyFunction(),
                new ExtractVariable()
            };
        }

        public static IEnumerable<RefactorInfo> GetValidRefactors(RefactorRequest request)
        {
            var refactors = new List<RefactorInfo>();
            if (request.EditorState.Content == null) return refactors;

            var ast = Parser.ParseInput(request.EditorState.Content, out Token[] tokens, out ParseError[] errors);
   
            foreach(var refactoring in _refactorings)
            {
                try
                {
                    if (refactoring.CanRefactor(request.EditorState, ast, request.Properties))
                    {
                        refactors.Add(refactoring.RefactorInfo);
                    }
                }
                catch
                {
                    continue;
                }
            }

            return refactors;
        }

        public static IEnumerable<TextEdit> Refactor(RefactorRequest request)
        {
            try
            {
                var refactoring = _refactorings.FirstOrDefault(m => m.RefactorInfo.Type == request.Type);
                if (refactoring == null) return new[] { TextEdit.None };

                var ast = Parser.ParseInput(request.EditorState.Content, out Token[] tokens, out ParseError[] errors);

                return refactoring.Refactor(request.EditorState, ast, request.Properties).ToArray();
            }
            catch
            {
                return new [] { TextEdit.None };    
            }
        }
    }
}
