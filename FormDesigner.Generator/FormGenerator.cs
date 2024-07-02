using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;

namespace IMS.FormDesigner
{
    public class FormGenerator
    {
        public string GenerateLogic(string fileContents, string filePath, string formPath)
        {
            var scriptBlock = ScriptBlock.Create(fileContents);

            var tabs = GetFieldsFromParamBlock(scriptBlock);

            var funcDef = scriptBlock.Ast.Find(m => m is FunctionDefinitionAst, false) as FunctionDefinitionAst;

            return GenerateLogic(funcDef.Name, tabs, $". '{filePath}'", formPath);
        }

        public string GenerateLogic(ScriptBlock scriptBlock)
        {
            var tabs = GetFieldsFromParamBlock(scriptBlock);

            var funcDef = scriptBlock.Ast.Find(m => m is FunctionDefinitionAst, false) as FunctionDefinitionAst;

            return GenerateLogic(funcDef.Name, tabs);
        }

        public string GenerateLogic(FunctionInfo functionInfo)
        {
            var tabs = GetFieldsFromParamBlock(functionInfo);
            return GenerateLogic(functionInfo.Name, tabs, functionInfo.Definition);
        }

        public string GenerateLogic(CmdletInfo cmdletInfo)
        {
            var tabs = GetFieldsFromParamBlock(cmdletInfo);
            return GenerateLogic(cmdletInfo.Name, tabs);
        }


        public string GenerateLogic(string functionName, IEnumerable<FormTab> tabs, string definition = null, string formPath = null)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(definition);

            stringBuilder.AppendLine("Add-Type -AssemblyName System.Windows.Forms");

            foreach (var tab in tabs)
            {
                stringBuilder.AppendLine($"$btn{tab.Name}_Click = {{");
                stringBuilder.Append($"\t{functionName} ");
                foreach (var parameter in tab.FormFields)
                {
                    if (parameter.Type == FieldTypes.Textbox)
                    {
                        stringBuilder.Append($"-{parameter.Name} $txt{parameter.Name}.Text ");
                    }
                    else if (parameter.Type == FieldTypes.Checkbox)
                    {
                        if (parameter.DotNetType == typeof(SwitchParameter))
                        {
                            stringBuilder.Append($"-{parameter.Name}:$chk{parameter.Name}.Checked ");
                        }
                        else
                        {
                            stringBuilder.Append($"-{parameter.Name} $chk{parameter.Name}.Checked ");
                        }
                    }
                    else if (parameter.Type == FieldTypes.Select)
                    {
                        stringBuilder.Append($"-{parameter.Name} $com{parameter.Name}.SelectedItem ");
                    }
                }

                stringBuilder.AppendLine();
                stringBuilder.AppendLine("}");
            }

            if (formPath == null)
            {
                stringBuilder.AppendLine(". $MyInvocation.InvocationName.Replace('form.ps1', 'form.designer.ps1')");
            }
            else
            {
                stringBuilder.AppendLine($". '{formPath}'");
            }
            
            stringBuilder.AppendLine("$Form1.ShowDialog()");
            return stringBuilder.ToString();
        }

        public string GenerateForm(ScriptBlock scriptBlock)
        {
            var tabs = GetFieldsFromParamBlock(scriptBlock);
            return GenerateForm(tabs);
        }

        public string GenerateForm(FunctionInfo functionInfo)
        {
            var tabs = GetFieldsFromParamBlock(functionInfo);
            return GenerateForm(tabs);
        }

        public string GenerateForm(CmdletInfo cmdletInfo)
        {
            var tabs = GetFieldsFromParamBlock(cmdletInfo);
            return GenerateForm(tabs);
        }

        public string GenerateForm(IEnumerable<FormTab> tabs)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("$Form1 = New-Object -TypeName System.Windows.Forms.Form");
            var tallestTab = (tabs.Max(m => m.FormFields.Count()) * 40) + 50;

            var multipleTabs = tabs.Count() > 1;

            if (multipleTabs)
            {
                stringBuilder.AppendLine("[System.Windows.Forms.TabControl]$tabControl = $null");
            }

            foreach (var item in tabs)
            {
                if (multipleTabs)
                {
                    stringBuilder.AppendLine($"[System.Windows.Forms.TabPage]$tab{item.Name} = $null");
                }

                stringBuilder.AppendLine($"[System.Windows.Forms.Button]$btn{item.Name} = $null");

                foreach (var parameter in item.FormFields)
                {
                    if (parameter.Type == FieldTypes.Textbox)
                    {
                        stringBuilder.AppendLine($"[System.Windows.Forms.Label]${item.Name}lbl{parameter.Name} = $null");
                        stringBuilder.AppendLine($"[System.Windows.Forms.TextBox]${item.Name}txt{parameter.Name} = $null");
                    }
                    else if (parameter.Type == FieldTypes.Checkbox)
                    {
                        stringBuilder.AppendLine($"[System.Windows.Forms.CheckBox]${item.Name}chk{parameter.Name} = $null");
                    }
                    else if (parameter.Type == FieldTypes.Select)
                    {
                        stringBuilder.AppendLine($"[System.Windows.Forms.Label]${item.Name}lbl{parameter.Name} = $null");
                        stringBuilder.AppendLine($"[System.Windows.Forms.ComboBox]${item.Name}com{parameter.Name} = $null");
                    }
                }
            }

            stringBuilder.AppendLine("function InitializeComponent");
            stringBuilder.AppendLine("{");

            if (multipleTabs)
            {
                stringBuilder.AppendLine($"$tabControl = (New-Object -TypeName System.Windows.Forms.TabControl)");

                stringBuilder.AppendLine($"$tabControl.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]10,[System.Int32]10))");
                stringBuilder.AppendLine($"$tabControl.Name = [System.String]'tabControl'");
                stringBuilder.AppendLine($"$tabControl.Padding = (New-Object -TypeName System.Windows.Forms.Padding -ArgumentList @([System.Int32]3))");
                stringBuilder.AppendLine($"$tabControl.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]360,[System.Int32]{tallestTab - 5}))");
            }

            foreach (var item in tabs)
            {
                if (multipleTabs)
                {
                    stringBuilder.AppendLine($"$tab{item.Name} = (New-Object -TypeName System.Windows.Forms.TabPage)");
                }

                stringBuilder.AppendLine($"$btn{item.Name} = (New-Object -TypeName System.Windows.Forms.Button)");

                foreach (var parameter in item.FormFields)
                {
                    if (parameter.Type == FieldTypes.Textbox)
                    {
                        stringBuilder.AppendLine($"${item.Name}lbl{parameter.Name} = (New-Object -TypeName System.Windows.Forms.Label)");
                        stringBuilder.AppendLine($"${item.Name}txt{parameter.Name} = (New-Object -TypeName System.Windows.Forms.TextBox)");
                    }
                    else if (parameter.Type == FieldTypes.Checkbox)
                    {
                        stringBuilder.AppendLine($"${item.Name}chk{parameter.Name} = (New-Object -TypeName System.Windows.Forms.CheckBox)");
                    }
                    else if (parameter.Type == FieldTypes.Select)
                    {
                        stringBuilder.AppendLine($"${item.Name}lbl{parameter.Name} = (New-Object -TypeName System.Windows.Forms.Label)");
                        stringBuilder.AppendLine($"${item.Name}com{parameter.Name} = (New-Object -TypeName System.Windows.Forms.ComboBox)");
                    }
                }

                if (multipleTabs)
                {
                    stringBuilder.AppendLine($"$tab{item.Name}.SuspendLayout()");
                }
            }

            if (multipleTabs)
            {
                stringBuilder.AppendLine("$tabControl.SuspendLayout()");
            }

            stringBuilder.AppendLine("$form1.SuspendLayout()");

            int tabIndex = 1;

            foreach (var item in tabs)
            {
                int row = 0;
                foreach (var parameter in item.FormFields)
                {
                    if (parameter.Type == FieldTypes.Textbox)
                    {
                        var controlName = $"{item.Name}lbl{parameter.Name}";
                        stringBuilder.AppendLine("#");
                        stringBuilder.AppendLine($"# {controlName}");
                        stringBuilder.AppendLine("#");
                        stringBuilder.AppendLine($"${controlName}.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]12,[System.Int32]{10 + (row * 30)}))");
                        stringBuilder.AppendLine($"${controlName}.Name = [System.String]'{controlName}'");
                        stringBuilder.AppendLine($"${controlName}.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]100,[System.Int32]23))");
                        stringBuilder.AppendLine($"${controlName}.TabIndex = {tabIndex}");
                        stringBuilder.AppendLine($"${controlName}.Text = '{parameter.Name}'");
                        stringBuilder.AppendLine($"${controlName}.UseCompatibleTextRendering = $true");

                        tabIndex++;

                        controlName = $"{item.Name}txt{parameter.Name}";
                        stringBuilder.AppendLine("#");
                        stringBuilder.AppendLine($"# {controlName}");
                        stringBuilder.AppendLine("#");
                        stringBuilder.AppendLine($"${controlName}.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]120,[System.Int32]{10 + (row * 30)}))");
                        stringBuilder.AppendLine($"${controlName}.Name = [System.String]'{controlName}'");
                        stringBuilder.AppendLine($"${controlName}.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]200,[System.Int32]23))");
                        stringBuilder.AppendLine($"${controlName}.TabIndex = {tabIndex}");
                    }
                    else if (parameter.Type == FieldTypes.Checkbox)
                    {
                        var controlName = $"{item.Name}chk{parameter.Name}";
                        stringBuilder.AppendLine("#");
                        stringBuilder.AppendLine($"# {controlName}");
                        stringBuilder.AppendLine("#");
                        stringBuilder.AppendLine($"${controlName}.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]12,[System.Int32]{10 + (row * 30)}))");
                        stringBuilder.AppendLine($"${controlName}.Name = [System.String]'{controlName}'");
                        stringBuilder.AppendLine($"${controlName}.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]100,[System.Int32]23))");
                        stringBuilder.AppendLine($"${controlName}.TabIndex = {tabIndex}");
                        stringBuilder.AppendLine($"${controlName}.Text = '{parameter.Name}'");
                        stringBuilder.AppendLine($"${controlName}.UseCompatibleTextRendering = $true");
                        stringBuilder.AppendLine($"${controlName}.UseVisualStyleBackColor = $true");
                    }
                    else if (parameter.Type == FieldTypes.Select)
                    {
                        var controlName = $"{item.Name}lbl{parameter.Name}";
                        stringBuilder.AppendLine("#");
                        stringBuilder.AppendLine($"# {controlName}");
                        stringBuilder.AppendLine("#");
                        stringBuilder.AppendLine($"${controlName}.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]12,[System.Int32]{10 + (row * 30)}))");
                        stringBuilder.AppendLine($"${controlName}.Name = [System.String]'{controlName}'");
                        stringBuilder.AppendLine($"${controlName}.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]100,[System.Int32]23))");
                        stringBuilder.AppendLine($"${controlName}.TabIndex = {tabIndex}");
                        stringBuilder.AppendLine($"${controlName}.Text = '{parameter.Name}'");
                        stringBuilder.AppendLine($"${controlName}.UseCompatibleTextRendering = $true");

                        tabIndex++;

                        var validOptions = parameter.ValidOptions.Select(m => $"[System.String]'{m.ToString()}'").Aggregate((x, y) => x + "," + y);

                        controlName = $"{item.Name}com{parameter.Name}";
                        stringBuilder.AppendLine("#");
                        stringBuilder.AppendLine($"# {controlName}");
                        stringBuilder.AppendLine("#");
                        stringBuilder.AppendLine($"${controlName}.ImeMode = [System.Windows.Forms.ImeMode]::NoControl");
                        stringBuilder.AppendLine($"${controlName}.Items.AddRange([System.Object[]]@({validOptions}))");
                        stringBuilder.AppendLine($"${controlName}.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]120,[System.Int32]{10 + (row * 30)}))");
                        stringBuilder.AppendLine($"${controlName}.Name = [System.String]'{controlName}'");
                        stringBuilder.AppendLine($"${controlName}.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]200,[System.Int32]23))");
                        stringBuilder.AppendLine($"${controlName}.TabIndex = {tabIndex}");
                    }

                    row++;
                    tabIndex++;
                }

                var buttonName = $"btn{item.Name}";
                stringBuilder.AppendLine("#");
                stringBuilder.AppendLine($"# {buttonName}");
                stringBuilder.AppendLine("#");

                stringBuilder.AppendLine($"${buttonName}.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]220,[System.Int32]{10 + (row * 30)}))");
                stringBuilder.AppendLine($"${buttonName}.Name = [System.String]'{buttonName}'");
                stringBuilder.AppendLine($"${buttonName}.Padding = (New-Object -TypeName System.Windows.Forms.Padding -ArgumentList @([System.Int32]3))");
                stringBuilder.AppendLine($"${buttonName}.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]100,[System.Int32]23))");
                stringBuilder.AppendLine($"${buttonName}.TabIndex = {tabIndex}");
                stringBuilder.AppendLine($"${buttonName}.Text = 'Submit'");
                stringBuilder.AppendLine($"${buttonName}.UseVisualStyleBackColor = $true");
                stringBuilder.AppendLine($"${buttonName}.add_Click(${buttonName}_Click)");

                if (multipleTabs)
                {
                    var controlName = $"tab{item.Name}";
                    stringBuilder.AppendLine("#");
                    stringBuilder.AppendLine($"# {controlName}");
                    stringBuilder.AppendLine("#");

                    foreach(var control in item.FormFields)
                    {
                        if (control.Type == FieldTypes.Checkbox)
                        {
                            stringBuilder.AppendLine($"${controlName}.Controls.Add(${item.Name}chk{control.Name})");
                        }

                        if (control.Type == FieldTypes.Textbox)
                        {
                            stringBuilder.AppendLine($"${controlName}.Controls.Add(${item.Name}lbl{control.Name})");
                            stringBuilder.AppendLine($"${controlName}.Controls.Add(${item.Name}txt{control.Name})");
                        }

                        if (control.Type == FieldTypes.Select)
                        {
                            stringBuilder.AppendLine($"${controlName}.Controls.Add(${item.Name}lbl{control.Name})");
                            stringBuilder.AppendLine($"${controlName}.Controls.Add(${item.Name}com{control.Name})");
                        }
                    }

                    stringBuilder.AppendLine($"${controlName}.Controls.Add(${buttonName})");

                    stringBuilder.AppendLine($"${controlName}.Location = (New-Object -TypeName System.Drawing.Point -ArgumentList @([System.Int32]10,[System.Int32]10))");
                    stringBuilder.AppendLine($"${controlName}.Name = [System.String]'{controlName}'");
                    stringBuilder.AppendLine($"${controlName}.Padding = (New-Object -TypeName System.Windows.Forms.Padding -ArgumentList @([System.Int32]3))");
                    stringBuilder.AppendLine($"${controlName}.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]360,[System.Int32]{tallestTab - 5}))");
                    stringBuilder.AppendLine($"${controlName}.TabIndex = {tabIndex}");
                    stringBuilder.AppendLine($"${controlName}.Text = '{controlName}'");
                    stringBuilder.AppendLine($"${controlName}.UseVisualStyleBackColor = $true");

                    tabIndex++;
                }
            }

            

            stringBuilder.AppendLine($"$Form1.ClientSize = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]380,[System.Int32]{tallestTab}))");

            if (multipleTabs)
            {
                foreach (var tab in tabs)
                {
                    stringBuilder.AppendLine($"$TabControl.TabPages.Add($tab{tab.Name})");
                }

                stringBuilder.AppendLine($"$Form1.Controls.Add($TabControl)");
            }
            else
            {
                foreach (var parameter in tabs.First().FormFields)
                {
                    if (parameter.Type == FieldTypes.Textbox)
                    {
                        stringBuilder.AppendLine($"$Form1.Controls.Add(${tabs.First().Name}lbl{parameter.Name})");
                        stringBuilder.AppendLine($"$Form1.Controls.Add(${tabs.First().Name}txt{parameter.Name})");
                    }
                    else if (parameter.Type == FieldTypes.Checkbox)
                    {
                        stringBuilder.AppendLine($"$Form1.Controls.Add(${tabs.First().Name}chk{parameter.Name})");
                    }
                    else if (parameter.Type == FieldTypes.Select)
                    {
                        stringBuilder.AppendLine($"$Form1.Controls.Add(${tabs.First().Name}lbl{parameter.Name})");
                        stringBuilder.AppendLine($"$Form1.Controls.Add(${tabs.First().Name}com{parameter.Name})");
                    }
                }

                stringBuilder.AppendLine($"$Form1.Controls.Add($btn{tabs.First().Name})");
            }

            stringBuilder.AppendLine($"$Form1.Text = [System.String]'Form1'");

            if (multipleTabs)
            {
                foreach(var tab in tabs)
                {
                    stringBuilder.AppendLine($"$tab{tab.Name}.ResumeLayout($true)");
                    stringBuilder.AppendLine($"$tab{tab.Name}.PerformLayout()");
                }
                stringBuilder.AppendLine($"$tabControl.ResumeLayout($true)");
            }

            stringBuilder.AppendLine($"$Form1.ResumeLayout($true)");
            
            foreach(var tab in tabs)
            {
                if (multipleTabs)
                {
                    stringBuilder.AppendLine($"Add-Member -InputObject $Form1 -Name tab{tab.Name} -Value $tab{tab.Name} -MemberType NoteProperty");
                }

                stringBuilder.AppendLine($"Add-Member -InputObject $Form1 -Name btn{tab.Name} -Value $btn{tab.Name} -MemberType NoteProperty");

                foreach (var parameter in tab.FormFields)
                {
                    if (parameter.Type == FieldTypes.Textbox)
                    {
                        var controlName = $"{tab.Name}lbl{parameter.Name}";
                        stringBuilder.AppendLine($"Add-Member -InputObject $Form1 -Name {controlName} -Value ${controlName} -MemberType NoteProperty");
                        controlName = $"{tab.Name}txt{parameter.Name}";
                        stringBuilder.AppendLine($"Add-Member -InputObject $Form1 -Name {controlName} -Value ${controlName} -MemberType NoteProperty");
                    }
                    else if (parameter.Type == FieldTypes.Checkbox)
                    {
                        var controlName = $"{tab.Name}chk{parameter.Name}";
                        stringBuilder.AppendLine($"Add-Member -InputObject $Form1 -Name {controlName} -Value ${controlName} -MemberType NoteProperty");
                    }
                    else if (parameter.Type == FieldTypes.Select)
                    {
                        var controlName = $"{tab.Name}lbl{parameter.Name}";
                        stringBuilder.AppendLine($"Add-Member -InputObject $Form1 -Name {controlName} -Value ${controlName} -MemberType NoteProperty");
                        controlName = $"{tab.Name}com{parameter.Name}";
                        stringBuilder.AppendLine($"Add-Member -InputObject $Form1 -Name {controlName} -Value ${controlName} -MemberType NoteProperty");
                    }
                }
            }

            if (multipleTabs)
            {
                stringBuilder.AppendLine($"Add-Member -InputObject $Form1 -Name tabControl -Value $tabControl -MemberType NoteProperty");
            }

            stringBuilder.AppendLine("}");
            stringBuilder.AppendLine(". InitializeComponent");

            return stringBuilder.ToString();
        }

        private IEnumerable<FormTab> GetFieldsFromParamBlock(ScriptBlock scriptBlock)
        {
            var funcDef = scriptBlock.Ast.Find(m => m is FunctionDefinitionAst, false) as FunctionDefinitionAst;

            if (funcDef == null)
            {
                throw new Exception("Failed to find a function definition to generate a form with.");
            }

            return GetFieldsFromParamBlock(funcDef);
        }

        private IEnumerable<FormTab> GetFieldsFromParamBlock(FunctionDefinitionAst functionDefinitionAst)
        {
            var script = functionDefinitionAst.ToString();
            var functionName = functionDefinitionAst.Name;

            FunctionInfo cmdletInfo;
            using (var ps = PowerShell.Create())
            {
                ps.AddScript(script);
                ps.AddStatement().AddCommand("Get-Command").AddParameter("Name", functionName);
                cmdletInfo = ps.Invoke<FunctionInfo>().FirstOrDefault();
            }

            if (cmdletInfo == null)
            {
                throw new Exception("Failed to get cmdletinfo");
            }

            return GetFieldsFromParamBlock(cmdletInfo.ParameterSets);
        }

        private IEnumerable<FormTab> GetFieldsFromParamBlock(CmdletInfo cmdletInfo)
        {
            return GetFieldsFromParamBlock(cmdletInfo.ParameterSets);
        }

        private IEnumerable<FormTab> GetFieldsFromParamBlock(FunctionInfo cmdletInfo)
        {
            return GetFieldsFromParamBlock(cmdletInfo.ParameterSets);
        }

        private IEnumerable<FormTab> GetFieldsFromParamBlock(IEnumerable<CommandParameterSetInfo> parameterSets)
        {
            var commonParameters = new[]
            {
                "Debug",
                "ErrorAction",
                "ErrorVariable",
                "InformationAction",
                "InformationVariable",
                "OutVariable",
                "OutBuffer",
                "PipelineVariable",
                "Verbose",
                "WarningAction",
                "WarningVariable"
            };

            foreach(var parameterSet in parameterSets)
            {
                var formTab = new FormTab();
                formTab.Name =  parameterSet.Name == "__AllParameterSets" ? string.Empty : parameterSet.Name;

                foreach (var parameter in parameterSet.Parameters)
                {
                    if (commonParameters.Any(m => m.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    var field = new FormField();
                    field.Name = parameter.Name;
                    field.Required = parameter.IsMandatory;
                    field.DotNetType = parameter.ParameterType;

                    if (parameter.ParameterType == typeof(bool) || parameter.ParameterType == typeof(SwitchParameter))
                    {
                        field.Type = FieldTypes.Checkbox;
                    }
                    else if (parameter.ParameterType.IsEnum)
                    {
                        field.ValidOptions = Enum.GetNames(parameter.ParameterType);
                        field.Type = FieldTypes.Select;
                    }
                    else if (parameter.Attributes.Any(m => m is ValidateSetAttribute))
                    {
                        var attribute = parameter.Attributes.First(m => m is ValidateSetAttribute) as ValidateSetAttribute;

                        field.ValidOptions = attribute.ValidValues.ToArray();
                        field.Type = FieldTypes.Select;
                    }
                    else
                    {
                        field.Type = FieldTypes.Textbox;
                    }

                    formTab.FormFields.Add(field);
                }

                yield return formTab;
            }
        }
    }
}
