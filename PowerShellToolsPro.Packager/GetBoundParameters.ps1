function Get-BoundParameter {
	param($commandAst, $proxyCommand)

	if ($proxyCommand) {
		Invoke-Expression $proxyCommand
	}
	

	$psuedoParameterBinderType = [System.Management.Automation.AliasInfo].Assembly.GetType('System.Management.Automation.Language.PseudoParameterBinder')
	$bindingTypeType = [System.Management.Automation.AliasInfo].Assembly.GetType('System.Management.Automation.Language.PseudoParameterBinder+BindingType')
	$psuedoParameterBinder = [Activator]::CreateInstance($psuedoParameterBinderType)
	$DoPseudoParameterBinding = $psuedoParameterBinderType.GetMethod('DoPseudoParameterBinding', [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic)
	$pseudoBindingInfoType = [System.Management.Automation.AliasInfo].Assembly.GetType('System.Management.Automation.Language.PseudoBindingInfo')

	$bindingInfo = $DoPseudoParameterBinding.Invoke($psuedoParameterBinder, @($commandAst, $null, $null, $bindingTypeType.GetEnumValues()[0]))

	$boundArgumentsProperty = $pseudoBindingInfoType.GetProperty("BoundArguments", [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic)
	$boundArgumentsProperty.GetValue($bindingInfo).GetEnumerator()  | % { @{ Name = $_.Key; Ast = $_.Value.Argument }  } 
}