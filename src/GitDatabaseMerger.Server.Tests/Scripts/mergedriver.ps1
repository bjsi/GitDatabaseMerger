 param (
	[Parameter(Mandatory=$true)][string]$local,
	[Parameter(Mandatory=$true)][string]$remote,
	[Parameter(Mandatory=$true)][string]$ancestor
)

$localCopy = New-TemporaryFile
$remoteCopy = New-TemporaryFile
$ancestorCopy = New-TemporaryFile

Copy-Item $local $localCopy
Copy-Item $remote $remoteCopy
Copy-Item $ancestor $ancestorCopy

write-output "$localCopy $remoteCopy $ancestorCopy"
exit 1
