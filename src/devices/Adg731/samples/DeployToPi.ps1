$localPath = "$PSScriptRoot\bin\Debug\netcoreapp3.1\linux-arm\publish"
$remotePath = "/home/pi/Work/dotnet/Iot.Device.Adg731/Adg731.Samples.BasicSample"
$remoteExecutable = "Adg731.Samples.BasicSample"


# Load WinSCP .NET assembly
Add-Type -Path "$PSScriptRoot\WinSCPnet.dll"

function FileTransferProgress {
	param($e)

	Write-Progress `
	-Activity "Uploading" -Status ("{0:P0} complete:" -f $e.OverallProgress) `
	-PercentComplete ($e.OverallProgress * 100)

	Write-Progress `
	-Id 1 -Activity $e.FileName -Status ("{0:P0} complete:" -f $e.FileProgress) `
	-PercentComplete ($e.FileProgress * 100)
}

function OutputDataReceived {
	param($e)

	Write-Host $e.Data
}

# Set up session options
$sessionOptions = New-Object WinSCP.SessionOptions -Property @{
	Protocol              = [WinSCP.Protocol]::Sftp
	HostName              = "192.168.1.36"
	UserName              = "pi"
	Password              = "raspPi36"
	SshHostKeyFingerprint = "ssh-ed25519 256 KZ8UfCfs2T0fF9P0P6Fs33fUOZuToxJzOlO4m+LS5n4="
}

$session = New-Object WinSCP.Session

try 
{
	# Will continuously report progress of transfer
	$session.add_FileTransferProgress( { FileTransferProgress($_) } )

	# Connect
	Write-Host ''
	Write-Host 'Openning connection to Raspberry Pi...'
	$session.Open($sessionOptions)

	try {
		Write-Host ''
		Write-Host 'Stopping the running project on Raspberry Pi...'
		$session.ExecuteCommand("killall "+$remoteExecutable).Check();
	}
	catch {
		# Write-Host 'We did not kill the task because it was not running.'
	}

	# Build
	Write-Host ''
	Write-Host 'Building project for ARM32...'
	Start-Process dotnet -ArgumentList 'publish -r linux-arm' -Wait -NoNewWindow -WorkingDirectory $PSScriptRoot

	# Trasnfer files
	Write-Host ''
	Write-Host 'Transferring files to Raspberry Pi...'
	$transferResult = $session.SynchronizeDirectories([WinSCP.SynchronizationMode]::Remote, $localPath+"\", $remotePath+"/", $false).Check();	
	Write-Host $transferResult

	# Set execution attributes
	Write-Host ''
	Write-Host 'Executing the project on Raspberry Pi...'
	$session.ExecuteCommand("chown pi "+$remotePath+" -R").Check();
	$session.ExecuteCommand("chmod 777 "+$remotePath+"/"+$remoteExecutable).Check();

	# Execute the project
	$session.add_OutputDataReceived( { OutputDataReceived($_) } )
	$session.ExecuteCommand($remotePath+"/"+$remoteExecutable).Check();

	#Write-Host $session.Output

	Write-Host ''
	Write-Host 'Done.'
}
finally 
{
	$session.Dispose()
}