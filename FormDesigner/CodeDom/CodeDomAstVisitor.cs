using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;

namespace PowerShellToolsPro.FormsDesigner
{
    public class CodeDomAstVisitor : ICustomAstVisitor
    {
        protected CodeCompileUnit unit = new CodeCompileUnit();
        protected CodeTypeDeclaration _currentClass;
        protected CodeMemberMethod _currentMethod;
        protected CodeConstructor _currentConstructor;

		public object Visit(Ast ast)
		{
			if (ast is ArrayExpressionAst)
			{
				return VisitArrayExpression(ast as ArrayExpressionAst);
			}

			if (ast is ArrayLiteralAst)
			{
				return VisitArrayLiteral(ast as ArrayLiteralAst);
			}

			if (ast is ConvertExpressionAst)
			{
				return VisitConvertExpression(ast as ConvertExpressionAst);
			}

			if (ast is InvokeMemberExpressionAst)
			{
				return VisitInvokeMemberExpression(ast as InvokeMemberExpressionAst);
			}

			if (ast is ScriptBlockAst)
			{
				return VisitScriptBlock(ast as ScriptBlockAst);
			}

			if (ast is ScriptBlockExpressionAst)
			{
				return VisitScriptBlockExpression(ast as ScriptBlockExpressionAst);
			}

			if (ast is VariableExpressionAst)
			{
				return VisitVariableExpression(ast as VariableExpressionAst);
			}

			if (ast is FunctionDefinitionAst)
			{
				return VisitFunctionDefinition(ast as FunctionDefinitionAst);
			}

			if (ast is AssignmentStatementAst)
			{
				return VisitAssignmentStatement(ast as AssignmentStatementAst);
			}

			if (ast is CommandAst)
			{
				return VisitCommand(ast as CommandAst);
			}

			if (ast is CommandExpressionAst)
			{
				return VisitCommandExpression(ast as CommandExpressionAst);
			}

			if (ast is PipelineAst)
			{
				return VisitPipeline(ast as PipelineAst);
			}

			if (ast is MemberExpressionAst)
			{
				return VisitMemberExpression(ast as MemberExpressionAst);
			}

			if (ast is ConstantExpressionAst)
			{
				return VisitConstantExpression(ast as ConstantExpressionAst);
			}

			if (ast is StatementBlockAst)
			{
				return VisitStatementBlock(ast as StatementBlockAst);
			}

			if (ast is TypeExpressionAst)
			{
				return VisitTypeExpression(ast as TypeExpressionAst);
			}

			if (ast is ParenExpressionAst)
			{
				return VisitParenExpression(ast as ParenExpressionAst);
			}


            if (ast is BinaryExpressionAst)
            {
                return VisitBinaryExpression(ast as BinaryExpressionAst);
            }


            return unit;
        }

        

        public object VisitErrorStatement(ErrorStatementAst errorStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitErrorExpression(ErrorExpressionAst errorExpressionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            foreach (var statement in scriptBlockAst.EndBlock.Statements)
            {
                Visit(statement);
            }
            return unit;
        }

        public object VisitParamBlock(ParamBlockAst paramBlockAst)
        {
            throw new NotImplementedException();
        }

        public object VisitNamedBlock(NamedBlockAst namedBlockAst)
        {
            throw new NotImplementedException();
        }

        public object VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
        {
            throw new NotImplementedException();
        }

        public object VisitAttribute(AttributeAst attributeAst)
        {
            throw new NotImplementedException();
        }

        public object VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst)
        {
            throw new NotImplementedException();
        }

        public object VisitParameter(ParameterAst parameterAst)
        {
            throw new NotImplementedException();
        }

        public object VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            _currentMethod = new CodeMemberMethod();

            _currentMethod.Name = functionDefinitionAst.Name;

            if (functionDefinitionAst.Parameters != null)
            {
                foreach (var parameter in functionDefinitionAst.Parameters)
                {
                    _currentMethod.Parameters.Add(new CodeParameterDeclarationExpression(parameter.StaticType,
                        parameter.Name.VariablePath.ToString()));
                }    
            }

            _currentClass.Members.Add(_currentMethod);

            foreach (var statement in functionDefinitionAst.Body.EndBlock.Statements)
            {
                var code = Visit(statement);
                if (code is CodeStatement)
                {
                    _currentMethod.Statements.Add(code as CodeStatement);
                }
                if (code is CodeMethodInvokeExpression)
                {
                    _currentMethod.Statements.Add(new CodeExpressionStatement(code as CodeExpression));
                }
                else if (code is CodeExpression)
                {
                    _currentMethod.Statements.Add(code as CodeExpression);
                }
            }

            _currentMethod = null;

            return unit;
        }

        public object VisitStatementBlock(StatementBlockAst statementBlockAst)
        {
            var statements = new CodeStatementCollection();
            foreach (var code in statementBlockAst.Statements.Select(Visit))
            {
                if (code is CodeStatement)
                {
                    statements.Add(code as CodeStatement);
                }
                if (code is CodeExpression)
                {
                    statements.Add(code as CodeExpression);
                }
            }

            return statements;
        }

        public object VisitIfStatement(IfStatementAst ifStmtAst)
        {
            throw new NotImplementedException();
        }

        public object VisitTrap(TrapStatementAst trapStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitSwitchStatement(SwitchStatementAst switchStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitDataStatement(DataStatementAst dataStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitForEachStatement(ForEachStatementAst forEachStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitForStatement(ForStatementAst forStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitWhileStatement(WhileStatementAst whileStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitCatchClause(CatchClauseAst catchClauseAst)
        {
            throw new NotImplementedException();
        }

        public object VisitTryStatement(TryStatementAst tryStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitBreakStatement(BreakStatementAst breakStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitContinueStatement(ContinueStatementAst continueStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitExitStatement(ExitStatementAst exitStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitThrowStatement(ThrowStatementAst throwStatementAst)
        {
            throw new NotImplementedException();
        }

        public object VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst)
        {
            throw new NotImplementedException();
        }

        public virtual object VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            var variableExpression = assignmentStatementAst.Left as VariableExpressionAst;
            if (_currentClass == null && variableExpression != null)
            {
                var variableName = variableExpression.VariablePath.ToString();

	            var node = assignmentStatementAst.Right.Find(ast =>
	            {
		            var commandAst = ast as CommandAst;
		            if (commandAst == null) return false;
		            if (!commandAst.GetCommandName()
			            .Equals("New-Object", StringComparison.OrdinalIgnoreCase)) return false;
		            foreach (var element in commandAst.CommandElements)
		            {
			            var bareword = element as StringConstantExpressionAst;
			            if (bareword == null) continue;
			            if (bareword.Value.Equals("System.Windows.Forms.Form",
				            StringComparison.OrdinalIgnoreCase)) return true;
		            }

		            return false;
	            }, true);

				if (node != null)
                {
                    _currentClass = new CodeTypeDeclaration(variableName);
                    var codeNamespace = unit.Namespaces.Cast<CodeNamespace>().FirstOrDefault();
                    if (codeNamespace == null)
                    {
                        codeNamespace = new CodeNamespace("ns");
                        unit.Namespaces.Add(codeNamespace);
                    }

                    _currentConstructor = new CodeConstructor();
                    _currentConstructor.Attributes = MemberAttributes.Public;
                    _currentClass.Members.Add(_currentConstructor);
                    _currentClass.BaseTypes.Add("System.Windows.Forms.Form");

                    codeNamespace.Types.Add(_currentClass);
                    return new CodeExpression();
                }
            }
            else if (variableExpression != null && variableExpression.VariablePath.ToString() == "resources")
            {
                var type = new CodeTypeReference("System.ComponentModel.ComponentResourceManager");
                var newCrm = new CodeObjectCreateExpression(type, new CodeTypeOfExpression(_currentClass.Name));
                return new CodeVariableDeclarationStatement(type, "resources", newCrm);
            }
			else if (assignmentStatementAst.Left is ConvertExpressionAst)
			{
				var variableExpressionAst = (assignmentStatementAst.Left as ConvertExpressionAst).Find(m => m is VariableExpressionAst, true) as VariableExpressionAst;
				var variableName = variableExpressionAst?.VariablePath.UserPath;

				if (variableName != null && _currentMethod != null)
				{
					var convertExpression = assignmentStatementAst.Left as ConvertExpressionAst;
					var initialization = Visit(assignmentStatementAst.Right) as CodeExpression;
					var type = new CodeTypeReference(convertExpression.Type.TypeName.FullName);
					return new CodeVariableDeclarationStatement(type, variableName, initialization);
				}
			}

            var left = Visit(assignmentStatementAst.Left) as CodeExpression;
            if (_currentClass != null && _currentMethod == null && left is CodeVariableReferenceExpression)
            {
                var variableName = (left as CodeVariableReferenceExpression).VariableName;
                _currentClass.Members.Add(new CodeMemberField("System.Object", variableName));
                return new CodeExpression();
            }

            var rightEx = Visit(assignmentStatementAst.Right) as CodeExpression;

            return new CodeAssignStatement(left, rightEx);
        }

        public object VisitPipeline(PipelineAst pipelineAst)
        {
            foreach (var element in pipelineAst.PipelineElements)
            {
                return Visit(element);
            }

            return new CodeExpression();
        }

        public object VisitCommand(CommandAst commandAst)
        {
            if (commandAst.CommandElements[0] is StringConstantExpressionAst)
            {
                var commandName = (commandAst.CommandElements[0] as StringConstantExpressionAst).Value;
                if (commandName == "New-Object")
                {
                    var arguments = new List<CodeExpression>();
                    var elements = new List<CommandElementAst>(commandAst.CommandElements);

                    var typeNameIndex =
                        elements.FindIndex(
                            m =>
                                m is CommandParameterAst &&
                                (m as CommandParameterAst).ParameterName.Equals("TypeName",
                                    StringComparison.OrdinalIgnoreCase));

                    var argumentListIndex =
                        elements.FindIndex(
                            m =>
                                m is CommandParameterAst &&
                                (m as CommandParameterAst).ParameterName.Equals("ArgumentList",
                                    StringComparison.OrdinalIgnoreCase));

                    if (typeNameIndex == -1)
                    {
                        return new CodeExpression();
                    }

                    if (argumentListIndex != -1 && elements[argumentListIndex + 1] is ArrayExpressionAst)
                    {
                        var arrayExpression = Visit(elements[argumentListIndex + 1]);
                        if (arrayExpression is CodeArrayCreateExpression)
                        {
                            var collection = arrayExpression as CodeArrayCreateExpression;
							arguments.AddRange(collection.Initializers.OfType<CodeExpression>());
						}
                    }

                    var constant = elements[typeNameIndex + 1] as ConstantExpressionAst;
                    return new CodeObjectCreateExpression(constant.Value.ToString(), arguments.ToArray());
                }

                if (commandName.Equals("Import-LocalizedData", StringComparison.OrdinalIgnoreCase) && _currentClass != null)
                {
                    var type = new CodeTypeReference("System.ComponentModel.ComponentResourceManager");
                    var newCrm = new CodeObjectCreateExpression(type, new CodeTypeOfExpression(_currentClass.Name));
                    return new CodeVariableDeclarationStatement(type, "resources", newCrm);
                }

                if (_currentMethod == null)
                {
                    if (commandName.StartsWith(". "))
                    {
                        commandName = commandName.TrimStart('.', ' ');
                    }

                    var methodInvoke = new CodeMethodInvokeExpression(
                        new CodeThisReferenceExpression(), commandName);
                    _currentConstructor.Statements.Add(methodInvoke);

                    return new CodeExpression();
                }
            }
           

            return new CodeExpression();
        }


        public object VisitCommandExpression(CommandExpressionAst commandExpressionAst)
        {
            return Visit(commandExpressionAst.Expression);
        }

        public object VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            throw new NotImplementedException();
        }

        public object VisitFileRedirection(FileRedirectionAst fileRedirectionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitMergingRedirection(MergingRedirectionAst mergingRedirectionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            var left = Visit(binaryExpressionAst.Left) as CodeExpression;
            var right = Visit(binaryExpressionAst.Right) as CodeExpression;

            CodeBinaryOperatorType type = CodeBinaryOperatorType.Assign;
            switch(binaryExpressionAst.Operator)
            {
                case TokenKind.Bor:
                    type = CodeBinaryOperatorType.BitwiseOr;
                    break;

            }

            return new CodeBinaryOperatorExpression(left, type, right);
        }

        public object VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
        {
			var typeName = convertExpressionAst.Type.TypeName.FullName;
			if (_currentMethod == null)
            {
                var refExp = Visit(convertExpressionAst.Child) as CodeVariableReferenceExpression;
                if (refExp != null)
                {
                    _currentClass.Members.Add(new CodeMemberField(typeName, refExp.VariableName));
                }
            }

			var variableExpressionAst = convertExpressionAst.Child.Find(m => m is VariableExpressionAst, true) as VariableExpressionAst;
			if (variableExpressionAst?.VariablePath.ToString().Equals("resources") == true)
			{
                var memberExpression = convertExpressionAst.Child as MemberExpressionAst;
                var stringConstant = memberExpression?.Member as StringConstantExpressionAst;
				if (stringConstant != null)
				{
                    if (convertExpressionAst.Type.TypeName.Name == "System.String")
                    {
                        return new CodeCastExpression(typeName, new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("resources"), "GetString"), new CodePrimitiveExpression(stringConstant.Value)));
                    }
                    else
                    {
                        return new CodeCastExpression(typeName, new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("resources"), "GetObject"), new CodePrimitiveExpression(stringConstant.Value)));
                    }
					
				}
			}

			var childExpression = Visit(convertExpressionAst.Child) as CodeExpression;

			if (convertExpressionAst.Attribute.TypeName.FullName.EndsWith("[]"))
			{
				new CodeCastExpression(new CodeTypeReference(convertExpressionAst.Attribute.TypeName.FullName.TrimEnd('[',']'), 1), childExpression);
			}

			return new CodeCastExpression(new CodeTypeReference(convertExpressionAst.Attribute.TypeName.FullName), childExpression);
        }

        public object VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
        {
            return new CodePrimitiveExpression(constantExpressionAst.Value);
        }

        public object VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitSubExpression(SubExpressionAst subExpressionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            throw new NotImplementedException();
        }

		public object VisitVariableExpression(VariableExpressionAst variableExpressionAst)
		{
			if (_currentClass != null)
			{
				foreach (var member in _currentClass.Members)
				{
					var field = member as CodeMemberField;
					if (field != null && field.Name == variableExpressionAst.VariablePath.ToString())
					{
						return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name);
					}
				}

				if (_currentClass.Name.Equals(variableExpressionAst.VariablePath.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return new CodeThisReferenceExpression();
				}
			}

            var variableName = variableExpressionAst.VariablePath.ToString();

            if (variableName.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return new CodePrimitiveExpression(true);
            }

            if (variableName.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                return new CodePrimitiveExpression(false);
            }

			var varRef = new CodeVariableReferenceExpression(variableName);
			return varRef;
		}

        public object VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            var typeName = typeExpressionAst.TypeName.FullName;

            return new CodeTypeReferenceExpression(typeName);
        }

        public virtual object VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            var expression = Visit(memberExpressionAst.Expression);

            if (expression is CodeTypeReferenceExpression)
            {
                var fieldRef = new CodeFieldReferenceExpression(expression as CodeExpression, memberExpressionAst.Member.ToString());

                return fieldRef;
            }
            else
            {
                var fieldRef = new CodePropertyReferenceExpression(expression as CodeExpression,
                    memberExpressionAst.Member.ToString());

                return fieldRef;
            }
        }

        public object VisitInvokeMemberExpression(InvokeMemberExpressionAst invokeMemberExpressionAst)
        {
            var expression = Visit(invokeMemberExpressionAst.Expression);

            var memberName = invokeMemberExpressionAst.Member.ToString();

            if (memberName.StartsWith("add_"))
            {
                if (invokeMemberExpressionAst.Arguments.Count != 1)
                {
                    return new CodeExpression();
                }

                var codeArg = Visit(invokeMemberExpressionAst.Arguments[0]) as CodeVariableReferenceExpression;
                if (codeArg != null)
                {
                    var stubMethod = new CodeMemberMethod {Name = codeArg.VariableName};
                    _currentClass.Members.Add(stubMethod);

                    stubMethod.UserData.Add("DontGenerate", null);

                    var variable = new CodeDelegateCreateExpression(new CodeTypeReference(typeof (EventHandler)),
                        new CodeThisReferenceExpression(), codeArg.VariableName);

                    return new CodeAttachEventStatement(expression as CodeExpression,
                        memberName.Replace("add_", String.Empty), variable);
                }
            }

            var methodInvoke = new CodeMethodInvokeExpression(expression as CodeExpression,
                memberName);

            if (invokeMemberExpressionAst.Arguments == null)
                return methodInvoke;

            foreach (var arg in invokeMemberExpressionAst.Arguments)
            {
				CodeExpression codeArg = null;
				var array = arg as ArrayExpressionAst;
				if (array != null)
				{
					var arrayLiteral = array.SubExpression.Find(m => m is ArrayLiteralAst, true);
					if (arrayLiteral != null)
						codeArg = VisitArrayLiteral(arrayLiteral as ArrayLiteralAst) as CodeExpression;
				}
				else
				{
					codeArg = Visit(arg) as CodeExpression;
				}
				
                if(codeArg != null)
                    methodInvoke.Parameters.Add(codeArg as CodeExpression);
            }

            return methodInvoke;
        }

		public object VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
		{
			var arrayLiteral = arrayExpressionAst.SubExpression.Find(m => m is ArrayLiteralAst, false) as ArrayLiteralAst;
			if (arrayLiteral != null)
			{
				return VisitArrayLiteral(arrayLiteral);
			}
			else
			{
				string typeName = "System.Object";
				var items = new List<CodeExpression>();

				var conversions = arrayExpressionAst.FindAll(m => m is ConvertExpressionAst, false).OfType<ConvertExpressionAst>();

				// This happens when there is a single item in an array that is a variable
				var convertExpression = arrayExpressionAst.Parent as ConvertExpressionAst;
				if (!conversions.Any() && convertExpression != null)
				{
					typeName = convertExpression.Type.TypeName.FullName;

					var item = Visit(arrayExpressionAst.SubExpression);

					if (item is CodeStatementCollection)
					{
						foreach(var expr in (item as CodeStatementCollection))
						{
							var codeExpr = expr as CodeExpression;
							if (codeExpr != null)
                            {
                                items.Add(codeExpr);
                            }
							else if (expr is CodeExpressionStatement statement)
                            {
                                items.Add(statement.Expression);
                            }
						}
					}
				}
				// This happens when you have one or more items and they aren't variables.
				else if (conversions.Any())
				{
					typeName = conversions.First().Type.TypeName.FullName;

					foreach (var item in conversions)
					{
						var castExpression = Visit(item) as CodeExpression;
						if (castExpression != null)
							items.Add(castExpression);
					}
				}

				return new CodeArrayCreateExpression(typeName, items.ToArray());
			}
        }

        public object VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
        {
			var typeName = "System.Object";
			var conversions = arrayLiteralAst.Find(m => m is ConvertExpressionAst, false) as ConvertExpressionAst;
			if (conversions != null)
				typeName = conversions.Type.TypeName.FullName;

			var convertExpression = arrayLiteralAst.Parent.Parent.Parent.Parent.Parent as ConvertExpressionAst;
			if (conversions == null && convertExpression != null)
			{
				typeName = convertExpression.Type.TypeName.FullName;
			}

			var items = arrayLiteralAst.Elements.Select(Visit).OfType<CodeExpression>().ToArray();
            return new CodeArrayCreateExpression(typeName, items);
        }

        public object VisitHashtable(HashtableAst hashtableAst)
        {
            throw new NotImplementedException();
        }

        public object VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            var endBlock = scriptBlockExpressionAst.ScriptBlock.EndBlock;
            foreach (var statement in endBlock.Statements)
            {
                Visit(statement);
            }

            return unit;
        }

        public object VisitParenExpression(ParenExpressionAst parenExpressionAst)
        {
			return VisitPipeline(parenExpressionAst.Pipeline as PipelineAst);
        }

        public object VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitIndexExpression(IndexExpressionAst indexExpressionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst)
        {
            throw new NotImplementedException();
        }

        public object VisitBlockStatement(BlockStatementAst blockStatementAst)
        {
            throw new NotImplementedException();
        }
}
}
