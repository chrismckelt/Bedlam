$files = get-childitem "D:\Development\Bedlam\Plugins\" -recurse | where {$_.extension -eq ".dll"} | % {
     Write-Host $_.FullName
	 Copy-Item $_.FullName "D:\Development\Bedlam\Plugins\bin"
}

$files = get-childitem "D:\Development\Bedlam\packages" -recurse | where {$_.extension -eq ".dll"} | % {
     Write-Host $_.FullName
	 Copy-Item $_.FullName "D:\Development\Bedlam\Plugins\bin"
}
