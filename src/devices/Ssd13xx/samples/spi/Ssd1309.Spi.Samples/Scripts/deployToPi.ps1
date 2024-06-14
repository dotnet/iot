# Licensed to the .NET Foundation under one or more agreements.
# The .NET Foundation licenses this file to you under the MIT license.

# Uses SCP to copy local output files to a RaspberryPi on the same network
# This assumes development on a Windows machine. For Linux, consider rsync instead.
# Replace the variables below with your desired values:

$PiUser = "pi"
$PiHostnameOrIp = "pi"
$TargetRuntime = "linux-arm64"
$PublishDirectory = "C:\ssd1309Sample"
$RemoteDirectory = "/home/${PiUser}/ssd1309sample"

# Publish
dotnet publish "../Ssd1309.Spi.Samples.csproj" --self-contained -c Debug -o $PublishDirectory -r $TargetRuntime

# Deploy
if ($?)
{
    Write-Host "Deploying files from ${PublishDirectory} to ${PiUser}@${PiHostnameOrIp}"
    scp -r $PublishDirectory ${PiUser}@${PiHostnameOrIp}:${RemoteDirectory}
}
else 
{
    Write-Host "Publish not successful. Skipping deployment."
}
