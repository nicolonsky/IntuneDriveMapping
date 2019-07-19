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

$driveMappingConfig= $driveMappingJson | ConvertFrom-Json -ErrorAction Stop

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
        $Searcher = New-Object -TypeName System.DirectoryServices.DirectorySearcher
        $Searcher.Filter = "(&(userprincipalname=$UserPrincipalName))"
        $Searcher.SearchRoot = "LDAP://$env:USERDNSDOMAIN"
        
		$Searcher.FindOne().GetDirectoryEntry().memberOf | ForEach-Object { 
            
            $PSItem.split(",")[0].replace("CN=","") 
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
# DNS connection tests																	  #
###########################################################################################

## verify dns resolution to individual resources
$connected=$false
$retries=1
$maxRetries=3

# get unique dns names from all entries in the list
$dnsNames= @()

$driveMappingConfig.Path | Select-Object -Unique | ForEach-Object {

    $i=$_.lastIndexOf('\')

    $dnsNames+= $_.Substring(0,$i).Replace("\","")
}

$dnsNames = $dnsNames | Select-Object -Unique

#try resolving
$dnsNames | ForEach-Object {
    do {
        
        if (Resolve-DnsName $PSItem -ErrorAction SilentlyContinue){
        
            $connected=$true

        } else{
    
            $retries++
            
            Write-Warning "Cannot resolve: $PSItem, assuming no connection to fileserver"
    
            Start-Sleep -Seconds 3
    
            if ($retries -eq $maxRetries){
                
                Write-Error "Exceeded maximum numbers of retries ($maxRetries) to resolve dns name ($PSItem)"
                $Connected=$true
            }
        }
    
    }while( -not ($Connected))
}

###########################################################################################
# Mapping network drives																  #
###########################################################################################

#Refresh PowerShell drives (useful for testing)
$null= Get-PSDrive

$driveMappingConfig.GetEnumerator() | ForEach-Object {

    ## check itemleveltargeting for group membership
    if ($PSItem.GroupFilter -ne $null -and $groupMemberships -contains $PSItem.GroupFilter)
    {
		Write-Output "Mapping network drive $($PSItem.Path)"

		$null = New-PSDrive -PSProvider FileSystem -Name $PSItem.DriveLetter -Root $PSItem.Path -Description $PSItem.Label -Persist -Scope global

		(New-Object -ComObject Shell.Application).NameSpace("$($PSItem.DriveLetter):").Self.Name=$PSItem.Label

    }elseif ($PSItem.GroupFilter -eq $null) {

        Write-Output "Mapping network drive $($PSItem.Path)"

        $null = New-PSDrive -PSProvider FileSystem -Name $PSItem.DriveLetter -Root $PSItem.Path -Description $PSItem.Label -Persist -Scope global

        (New-Object -ComObject Shell.Application).NameSpace("$($PSItem.DriveLetter):").Self.Name=$PSItem.Label
    }     
}

###########################################################################################
# End & finish transcript																  #
###########################################################################################

Stop-transcript

###########################################################################################
# Done																					  #
###########################################################################################