




#import-module D:\threadjob\threadjob.1.1.2\ThreadJob.psd1
#install-module 

$ARCHIVECOMPLETED_Click = {
ARCHIVE
}

$BTNCounterUpdate_Click = {
	if (Test-Path "C:\vmtoolsupdater\VarsForUpradeScript\list.txt"){
		$file = "C:\vmtoolsupdater\VarsForUpradeScript\list.txt"
		(gc $file) | ? {$_.trim() -ne "" -or $_.trim() -ne "`n"} | set-content "$file" 
		$total = (gc "C:\vmtoolsupdater\VarsForUpradeScript\list.txt").count
		$total = $total -replace("`n`n","")
		$total = $total -replace(" ","")
	}
	$errcntr = (Get-ChildItem "C:\vmtoolsupdater\logs\error").count
	$ccntr = (Get-ChildItem "C:\vmtoolsupdater\logs\complete").count
	$hwcntr = (Get-ChildItem "C:\vmtoolsupdater\logs\hardwareupgrade").count
	$stcntr = (Get-ChildItem "C:\vmtoolsupdater\logs\startingvms").count
	$rdpcntr = (Get-ChildItem "C:\vmtoolsupdater\logs\startingRDPcheck").count
	$stpcntr = (Get-ChildItem "C:\vmtoolsupdater\logs\stopping").count
	$toolcntr = (Get-ChildItem "C:\vmtoolsupdater\logs\toolsupgrade").count

	$tbstart.text = $stcntr
	$tbupdatetools.text = $toolcntr
	$tbstop.text = $stpcntr
	$tbhwupgradestart.text = $hwcntr
	$tbstartrdp.text = $rdpcntr
	$tbcomplete.text = $ccntr
	$tbcompleted.text = $ccntr
	$tberror.text = $errcntr
	$tbtotals.Text = $total
	$tbrunning.Text = $r

	$tbrunning.text = [int]$hwcntr + [int]$stcntr + [int]$rdpcntr + [int]$stpcntr + [int]$toolcntr
}

$btnProcess_Click = {
$vcc = $TBVcenter.text
$user = $TBUserID.text
$password = $TBPassword.text
$computers = $textBox1.text | where {$_ -ne $null -and $_ -ne ""}
$textbox1.Text = $computers
$textBox2.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]11,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
$textBox2.Clear()
$progressbar1.Maximum = $textBox1.Lines.Count
$progressbar1.Step = 1
$progressbar1.Value = 0

	if ($GBUtilities.Enabled -eq $false){
		$VCC = $TBVcenter.Text
		$ESXVcenterUsername = $TBUserID.Text
		import-module VMware.VimAutomation.Core -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
	
		CONNECTTOVCENTER $VCC $user $password
		$esxVcenterUsername = $TBUserID.text
		$vcc = $TBVcenter.Text
		if ($RBQueries.checked -eq $true -and $RBNicinfo.checked -eq $true){
			$textBox2.Font = (New-Object -TypeName System.Drawing.Font -ArgumentList @([System.String]'Microsoft Sans Serif',[System.Single]10,[System.Drawing.FontStyle]::Regular,[System.Drawing.GraphicsUnit]::Point,([System.Byte][System.Byte]0)))
			foreach ($vm in $textBox1.Lines){
				$networkinfo = (get-vm $vm | Get-NetworkAdapter)
				foreach ($adapter in $networkinfo){
					$name = $adapter.name
					$portgroup = $adapter.NetworkName
					$mac = $adapter.MacAddress
					$result = "$vm -- NIC: $name -- PortGroup: $portgroup -- MacAddress: $mac "
					$textBox2.AppendText("$result `n`n")
					$progressbar1.PerformStep()
				}
			}
		}
		if ($RBQueries.checked -eq $true -and $BTNgetvmHWversion.checked -eq $true){
			foreach ($vm in $textBox1.lines){
				$thisvm = (get-vm $vm)
				$hwversion = $thisvm.version
				$ESXhost = $thisvm.vmhost
				$ESXhostversion = $thisvm.vmhost.Version
				$result = "$vm -- HWVer: $hwversion -- HostName: $ESXhost -- HostVer: $ESXhostversion "
				$textBox2.AppendText("$result `n`n")
				$progressbar1.PerformStep()
			}
		}
		if ($RBQueries.checked -eq $true -and $RBCurrentHost.checked -eq $true){
			foreach ($vm in $textBox1.lines){
				$thisvm = (get-vm $vm)
				$ESXhost = $thisvm.vmhost
				$ESXhostversion = $thisvm.vmhost.Version
				$result = "$vm -- CurrentHost: $ESXhost -- HostVer: $ESXhostversion "
				$textBox2.AppendText("$result `n`n")
				$progressbar1.PerformStep()
			}
		}
		if ($RBQueries.checked -eq $true -and $RBToolsver.checked -eq $true){
			foreach ($vm in $textBox1.lines){
				$thisvm = (Get-VM $vm |Get-View -Property Guest)
				$version = $thisvm.guest.toolsversion
				$status = $thisvm.guest.toolsversionstatus
				$result = "$vm -- ToolsVer: $version -- ToolsStatus: $status"
				$textBox2.AppendText("$result `n`n")
				$progressbar1.PerformStep()
			}
		}	
		if ($RBQueries.checked -eq $true -and $rbpowerstate.checked -eq $true){
			foreach ($vm in $textBox1.lines){
				$state = (get-vm -name $vm).powerstate
				$result = "$vm : $state"
				$textBox2.AppendText("$result `n`n")
				$progressbar1.PerformStep()
			}
		}

	
		if ($RBChange.checked -eq $true -and $RBStopVMOS.checked -eq $true){
			foreach ($vm in $textBox1.lines){
				$state = (get-vm $vm).powerstate
				if ($state -match "PoweredOn"){
					$textBox2.AppendText("$VM : Powered on : Powering off with guest shutdown... `n`n")
					Stop-VMGuest -vm  $vm -Confirm:$false
				}elseif($state -match "Poweredoff"){
					$textBox2.AppendText("$VM : is already powered off. `n`n")
					$progressbar1.PerformStep()
				}
			}
		}	
		if ($RBChange.checked -eq $true -and $RBStartVM.checked -eq $true){
			foreach ($vm in $textBox1.lines){
				$state = (get-vm $vm).powerstate
				if ($state -match "Poweredoff"){
					$textBox2.AppendText("$VM : Powered off : Powering up... `n`n")
					start-vm -vm  $vm -Confirm:$false
				}elseif($state -match "Poweredon"){
					$textBox2.AppendText("$VM : is already powered on. `n`n")
					$progressbar1.PerformStep()
				}
			}
		}
		if ($RBChange.checked -eq $true -and $RBSTopVMHard.checked -eq $true){
			foreach ($vm in $textBox1.lines){
				$state = (get-vm -name $vm).powerstate
				if ($state -match "poweredoff"){
					$textBox2.AppendText("$VM : is already powered off... `n`n")
					start-vm -vm  $vm -Confirm:$false
				}elseif($state -match "Poweredon"){
					$textBox2.AppendText("$VM : VM is powered on. Performing hard shutdown... `n`n")
					stop-vm -VM $vm -confirm:$false
					$progressbar1.PerformStep()
				}
			}
		}
		if ($RBChange.checked -eq $true -and $RBRebootOS.checked -eq $true){
			foreach ($vm in $textBox1.lines){
				$state = (get-vm -name $vm).powerstate
				if ($state -match "poweredoff"){
					$textBox2.AppendText("$VM : is powered off... `n`n")
					start-vm -vm  $vm -Confirm:$false
				}elseif($state -match "Poweredon"){
					$textBox2.AppendText("$VM : Rebooting guest... `n`n")
					Restart-VMGuest -VM $vm -confirm:$false
					$progressbar1.PerformStep()
				}
			}
		}
		if ($RBChange.checked -eq $true -and $RBRebootHard.checked -eq $true){
			foreach ($vm in $textBox1.lines){
				$state = (get-vm -name $vm).powerstate
				if ($state -match "poweredoff"){
					$textBox2.AppendText("$VM : is powered off... `n`n")
					start-vm -vm  $vm -Confirm:$false
				}elseif($state -match "Poweredon"){
					$textBox2.AppendText("$VM : Restarting VM with hard restart... `n`n")
					Restart-VM -VM $vm -confirm:$false
					$progressbar1.PerformStep()
				}
			}
		}
	DISCONNECTFROMVCENTER $VCC
	}

#region Multithread with Runspacepool

			
	$codecontainer = $null
	if ($RBUtilities.checked -eq $true -and $rbping.checked -eq $true){
		$codecontainer = $ping
	}elseif($RBUtilities.checked -eq $true -and $rbrdp.checked -eq $true){
		$codecontainer = $rdp
	}

if ($codecontainer -ne $null){
	$textBox2.Clear()
	$trackBar1.Value = $env:NUMBER_OF_PROCESSORS
	$tbtrackbarval.Text = $env:NUMBER_OF_PROCESSORS
	$runspacepool = [Runspacefactory]::CreateRunspacepool(1, $trackBar1.value) #min and max threads
	$runspacepool.Apartmentstate = "MTA" #thread state. Determines if STA or MTA for multithreading. 
	$runspacepool.open()
					$i=0	
					$results= @()
					$textBox2.WordWrap = $false
					$progressbar1.Maximum = $textBox1.Lines.Count
					$progressbar1.Step = 1
					$progressbar1.Value = 0
					#codeblock to execute per thread
					$threads = @() #Emtpy array to hold objects for threads you spin up.
					$stopwatch = [system.Diagnostics.Stopwatch]::StartNew()
					$count = 0
					foreach ($server in $textBox1.lines){
						IF ($server -ne "") {
							$count += 1
							$server = $server -replace " ",""
							$server = $server -replace "`r`n",'`n'
							$server = $server.trim()
							$runspaceobject = [pscustomobject] @{
							Runspace = [powershell]::Create()
							Invoker = $null #stores the status of the thread.
							}
							$runspaceobject.runspace.runspacepool = $runspacepool
							$runspaceobject.runspace.addscript($codecontainer) | Out-Null
							$runspaceobject.runspace.addargument($server) | Out-Null #or .addperameter 
							$runspaceobject.invoker = $runspaceobject.runspace.begininvoke() #this method returns an object that tells you the status of the thread. If completed or not.
							$threads += $runspaceobject
							$threadresults = @()
						}
					}
								
					$threadresults = @()
					foreach ($t in $threads){
								While ($t.Invoker.IsCompleted -contains $false){}
								If ($t.invoker.iscompleted -eq $true){
								$completed += 1
									Start-Sleep -Milliseconds 5 #Added to stop the timeout of the form
								#$label1.Text = "Servers Processed: $completed"
								#$label1.Refresh()
								}
							$feedtextbox = $t.Runspace.EndInvoke($t.Invoker) #Returns thread results when finished. Not equal $false for iscompleted.
							$t.Runspace.dispose() #dispose individual thread
							$textBox2.appendtext("$feedtextbox")
							$textbox2.appendtext("`n`n")
							$elapsed = $stopwatch.Elapsed
							$progressbar1.PerformStep()
							$t.Runspace.Dispose()
					}	
					$progressbar1.PerformStep()
					$runspacepool.close() #close pool
					$runspacepool.dispose() #dispose pool
					$elapsed = $stopwatch.Elapsed
}
#endregion Multithread with Runspacepool



}

$button2_Click = {
	$button2.BackColor = [System.Drawing.Color]::LightYellow
	$textBox2.Clear()
	$textbox1.text = $serversintextbox
	$vcc = $TBVcenter.Text
	$user = $TBUserID.text
	$password = $TBPassword.Text
	if (Test-Path "C:\vmtoolsupdater\VarsForUpradeScript\ESXVars.txt"){del "C:\vmtoolsupdater\VarsForUpradeScript\ESXVars.txt" -Force}
	write-output $user | out-file "C:\vmtoolsupdater\VarsForUpradeScript\ESXVars.txt"
	write-output $vcc | out-file "C:\vmtoolsupdater\VarsForUpradeScript\ESXVars.txt" -Append
	$textBox2.AppendText("`n`n")
	$textBox2.AppendText(" Connecting to Vcenter: $vcc `n`n")
	$lblstatus.Text = "Connecting..."
	$return = CONNECTTOVCENTER $vcc $user $password
	Start-Sleep -Seconds 2
	if ($return -match $vcc){
		$textBox2.Clear()
		$textBox2.AppendText("`n`n")
		$groupBox3.Enabled = $false
		$GBQueries.enabled = $false
        $GBChange.enabled = $false
		$GBUpgrade.enabled = $false
        $GBUtilities.enabled = $true
        $rbqueries.enabled = $true
        $rbchange.enabled = $true
        $rbutilities.enabled = $true
        $RBUpgrade.enabled = $true
        $rbqueries.checked = $false
        $rbchange.checked = $false
        $rbutilities.checked = $true
        $RBUpgrade.checked = $false
        $rbping.checked = $true
        $button2.Visible = $false
		$btnProcess.BackColor = "limegreen"
        $btnProcess.enabled = $true
		$answer = " Successfully connected to $vcc."
		$textBox2.AppendText("`n`n")
		$textBox2.AppendText(" $answer `n`n")
		$textBox2.AppendText(" `n`n")
		$textBox2.AppendText("  Utility is now ready for use...  `n`n")
		$lblstatus.Text = "Connected!"		
		Start-sleep -Milliseconds 5
		$disconnect = DISCONNECTFROMVCENTER $vcc
	}else{
		$GBQueries.enabled = $false
		$GBChange.enabled = $false
		$GBUpgrade.enabled = $false
		$answer = "Failed to connect to $vcc."
		$lblstatus.Text = " Try again..."
		$button2.Text = "Failed! Try again.."
		$button2.BackColor = [System.Drawing.Color]::"Red"
		$lblstatus.forecolor = [System.Drawing.Color]::"Red"
	
	}

}

$button3_Click = {
	$groupBox3.Enabled = $true
	$button2.Visible = $true
	$button2.Text = "Connect To Vcenter"
	$button2.BackColor = [System.Drawing.Color]::limegreen
}

$button4_Click = {
	ARCHIVE
	$istheretext = $textBox1.Text
	if ($istheretext[1]){
		if (test-path "C:\vmtoolsupdater\VarsForUpradeScript\list.txt"){del "C:\vmtoolsupdater\VarsForUpradeScript\list.txt" -Force}
		#Clean running dirs START
			$runningdirs = gc "C:\vmtoolsupdater\VarsForUpradeScript\runningdirs.txt"
			foreach ($line in $runningdirs){del "$line\*.*" -force}
		#Clean running dirs END
		$computers = $textBox1.Lines | where {$_ -ne $null -and $_ -ne ""}
		$computers | sc "C:\vmtoolsupdater\VarsForUpradeScript\list.txt"
		$UPGRADETOOLS_SCRIPTBLOCK | SC "C:\vmtoolsupdater\MultithreadFromUI.ps1"
		$jobname = "RunRemoteScriptFromUI"
		$password = $TBPassword.text
		Start-ThreadJob -name $jobname -FilePath "C:\vmtoolsupdater\MultithreadFromUI.ps1" -ArgumentList @("$password")
		$TextBox2.Clear()
		$TextBox2.appendtext(" `n`n") 
		$TextBox2.appendtext("  Running VMTools and HW update script on the VM's listed on the left. `n`n") 
		$TextBox2.appendtext("  ======================================================= `n`n") 
		$TextBox2.appendtext("  `n`n")
		$TextBox2.appendtext("  The status of each server will be indicated by the appearance of the  `n`n") 
		$TextBox2.appendtext("  SERVERNAME.LOG in each of the windows below. This will also indicate   `n`n") 
		$TextBox2.appendtext("  which step the server is currently executing. `n`n") 
		$TextBox2.appendtext("  `n`n") 
		$TextBox2.appendtext("  `n`n") 
		$TextBox2.appendtext("  If you need to abort the jobs in flight, press the !! Abort !! button.`n`n") 
		$TextBox2.appendtext("  This will stop all powershell jobs currently running.`n`n") 
	}else{
		$TextBox2.Clear()
		$TextBox2.appendtext("  `n`n")
		$TextBox2.appendtext("  Please enter servers in the textbox on the left... `n`n") 
		$TextBox2.appendtext("  =================================== `n`n") 
		$TextBox2.appendtext("  `n`n")
		
	
	
	}
}

$btnclear_Click = {
$textBox1.Clear()
}

$BtnAbort_Click = {
$TextBox2.Clear()
$TextBox2.appendtext(" `n`n") 
$TextBox2.appendtext("  ============= `n`n") 
$TextBox2.appendtext("  !! Aborting Jobs !! `n`n") 
$TextBox2.appendtext("  ============= `n`n") 
get-process -name powershell | stop-process -Force -Confirm:$false
}

$UPGRADETOOLS_SCRIPTBLOCK = {
param (
    $password
)

	########################################################################################################################
	<#
	Upgrade VMTools and VM Hardware Version

	#https://virtualfrog.wordpress.com/2017/07/24/powercli-automating-vmware-tools-and-hardware-upgrades/

	Depends on:
		* Import-module c:\VMToolsUpdater\VMTools_Hardware_Upgrade_Functions.psm1
		* get-content \\ptlpdfsnnnh001\dept\UE\DISTINFR\HPAssetMgrData\SCCM_NIC_EXTRACT\NICPerServer_6amReport.txt
		* Logdir c:\VMToolsUpdater\logs\
		* Cred files c:\VMToolsUpdater\CredFiles\
		* Host version 6.5.0 or below

	Steps:
		* Send functions to a file
		* Import functions for upgrade
		* Import NIC extract info and log it
		* Connect to Vcenter
		* Get HW version info
		* Start VM if not started
		* Perform Tools upgrade if needed
		* Shutdown VMGuest
		* Perform HW upgrade if needed
		* Start VM
		* Disconnect from VCenter

	Author: Tom Wilson
	Date: 4/16/2019

	#>
	#########################################################################################################################

	#region SCRIPTBLOCK 
	#++++++++++++++++++++++++#
	#      SCRIPTBLOCKS      #    
	#++++++++++++++++++++++++#
	$SCRIPTBLOCK = {
		PARAM(
			$VM,
			$vcc,
			$User,
            $Password
		)

	cls
		$progressBar1.PerformStep()
		##################
		# Variables      #
		##################
		$NICEXTRACTINFO = IMPORT-CSV -Path "\\ptlpdfsnnnh001\dept\UE\DISTINFR\HPAssetMgrData\SCCM_NIC_EXTRACT\NICPerServer_6amReport.txt" -Delimiter "~"
		$log = "C:\vmtoolsupdater\logs\startingvms\" + $vm + ".log"
		Write-Output "Starting job for $vm ." | out-file $log

		##################
		# Snapins        #
		##################
		import-module VMware.VimAutomation.Core -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
		Add-PSSnapIn VMware.VimAutomation.Core -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
		Import-Module C:\vmtoolsupdater\VMTools_Hardware_Upgrade_Functions.psm1 

		##################
		# Connect        #
		##################
		$ltd = logtimedate ; $entry = "Connecting to VCenter..." ; write-output "$ltd : $entry" | out-file $log 
		$return =  CONNECTTOVCENTER $vcc $user $Password ; $ltd = logtimedate; $entry = "Output from CONNECTTOVCENTER function : $return" ; write-output "$ltd : $entry" | out-file $log -Append

		##################
		# NIC Info       #
		##################
		$thisServerExtractInfo = $NICEXTRACTINFO | where {$_.hostname -match "$vm"}
		$ltd = logtimedate ; $entry = "Network information..." ; write-output "$ltd : $entry" | out-file $log -Append
		Write-Output $thisServerExtractInfo | out-file $log -Append

		##################
		# HW Info        #
		##################
		$ltd = logtimedate ; $entry = "Hardware information..." ; write-output "$ltd : $entry" | out-file $log -Append
		$hwresult = query -vm $vm ; $hwresult | select VM, HW_Host, VM_HW_Current, VM_HW_Final, HW_Upgrade, Tools_Current, Tools_Final, Tools_Upgrade | Out-File $log -Append
        
		##################
		# Start VM       #
		##################
		$return = ""
		$ltd = logtimedate ; $entry = "Powering on $vm if not already powered up..." ; write-output "$ltd : $entry" | out-file $log -Append
		$return =  POWERONVM $vm  ;  $ltd = logtimedate; $entry = "Output from POWERONVM function : $return" ; write-output "$ltd : $entry" | out-file $log -Append

		##################
		# Tools Upgrade  #
		##################
		move $log "C:\vmtoolsupdater\logs\toolsupgrade" -Force
		$log = "C:\vmtoolsupdater\logs\toolsupgrade\" + $vm + ".log"
		if ($hwresult.Tools_Final -eq "LinuxNoUpgrade"){
			$ltd = logtimedate ; $entry = "Linux server. GuestToolsUnmanaged. Skipping tools upgrade." ; write-output "$ltd : $entry" | out-file $log -Append
			$linux = "True"
		}elseif ($hwresult.tools_upgrade -eq "True"){
				$return = ""
				$ltd = logtimedate ; $entry = "Updating vmtools if needed..." ; write-output "$ltd : $entry" | out-file $log -Append
				$currenttoolsversionnumber = (get-vm -name $vm).guest.ToolsVersion
				$CurrentToolsVersion = (Get-VM $VM |Get-View -Property Guest).Guest.ToolsVersionStatus
				$ltd = logtimedate ; $entry = "Pre tools upgrade : Current Version number: $currenttoolsversionnumber " ; write-output "$ltd : $entry" | out-file $log -Append
				$ltd = logtimedate ; $entry = "Pre tools upgrade : Version Status: $CurrentToolsVersion " ; write-output "$ltd : $entry" | out-file $log -Append
			$return =  UPGRADETOOLS $vm ;  $ltd = logtimedate; $entry = "Output from UPGRADETOOLS function : $return" ; write-output "$ltd : $entry" | out-file $log -Append
				$currenttoolsversionnumber = (get-vm -name $vm).guest.ToolsVersion
				$CurrentToolsVersion = (Get-VM $VM |Get-View -Property Guest).Guest.ToolsVersionStatus
				$ltd = logtimedate ; $entry = "Post tools upgrade : Current Version number: $currenttoolsversionnumber " ; write-output "$ltd : $entry" | out-file $log -Append
				$ltd = logtimedate ; $entry = "Post tools upgrade : Version Status: $CurrentToolsVersion " ; write-output "$ltd : $entry" | out-file $log -Append
		}else{
				$currenttoolsversionnumber = (get-vm -name $vm).guest.ToolsVersion
				$CurrentToolsVersion = (Get-VM $VM |Get-View -Property Guest).Guest.ToolsVersionStatus
				$ltd = logtimedate ; $entry = "Post tools : Tools are current. Skipping Tools upgrade portion of script." ; write-output "$ltd : $entry" | out-file $log -Append
				$ltd = logtimedate ; $entry = "Post tools : Current Version number : $currenttoolsversionnumber" ; write-output "$ltd : $entry" | out-file $log -Append
				$ltd = logtimedate ; $entry = "Post tools : Version Status : $CurrentToolsVersion" ; write-output "$ltd : $entry" | out-file $log -Append
			}
	
		##################
		# Sleep          #
		##################
		if ($linux -match "True"){
			#skip if linux
		}else{
			$ltd = logtimedate ; $entry = "Sleeping 30 seconds." ; write-output "$ltd : $entry" | out-file $log -Append
			start-sleep -Seconds 30 #Make sure tools finished
					while ((Get-VM $vm |Get-View -Property Guest).Guest.GuestOperationsReady -ne "True"){
						write-host "Waiting for API to start ...waiting another 10 seconds..." -ForegroundColor Yellow
						$ltd = logtimedate ; $entry = "Waiting another 10 seconds for API to start before moving to power of guest..." ; write-output "$ltd : $entry" | out-file $log -Append
						Start-Sleep -Seconds 10
					}
		}

		##################
		# Stop VM        #
		##################
		move $log "C:\vmtoolsupdater\logs\stopping" -Force
		$log = "C:\vmtoolsupdater\logs\stopping\" + $vm + ".log"
		$return = ""
		$ltd = logtimedate ; $entry = "Powering off $vm..." ; write-output "$ltd : $entry" | out-file $log -Append
		if (((Get-VM $ThisVM |Get-View -Property Guest).Guest.ToolsVersionStatus) -match "guestToolsUnmanaged"){$linux = "True"}
		if ($linux -match "True"){
			$return = Stop-VM $vm -Confirm:$false ;  $ltd = logtimedate ; $entry = "Output from POWEROFFVM : $return" ; write-output "$ltd : $entry" | out-file $log -Append
		}else{
			$return = POWEROFFVM $vm ;  $ltd = logtimedate ; $entry = "Output from POWEROFFVM function : $return" ; write-output "$ltd : $entry" | out-file $log -Append
		}

		##################
		# HW Upgrade     #
		##################
		move $log "C:\vmtoolsupdater\logs\hardwareupgrade" -Force
		$log = "C:\vmtoolsupdater\logs\hardwareupgrade\" + $vm + ".log"
		if ($hwresult.HW_Upgrade -eq "True"){
			$return = ""
			$vm_hw_final = $hwresult.VM_HW_Final
			$ltd = logtimedate ; $entry = "Hardware Upgrade to version: vmx-" + $vm_hw_final + "..." ; write-output "$ltd : $entry" | out-file $log -Append
			$return =  UPGRADEHARDWARE -VM $VM -To_HWversion $vm_hw_final ;  $ltd = logtimedate; $entry = "Output from UPGRADEHARDWARE function : $return" ; write-output "$ltd : $entry" | out-file $log -Append #upgrade hw based on version of hos
			$thisvm = get-vm -name $vm; $hardwarever = ($thisvm | Get-View).config.version
			$ltd = logtimedate ; $entry = "Post HW : $VM is now at hardware version : $hardwarever" ; write-output "$ltd : $entry" | out-file $log -Append
		}else{
			$thisvm = get-vm -name $vm; $hardwarever = ($thisvm | Get-View).config.version
			$ltd = logtimedate ; $entry = "Hardware is current. Skipping HW upgrade portion of script." ; write-output "$ltd : $entry" | out-file $log -Append
			$ltd = logtimedate ; $entry = "Post HW : $VM current hardware version : $hardwarever" ; write-output "$ltd : $entry" | out-file $log -Append
		}

		##################
		# Sleep          #
		################## 
		$ltd = logtimedate ; $entry = "Sleeping 5 seconds after HW upgrade." ; write-output "$ltd : $entry" | out-file $log -Append
		start-sleep -Seconds 5

		##################
		# Start VM       #
		##################
		move $log "C:\vmtoolsupdater\logs\startingRDPcheck" -Force
		$log = "C:\vmtoolsupdater\logs\startingRDPcheck\" + $vm + ".log"
		$return = ""
		$ltd = logtimedate ; $entry = "Powering on $vm..." ; write-output "$ltd : $entry" | out-file $log -Append
		$return =  POWERONVM $vm  ;  $ltd = logtimedate; $entry = "Output from POWERONVM function : $return" ; write-output "$ltd : $entry" | out-file $log -Append
		
		##################
		# Sleep          #
		##################
		$ltd = logtimedate ; $entry = "Sleeping 30 seconds." ; write-output "$ltd : $entry" | out-file $log -Append
		start-sleep -Seconds 30 #Make sure tools finished
				while ((Get-VM $vm |Get-View -Property Guest).Guest.GuestOperationsReady -ne "True"){
					write-host "Waiting for API to start ...waiting another 10 seconds..." -ForegroundColor Yellow
					$ltd = logtimedate ; $entry = "Waiting another 10 seconds for API to start before moving to power of guest..." ; write-output "$ltd : $entry" | out-file $log -Append
					Start-Sleep -Seconds 10
				}

		##################
		# RDP Check      # <----------------added due to RDP port not listening for some vms. Reboot seems to clear.
		##################
		if (((Get-VM $ThisVM |Get-View -Property Guest).Guest.ToolsVersionStatus) -match "guestToolsUnmanaged"){$linux = "True"}
		if ($linux -match "True"){
			#Linux
			$ltd = logtimedate ; $entry = "Checking port 22 for RDP. If not listening, will reboot 1 final time." ; write-output "$ltd : $entry" | out-file $log -Append
			$port = "22"
			$t = New-Object Net.Sockets.TcpClient
			$t.Connect($vm,$port)
		}else{
			#Wintel
			$ltd = logtimedate ; $entry = "Checking port 3389 for RDP. If not listening, will reboot 1 final time." ; write-output "$ltd : $entry" | out-file $log -Append
			$port = "3389"
			$t = New-Object Net.Sockets.TcpClient
			$t.Connect($vm,$port)
		}
		if ($t.Connected -ne $true){
			$ltd = logtimedate ; $entry = "RDP not listening. Restarting VMGuest one final time. 60 second sleep..." ; write-output "$ltd : $entry" | out-file $log -Append
			if ($linux -match "True"){
				#Linux
				Restart-VM -VM $vm -Confirm:$false
			}else{
				#Wintel
				Restart-VMGuest -vm $vm -Confirm:$false
			}
			start-sleep -Seconds 60
			$t.Connect($vm,$port)
			if ($t.Connected -ne $true){
				$ltd = logtimedate ; $entry = "Error : RDP not listening after final reboot. Check VM!" ; write-output "$ltd : $entry" | out-file $log -Append
				}else{
				$ltd = logtimedate ; $entry = "RDP is now listening after final reboot. VM is online." ; write-output "$ltd : $entry" | out-file $log -Append   
				}
		}else{
				$ltd = logtimedate ; $entry = "RDP port was listening. VM is online." ; write-output "$ltd : $entry" | out-file $log -Append    
		}
		
		##################
		# Errors         #
		##################

		$errorcheck = gc $log
		$error = $log | where {$_ -match "not listening"}
		if ($error -ne $null -or $error -ne ""){
			#Error with RDP. Move to error folder
			move $log C:\vmtoolsupdater\logs\error
		}else{
			#No Error found. Do nothing
		}






		##################
		# Disconnect     #
		##################
		$return = ""
		$ltd = logtimedate ; $entry = "Disconnecting from Vcenter $environment..." ; write-output "$ltd : $entry" | out-file $log -Append
		$return = DISCONNECTFROMVCENTER $vcc 


		$ltd = logtimedate ; $entry = "Script complete!" ; write-output "$ltd : $entry" | out-file $log -Append
		move $log "C:\vmtoolsupdater\logs\complete" -Force
    
	}
	#endregion SCRIPTBLOCK

	#region MainScriptVars
	#++++++++++++++++++++++++#
	#          LIST          #    
	#++++++++++++++++++++++++#
	$computers = GC "C:\vmtoolsupdater\VarsForUpradeScript\list.txt" | where {$_ -ne ""} | Get-Unique
	$esxvars = gc "C:\vmtoolsupdater\VarsForUpradeScript\ESXVars.txt"
	$vcc = $esxvars[1]
	$user = $esxvars[0]
    #$password = ":ZTdp{UXf2.0t6mJ"
    import-module VMware.VimAutomation.Core -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
    Add-PSSnapIn VMware.VimAutomation.Core -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
	#endregion MainScriptVars

#region Multithread with jobs
#++++++++++++++++++++++++#
#      MULTITHREAD       #    
#++++++++++++++++++++++++#



$MasterJobLoop = 0
$MaxThreads = 10 # 10 vms per 10 minutes | 3 hours 180 vmss
$SleepTimer = 10
$MaxWaitAtEnd = 10000
$jobcount = ""
$i = 0
"Killing existing jobs . . ."
Get-Job | Remove-Job -Force
"Done."

$completedcol = @()
ForEach ($thisvm in $Computers){
    $thisvm = $thisvm | ?{$_ -ne ""} 

    While ($(Get-Job -state running).count -ge $MaxThreads){
        Start-Sleep -Milliseconds $SleepTimer
    }

    #"Starting job - $Computer"
    $i++
    Start-Job -name $thisvm -ScriptBlock $SCRIPTBLOCK -ArgumentList @("$thisvm","$vcc","$user","$password")
}

$Complete = Get-date

While ($(Get-Job -State Running).count -gt 0){
    $ComputersStillRunning = ""
    ForEach ($System  in $(Get-Job -state running)){$ComputersStillRunning += ", $($system.name)"}
    $ComputersStillRunning = $ComputersStillRunning.Substring(2)
    If ($(New-TimeSpan $Complete $(Get-Date)).totalseconds -ge $MaxWaitAtEnd){"Killing all jobs still running . . .";Get-Job -State Running | Remove-Job -Force}
	Start-Sleep -Milliseconds $SleepTimer
}

#endregion Multithread with Jobs 

}
#COPY TO FILE. WILL BE USED DURING UPGRADE MULTITHREADING

$FUNCTIONS_SCRIPTBLOCK = {
	########################################################################################################################
	<#
	Upgrade VMTools and Hardware Functions that will be loaded during Upgrade script

	Author: Tom Wilson
	Date: 2/17/2019

	#>
	#########################################################################################################################

	##################################
	# Get Date and Time for Log      #
	##################################

	FUNCTION LOGTIMEDATE {
	$return = Get-Date -Format "yyyyMMdd-HHmmss"
	return $return
	}

	###########################
	# Connect to VCenter      #
	###########################

	FUNCTION CONNECTTOVCENTER {
		param (
			$VCC,
			$user,
			$Password
		)
	import-module VMware.VimAutomation.Core -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
	Add-PSSnapIn VMware.VimAutomation.Core -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
	# Connect to Vcenter
	#$ESXVcenterPassword = Get-Content "C:\vmtoolsupdater\creds\VCenterCred.cred" | ConvertTo-SecureString
	#$ESXVCentercentercredentials = new-object -typename System.Management.Automation.PSCredential -argumentlist "$ESXVcenterUsername",$ESXVcenterPassword
	#$VCCred = New-Object System.Management.Automation.PSCredential -ArgumentList $ESXVCentercentercredentials.UserName, $ESXVCentercentercredentials.Password
    
    $ESXVcenterPassword = ConvertTo-SecureString "$Password" -AsPlainText -Force
	$Cred = New-Object System.Management.Automation.PSCredential ("$user", $ESXVcenterPassword)
	$VCS = Connect-VIServer -Server $VCC -Credential $Cred -WarningAction SilentlyContinue -ErrorAction SilentlyContinue
	return $VCS
	}

	################################
	# Disconnect from VCenter      #
	################################

	FUNCTION DISCONNECTFROMVCENTER {
		param (
			[STRING] $VCC
		)
	#Always call at end of script to make sure you disconnect the session.
	import-module VMware.VimAutomation.Core -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
	Add-PSSnapIn VMware.VimAutomation.Core -ErrorAction SilentlyContinue -WarningAction SilentlyContinue

	Disconnect-VIServer $VCC -force -Confirm:$false 
	Return $return
	}

	##################
	# Start          #
	################## 


	##################
	# Query          #
	################## 

	Function QUERY {
		Param(
			[string] $vm
		)
	$collection = @()
	
	$thisvm = get-vm -name "$vm"
	$hostversion = (get-vm $thisvm | Get-VMHost).version
	$hardwarever = ($thisvm | Get-View).config.version
	$toolsversion = ($thisvm | Get-View).guest.ToolsVersion
	$toolstype = ($thisvm | Get-View).guest.ToolsVersionStatus 
	if ($toolstype -eq "guestToolsUnmanaged" ){
		$toolsversion = "guestToolsUnmanaged"
		$linuxFlag = "True"
	}
	$hostversion = $hostversion.Replace(".","")
	$hardwarever = $hardwarever.Replace("vmx-","")

	if ([int]$hostversion -eq "600"){
		$HW_Upgrade_Path = "11"
		if ($linuxFlag -notmatch "True"){$Tools_Upgrade_Path = "10309"}else{$Tools_Upgrade_Path = "LinuxNoUpgrade"}
	}elseif ([int]$hostversion -eq "650"){
		$HW_Upgrade_Path = "13"
		if ($linuxFlag -notmatch "True"){$Tools_Upgrade_Path = "10341"}else{$Tools_Upgrade_Path = "LinuxNoUpgrade"}
	}
	
	if ($linuxFlag -notmatch "True"){
		if ([int]$toolsversion -lt [int]$Tools_Upgrade_Path){
			$toolsupgrade = "True"
		}else{$toolsupgrade = "False"}
	}else{
		#Linux - set to false
		$toolsupgrade = "False"
	}

	if ([int]$hardwarever -lt [int]$HW_Upgrade_Path){
		$HardwareUpgrade = "True"
	}else{$HardwareUpgrade = "False"}

	$collection += New-Object psobject -Property @{VM="$vm";HW_Host="$hostversion";VM_HW_Current="$hardwarever";VM_HW_Final="$HW_Upgrade_Path";HW_Upgrade="$HardwareUpgrade";Tools_Current="$toolsversion";Tools_Final="$Tools_Upgrade_Path";Tools_Upgrade="$toolsupgrade"}
	#$collection | select VM, HW_Host, VM_HW_Current, VM_HW_Final, HW_Upgrade, Tools_Current, Tools_Final, Tools_Upgrade| ft -AutoSize
	return $collection
	}

	###########################
	# Upgrade Tools           #
	###########################

	FUNCTION UPGRADETOOLS {
		param (
			[string] $ThisVM
		)
	import-module VMware.VimAutomation.Core -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
	Add-PSSnapIn VMware.VimAutomation.Core -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
		$CurrentToolsVersion = (Get-VM $ThisVM |Get-View -Property Guest).Guest.ToolsVersionStatus
		if ($CurrentToolsVersion -eq "guestToolsNeedUpgrade") {
       
			#Make sure VM is powered on
			$CurrentStatus = (Get-VM $thisvm).powerstate 
			if ($CurrentStatus -ne "PoweredOn") {
				#Below line powers on
				$StartStatus = Start-VM -vm $ThisVM -Verbose:$false
				write-host "$ThisVM is not powered on. Powering up..." -ForegroundColor Yellow
				Start-Sleep -Seconds 30
				#If still not powered up after 30 seconds below will trigger a check..
						While ((Get-VM $ThisVM |Get-View -Property Guest).Guest.ToolsRunningStatus -ne "guestToolsRunning") {
							write-host "Powering up $thisvm is taking time. Waiting another 10 seconds..." -ForegroundColor Yellow
							Start-Sleep -Seconds 10
						}
			}
        
			#Check to make sure tools are running BEFORE upgrade
			While ((Get-VM $ThisVM |Get-View -Property Guest).Guest.ToolsRunningStatus -ne "guestToolsRunning"){
				write-host "Waiting for Tools to Start...waiting another 10 seconds..."  -ForegroundColor Yellow
				Start-Sleep -Seconds 10	
			}
    
			#Make sure API has started on guest
			while ((Get-VM $ThisVM |Get-View -Property Guest).Guest.GuestOperationsReady -ne "True"){
				write-host "Waiting for API to start ...waiting another 10 seconds..." -ForegroundColor Yellow
				Start-Sleep -Seconds 10
			}

     		##### UPDATE VMTOOLS #####
			write-host "Initiating update-tools for $thisvm" 
			$return = Update-Tools  -VM $ThisVM 

			#Make sure API has started on guest
			while ((Get-VM $ThisVM |Get-View -Property Guest).Guest.GuestOperationsReady -ne "True"){
				write-host "Waiting for API to start ...waiting another 10 seconds..." -ForegroundColor Yellow  
				Start-Sleep -Seconds 10
			}
			 write-host "Tools update is complete." -ForegroundColor Green
			 $statusoftools = "Tools update is complete." 
		} else {

		   write-host "VMtools are current."  -ForegroundColor Green
		   $statusoftools = "VMtools are current."
		}
	 if ($return -eq $null -or $return -eq ""){$return = $statusoftools}else{$return = $return}
	 return $return
	 }

	#######################
	# Upgrade HW          #
	#######################

	FUNCTION UPGRADEHARDWARE {
		param (                      #REM for testing a single VM
			$vm, 
			$To_HWversion
		)   
	#Before Snapshots
	#$adaptersBefore = get-vm -name "$vm" | Get-NetworkAdapter #Send to outfile locally to script server

	#Upgrade HW
		IF ([int]$To_HWversion -eq "13"){
			$vmview = Get-VM $vm | Get-View
			$v13 = "vmx-13"
			$result = Get-View ($vmView.UpgradeVM_Task($v13)) 
		}elseif([int]$To_HWversion -eq "11"){
			$vmview = Get-VM $vm | Get-View
			$v11 = "vmx-11"
			$result = Get-View ($vmView.UpgradeVM_Task($v11))
		}
	Return $result
	}

	#####################
	# Power on VM - NEW # <----------------New
	#####################

	FUNCTION POWERONVM {
		param (
			[string] $vm
		)
		  $vmview = get-VM $vm | Get-View
		  $getvm = Get-VM $vm
		  $powerstate = $getvm.PowerState
		  $toolsstatus = $vmview.Guest.ToolsStatus
	   if ($powerstate -ne "PoweredOn"){
		   Start-VM -VM $vm -Confirm:$false -RunAsync | Out-Null
		   Write-Host "$vm is starting!" -ForegroundColor Yellow
		   sleep 10
	 ` }

	   do {
		  $vmview = get-VM $vm | Get-View
		  $getvm = Get-VM $vm
		  $powerstate = $getvm.PowerState
		  $toolsstatus = $vmview.Guest.ToolsStatus
 
		  Write-Host "$vm is starting, powerstate is $powerstate and toolsstatus is $toolsstatus!" -ForegroundColor Yellow
		  sleep 5
		  #NOTE that if the tools in the VM get the state toolsNotRunning this loop will never end. There needs to be a timekeeper variable to make sure the loop ends
 
		}until(($powerstate -match "PoweredOn") -and (($toolsstatus -match "toolsOld") -or ($toolsstatus -match "toolsOk") -or ($toolsstatus -match "toolsNotInstalled")))
 
		if (($toolsstatus -match "toolsOk") -or ($toolsstatus -match "toolsOld")){
		   $Startup = "OK"
		   Write-Host "$vm is started and has ToolsStatus $toolsstatus"
		}
		return $Startup
	}

	######################
	# Power off VM - NEW # <---------------New
	######################

	FUNCTION POWEROFFVM {
		param (
			[string] $vm
		)
	   Shutdown-VMGuest -VM $vm -Confirm:$false | Out-Null
	   Write-Host "$vm is stopping!" -ForegroundColor Yellow
	   sleep 10
 
	   do {
		  $vmview = Get-VM $vm | Get-View
		  $getvm = Get-VM $vm
		  $powerstate = $getvm.PowerState
		  $toolsstatus = $vmview.Guest.ToolsStatus
 
		  Write-Host "$vm is stopping with powerstate $powerstate and toolsStatus $toolsstatus!" -ForegroundColor Yellow
		  sleep 5
 
	   }until($powerstate -match "PoweredOff")
 
	   if (($powerstate -match "PoweredOff") -and (($toolsstatus -match "toolsNotRunning") -or ($toolsstatus -match "toolsNotInstalled"))){
		  $Shutdown = "OK"
		  Write-Host "$vm is powered-off"
	   }
	   return $Shutdown
	}

	#######################
	# Thread Flags        # <---------------New
	#######################

	FUNCTION THREADFLAG {
 		foreach ($logpath in gc "C:\vmtoolsupdater\LogPaths.txt"){
			$logpath = $logpath.Trim() 
			IF (!(TEST-PATH $LOGPATH)){MKDIR $LOGPATH -Force}
		}
}
	
	#######################
	# Archive logs        # <---------------New
	#######################

	FUNCTION ARCHIVE {
		$td =  GET-DATE -Format "dd_MM_yyy_HH_mm_ss"
		$files = Get-ChildItem -path "C:\vmtoolsupdater\logs\complete" -File -recurse 
		Foreach ($file in $files){ $filename = $file.name + "_ArchivedOn_" + $td + ".log" ; REN $file.PSPATH $filename}
		get-childitem -path "C:\vmtoolsupdater\logs\complete" -File -recurse  |  move-item -destination "C:\vmtoolsupdater\logs\archive"
	}
} #End Functions

$td =  GET-DATE -Format "dd_MM_yyy_HH_mm_ss"

$trackBar1_Scroll = {
	$tbtrackbarval.Text = $trackBar1.value
}

$RBUpgrade_CheckedChanged = {
	$webBrowser1.Refresh()
	$webBrowser2.Refresh()
	$webBrowser3.Refresh()
	$webBrowser4.Refresh()
	$webBrowser5.Refresh()
	$webBrowser6.Refresh()
	$webBrowser7.Refresh()
	$btnProcess.visible = $false
	$button4.BackColor = "limegreen"
	$BTNCounterUpdate.BackColor = "limegreen"
	$ARCHIVECOMPLETED.BackColor = "limegreen"
	$TextBox2.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]804,[System.Int32]261))
	$TextBox2.Clear()
	$TextBox2.appendtext(" `n`n") 
	$TextBox2.appendtext("  !!!!! W A R N I N G !!!!! `n`n") 
	$TextBox2.appendtext("   `n`n") 
	$TextBox2.appendtext("  These options are disrupting. `n`n") 
	$TextBox2.appendtext("  Make sure your serverlist pasted in the 'Enter Servers' textbox are correct! `n`n") 
	$TextBox2.appendtext("  The automation task selected will be caried out for each server. `n`n") 
	$TextBox2.appendtext("  As the automation runs against the supplied serverlist the servers will move through `n`n")
	$TextBox2.appendtext("  the workflow windows below. When the servers have moved to the 'Completed' window, you `n`n") 
	$TextBox2.appendtext("  can click on the logfile created to check the return status of each job. `n`n")
	$TextBox2.appendtext("  When a new group of servers is added to the 'Enter Servers' box and ran through the automation,  `n`n")
	$TextBox2.appendtext("  the 'Completed' items will be moved to the archive window. `n`n")
	$GBThreadcounter.visible = $false
	$trackBar1.Visible = $false
	$GBQueries.Enabled = $false
	$GBChange.Enabled = $false
	$GBUtilities.Enabled = $false
	$GBUpgrade.Enabled = $true
	$GBThreads.visible = $true
	get-childitem -path "C:\vmtoolsupdater\logs\complete" -File -recurse |where {$_ -notmatch "waitingforthreads"} |  move-item -destination "C:\vmtoolsupdater\logs\archive" -Force
}

$RBQueries_CheckedChanged = {
	$button4.BackColor = "control"
	$BTNCounterUpdate.BackColor = "control"
	$ARCHIVECOMPLETED.BackColor = "control"
	$btnProcess.visible = $true
	$TextBox2.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]804,[System.Int32]815))
	$TextBox2.Clear() 
	$TextBox2.appendtext("   `n`n") 
	$TextBox2.appendtext("  These options are NON-disrupting. `n`n") 
	$GBThreadcounter.visible = $false
	$trackBar1.Visible = $false
	$GBQueries.Enabled = $true
	$GBChange.Enabled = $false
	$GBUtilities.Enabled = $false
	$GBUpgrade.Enabled = $false
	$GBThreads.visible = $false
}

$RBUtilities_CheckedChanged = {
	$button4.BackColor = "control"
	$BTNCounterUpdate.BackColor = "control"
	$ARCHIVECOMPLETED.BackColor = "control"
	$btnProcess.visible = $true
	$TextBox2.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]804,[System.Int32]815))
	$TextBox2.Clear() 
	$TextBox2.appendtext("   `n`n") 
	$TextBox2.appendtext("  These options are NON-disrupting. `n`n") 
	$GBThreadcounter.visible = $true
	$trackBar1.Visible = $true
	$GBQueries.Enabled = $false
	$GBChange.Enabled = $false
	$GBUtilities.Enabled = $true
	$GBUpgrade.Enabled = $false
	$GBThreads.visible = $false

}

$RBChange_CheckedChanged = {
	$button4.BackColor = "control"
	$BTNCounterUpdate.BackColor = "control"
	$ARCHIVECOMPLETED.BackColor = "control"
	$btnProcess.visible = $true
	$TextBox2.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]804,[System.Int32]815))
	$TextBox2.Clear()
	$TextBox2.appendtext(" `n`n") 
	$TextBox2.appendtext("  !!!!! W A R N I N G !!!!! `n`n") 
	$TextBox2.appendtext("   `n`n") 
	$TextBox2.appendtext("  These options are disrupting. `n`n") 
	$TextBox2.appendtext("  Make sure your serverlist pasted in the 'Enter Servers' textbox are correct! `n`n") 
	$TextBox2.appendtext("  The automation task selected will be caried out for each server. `n`n") 
	$TextBox2.appendtext("  When complete, you can utilize the 'Queries' options to validate the state of each VM.  `n`n") 
	$TextBox2.appendtext(" `n`n") 
	$GBThreadcounter.visible = $false
	$trackBar1.Visible = $false
	$GBQueries.Enabled = $false
	$GBChange.Enabled = $true
	$GBUtilities.Enabled = $false
	$GBUpgrade.Enabled = $false
	$GBThreads.visible = $false
}

$IntelHostingVMwareUtilityForm_Load = {
	$button2.Text = "Connect To Vcenter"
	$RBQueries.enabled = $false
	$RBChange.enabled = $false
	$RBUpgrade.enabled = $false
	$GBQueries.enabled = $false
	$GBChange.enabled = $false
	$GBUpgrade.enabled = $false
	$GBUtilities.Enabled = $true
	$RBUtilities.checked = $true
	$rbping.Checked = $true
	# Clean up files and set up env vars START
		foreach ($file in get-childitem -file "C:\vmtoolsupdater\VarsForUpradeScript"){del $file.pspath -Force}
		$TextBox2.Size = (New-Object -TypeName System.Drawing.Size -ArgumentList @([System.Int32]804,[System.Int32]815))
		#Send functions to a PSM1 file and load from each thread.
		if (Test-Path "C:\vmtoolsupdater\VMTools_Hardware_Upgrade_Functions.psm1"){del "C:\vmtoolsupdater\VMTools_Hardware_Upgrade_Functions.psm1" -Force}
		$FUNCTIONS_SCRIPTBLOCK | sc "C:\vmtoolsupdater\VMTools_Hardware_Upgrade_Functions.psm1" -Force
		if (Test-Path "C:\vmtoolsupdater\UpgradeVMtools_Hardware.ps1"){del "C:\vmtoolsupdater\UpgradeVMtools_Hardware.ps1" -Force}
		$toolsupgradeScript | sc "C:\vmtoolsupdater\UpgradeVMtools_Hardware.ps1" -Force
		import-module VMware.VimAutomation.Core -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
		if (Test-Path C:\vmtoolsupdater\LogPaths.txt){del C:\vmtoolsupdater\LogPaths.txt -Force}
		if (Test-Path C:\vmtoolsupdater\VarsForUpradeScript\runningdirs.txt){del C:\vmtoolsupdater\VarsForUpradeScript\runningdirs.txt -Force}
	# Clean up files and set up env vars END
	Add-PSSnapIn VMware.VimAutomation.Core -ErrorAction SilentlyContinue -WarningAction SilentlyContinue
	Import-Module C:\vmtoolsupdater\VMTools_Hardware_Upgrade_Functions.psm1
	import-module C:\Windows\System32\WindowsPowerShell\v1.0\Modules\threadjob\threadjob.1.1.2\ThreadJob.psd1
	# Set logdirs START
		Write-output "C:\vmtoolsupdater\VarsForUpradeScript" | Out-File "C:\vmtoolsupdater\LogPaths.txt" 
		Write-output "C:\vmtoolsupdater\logs\hardwareupgrade" | Out-File "C:\vmtoolsupdater\LogPaths.txt" -Append
		Write-output "C:\vmtoolsupdater\logs\startingRDPcheck" | Out-File "C:\vmtoolsupdater\LogPaths.txt" -Append
		Write-output "C:\vmtoolsupdater\logs\startingvms" | Out-File "C:\vmtoolsupdater\LogPaths.txt" -Append
		Write-output "C:\vmtoolsupdater\logs\stopping" | Out-File "C:\vmtoolsupdater\LogPaths.txt" -Append
		Write-output "C:\vmtoolsupdater\logs\toolsupgrade" | Out-File "C:\vmtoolsupdater\LogPaths.txt" -Append
		Write-output "C:\vmtoolsupdater\logs\complete" | Out-File "C:\vmtoolsupdater\LogPaths.txt" -Append
		Write-output "C:\vmtoolsupdater\logs\archive" | Out-File "C:\vmtoolsupdater\LogPaths.txt" -Append
		Write-output "C:\vmtoolsupdater\logs\error" | Out-File "C:\vmtoolsupdater\LogPaths.txt" -Append
		THREADFLAG
		Write-Output "C:\vmtoolsupdater\logs\startingvms" | out-file "C:\vmtoolsupdater\VarsForUpradeScript\runningdirs.txt"
		Write-output "C:\vmtoolsupdater\logs\toolsupgrade" | out-file "C:\vmtoolsupdater\VarsForUpradeScript\runningdirs.txt" -append
		Write-output "C:\vmtoolsupdater\logs\stopping" | out-file "C:\vmtoolsupdater\VarsForUpradeScript\runningdirs.txt" -append
		Write-output "C:\vmtoolsupdater\logs\hardwareupgrade" | out-file "C:\vmtoolsupdater\VarsForUpradeScript\runningdirs.txt" -append
		Write-output "C:\vmtoolsupdater\logs\startingRDPcheck" | out-file "C:\vmtoolsupdater\VarsForUpradeScript\runningdirs.txt" -append
		$runningdirs = gc "C:\vmtoolsupdater\VarsForUpradeScript\runningdirs.txt"
		foreach ($line in $runningdirs){del "$line\*.*" -force}
	# Set logdirs END
	# Set running dirs START
		
	# Set running dirs END
	$trackBar1.Minimum = 1
	$trackBar1.Maximum = $env:NUMBER_OF_PROCESSORS
	$tbtrackbarval.Text = $trackBar1.value


#Region Banner
	$TextBox2.appendtext(" `n`n") 
	$TextBox2.appendtext("  Intel Hosting VMWare Utility: `n`n") 
	$TextBox2.appendtext("  ============================= `n`n") 
	$TextBox2.appendtext(" `n`n") 
	$TextBox2.appendtext("  In order to use this utility, you will need to connect to a VCenter with `n`n") 
	$TextBox2.appendtext("  a valid credential.  `n`n") 
	$TextBox2.appendtext(" `n`n") 
	$TextBox2.appendtext("	* Enter your VCenter servername in the window on the left `n`n") 
	$TextBox2.appendtext("	* Enter your UserId and Password in this format Domain\UserId `n`n") 
	$TextBox2.appendtext("	* Press 'Connect to Vcenter' `n`n") 
	$TextBox2.appendtext("	* The credential will be stored and tested against the VCenter server. `n`n") 
	$TextBox2.appendtext("	* If you fail to connect to a VCenter: `n`n") 
	$TextBox2.appendtext("		* Check spelling of VCenter servername. `n`n") 
	$TextBox2.appendtext("		* Ensure account is not locked out. `n`n") 
	$TextBox2.appendtext("		* Ensure you have a current password. `n`n") 
	$TextBox2.appendtext("	 `n`n") 
	$TextBox2.appendtext("	!NOTE! You will only be connecting to one VCenter at a time. Please `n`n") 
	$TextBox2.appendtext("	Make sure your servers are housed in the VCenter you are connected to. `n`n") 
	$TextBox2.appendtext("	 `n`n") 
	$TextBox2.appendtext("	The Thread counter on the left can be adjusted to initiate multithreading. `n`n") 
	$TextBox2.appendtext("	The counter is specific to each machine at runtime and will only allow max threads based `n`n") 
	$TextBox2.appendtext("	on the number of logical processors of the host machine. `n`n") 
	$TextBox2.appendtext("	The current host machine has $env:NUMBER_OF_PROCESSORS logical processors and will allow $env:NUMBER_OF_PROCESSORS threads   `n`n") 
	$TextBox2.appendtext("	maximum. `n`n") 

#Endregion Banner
	
}

$ping = {
	Param(
		[string] $computername
	)
	$result = invoke-command -ScriptBlock {ping.exe -n 1 $computername | Select-String "Reply", "not", "out"}
	return $computername + " : $result"
}

$rdp = {
	Param(
		[string] $computername
	)
	$result = invoke-command -ScriptBlock {portqry.exe -n $computername -e 3389 | where {$_ -match "Port" -or $_ -match "Failed"}}
	return $computername + " : $result"
}



. (Join-Path $PSScriptRoot 'designer.ps1')

$IntelHostingVMwareUtilityForm.ShowDialog()