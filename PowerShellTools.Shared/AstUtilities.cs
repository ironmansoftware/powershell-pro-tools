using System.Management.Automation.Language;

namespace PowerShellToolsPro
{
	public static class AstUtilities
	{
		public static FunctionDefinitionAst FindFunctionDefinitionAst(this Ast ast, string methodName)
		{
			return ast.Find(m => m is FunctionDefinitionAst && ((FunctionDefinitionAst)m).Name == methodName, false) as FunctionDefinitionAst;
		}

	}
}
