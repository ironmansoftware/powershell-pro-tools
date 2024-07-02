get-process

dir 

function TestFunc()
{
get-process
[IO.File]::Create("Test")
	$ScriptBlock = { 
		dir | remove-item

		gps | Stop-Process
	}
}
