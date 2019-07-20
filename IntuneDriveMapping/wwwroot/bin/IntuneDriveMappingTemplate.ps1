<#
	.DESCRIPTION
		This script performs network drive mapping with PowerShell and is auto generated from a wepabb. 
	.NOTES
		Author: Nicola Suter, nicolonsky tech: https://tech.nicolonsky.ch
#>

[CmdletBinding()]
Param()

###########################################################################################
# Start transcript for logging															  #
###########################################################################################

Start-Transcript -Path $(Join-Path $env:temp "DriveMapping.log")

###########################################################################################
# Input values from generator															  #
###########################################################################################

$driveMappingJson='!INTUNEDRIVEMAPPINGJSON!'

$driveMappingConfig= $driveMappingJson | ConvertFrom-Json

###########################################################################################
# Helper function to determine a users group membership									  #
###########################################################################################

# Kudos for Tobias Renström who showed me this!
function Get-ADGroupMembership {
    param(
        [parameter(Mandatory=$true)]
        [string]$UserPrincipalName
    )
	process{

		try{
			
			$Searcher = New-Object -TypeName System.DirectoryServices.DirectorySearcher
			$Searcher.Filter = "(&(userprincipalname=$UserPrincipalName))"
			$Searcher.SearchRoot = "LDAP://$env:USERDNSDOMAIN"
        
			$Searcher.FindOne().GetDirectoryEntry().memberOf | ForEach-Object { 
            
				$PSItem.split(",")[0].replace("CN=","") 
			}
		}catch{
			
			Write-Warning "Could not determine group membership for: $UserPrincipalName"
		}
	}
}

###########################################################################################
# Get current group membership for the group filter capabilities						  #
###########################################################################################

if ($driveMappingConfig.GroupFilter){

    $groupMemberships = Get-ADGroupMembership -UserPrincipalName $(whoami -upn)
}

###########################################################################################
# Mapping network drives																  #
###########################################################################################

#Get PowerShell drives and rename properties
$psDrives = Get-PSDrive | Select-Object @{N="DriveLetter"; E={$_.Name}}, @{N="Path"; E={$_.DisplayRoot}}

#iterate through all network drive configuration entries
$driveMappingConfig.GetEnumerator() | ForEach-Object {

	#check if the drive is already connected with an identical configuration
	if ( -not ($psDrives.Path -contains $PSItem.Path -and $psDrives.DriveLetter -contains $PSItem.DriveLetter))
	{
		try{

			#check if drive exists - but with wrong config - to delete it
			if($psDrives.Path -contains $PSItem.Path -or $psDrives.DriveLetter -contains $PSItem.DriveLetter))
			{
				Get-PSDrive | Where-Object {$_.DisplayRoot -eq $PSItem.Path-or $_.Name -eq $PSItem.DriveLetter} | Remove-PSDrive -ErrorAction SilentlyContinue
			}

			 ## check itemleveltargeting for group membership
			if ($PSItem.GroupFilter -ne $null -and $groupMemberships -contains $PSItem.GroupFilter)
			{
				Write-Output "Mapping network drive $($PSItem.Path)"

				$null = New-PSDrive -PSProvider FileSystem -Name $PSItem.DriveLetter -Root $PSItem.Path -Description $PSItem.Label -Persist -Scope global -ErrorAction Stop

				(New-Object -ComObject Shell.Application).NameSpace("$($PSItem.DriveLetter):").Self.Name=$PSItem.Label

			}elseif ($PSItem.GroupFilter -eq $null) {

				Write-Output "Mapping network drive $($PSItem.Path)"

				$null = New-PSDrive -PSProvider FileSystem -Name $PSItem.DriveLetter -Root $PSItem.Path -Description $PSItem.Label -Persist -Scope global -ErrorAction Stop

				(New-Object -ComObject Shell.Application).NameSpace("$($PSItem.DriveLetter):").Self.Name=$PSItem.Label
			}    

		}catch{
			
			Write-Error $_.Exception

			#as soon as we write to the error stream IME considers this script as failed and will try it three times more

			#reset IME script execution?
			#copy script to client and try to rerun it from there until success?
			#package script inside win32app and detect drives via psdetectionscript

			#still lookin for the best solution
		}
	}else{
        
        Write-Output "Drive already exists with same DriveLetter and Path"
    }
}
###########################################################################################
# End & finish transcript																  #
###########################################################################################

Stop-transcript

###########################################################################################
# Done																					  #
###########################################################################################