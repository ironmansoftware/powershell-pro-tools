# $itemname$ Service Entry Point

<#
	This function is called when the service is started. Once this function returns, 
	your service will be set to a Running state.
#>
function OnStart() {

}

<#
	This function is called when the service is started. Once this function returns,
	your service will be set to a Stoppe state and the process will terminate.
#>
function OnStop() {

}

# Specifies whether this service can be stopped once started
$CanStop = $true