param([String]$targetDir)

$filename = "uninstall_info.txt"
$filepath = [System.String]::Format("{0}\{1}", "$targetDir", "$filename")
$Files = Get-ChildItem "$targetDir" -Name -Exclude "settings", "error.txt", "*.vshost.*" -Recurse -File

$Files += $filename

$file = New-Object System.IO.StreamWriter($filepath, $false, [System.Text.Encoding]::GetEncoding("utf-8"))
foreach($line in $Files) {
	if($line.StartsWith("settings\")) {
		continue
	}
	if($line -eq "error.txt") {
		continue
	}
	if($line.Contains(".vshost.")) {
		continue
	}
	$file.WriteLine($line)
}
$file.Close()
