param(
	$file = ".",
	$type = "7z",
	$compressionLevel = 5,
	[switch]$high
)

if($high) {
	$type = "7z"
	$compressionLevel = 9
}

$displayName = (get-item $file).Name

set-alias sevenzip "C:\Program Files\7-Zip\7z.exe"

$timestamp = Get-Date -format "yyyyMMdd_HHmmss"

$command = "sevenzip a -mx=9 -x!""*_archive_????????_??????.zip"" -x!""*_archive_????????_??????.7z"" -x!"".gitattributes"" -x!"".gitignore"" -xr!""bin"" -xr!""obj"" -xr!"".git"" -xr!"".vs"" -xr!""files"" ""$($displayName)_archive_$($timestamp).$($type)"" ""$($file)"""
$command
Invoke-Expression $command
