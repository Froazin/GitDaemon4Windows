<#
.Synopsis
..
.Description
..
.Example
..
.Notes
..
#>

#requires -RunAsAdministrator
#requires -Version 3

param
(
    [Parameter()]
    [string]
    $StartupType = "Automatic"
)

begin {
    # determine path is valid
    if (-not (Get-ChildItem -Path $PSScriptRoot).Name.Contains("GitDaemon4Windows.exe"))
    {
        Write-Error -Message "This installer script must be run from the same directory as GitDaemon4Windows.exe."
        exit $null
    }
}
process
{
    # Check there are no existing services with a conflicing name
    if (Get-Service -Name "GitDaemon4Windows" -ErrorAction SilentlyContinue)
    {
        # check sc.exe is isntalled for removing
        if ($null -ne (Get-Command -Name sc.exe -ErrorAction SilentlyContinue).Name)
        {
            $InputString = $null
            while (-not $InputString) {
                $InputString = Read-Host -Prompt "GitDaemon4Windows service already installed. Would you like to reinstall? (y|n)"

                switch -regex ($InputString)
                {
                    # yes
                    "(?:[Yy][esES]{0,2}.*)"
                    {
                        # check if service is running
                        if ((Get-Service -Name GitDaemon4Windows -ErrorAction SilentlyContinue).Status -like "Running")
                        {
                            Write-Host "Stopping service."
                            sc.exe stop GitDaemon4Windows | Out-Null
                        }
                        Write-Host "Removing previous installation."
                        sc.exe delete GitDaemon4Windows | Out-Null

                        if (Get-Service -Name GitDaemon4Windows -ErrorAction SilentlyContinue) {
                            exit
                        }
                    }
                    # no
                    "(?:[Nn][Oo]{0,1}.*)"
                    {
                        Write-Warning -Message "Scipt aborted..."
                        exit $null
                    }
                    default
                    {
                        $InputString = $false
                    }
                }
            }
        }
        else
        {
            Write-Error -Message "sc.exe in not available, please manually remove your previous installation of GitDaemon4Windows and retry."
            exit $null
        }
    }

    $path = $PSScriptRoot.ToString() + "\GitDaemon4Windows.exe"
    # Install GitDaemon4Windows service
    Write-Host "Installing service."
    New-Service -Name "GitDaemon4Windows" -DisplayName "Git Daemon 4 Windows" -Description "A simple .Net Core Worker for Git." -BinaryPathName $path -StartupType $StartupType | Out-Null

    $wcount = 0
    $wlimit = 5

    while (-not (Get-Service -Name GitDaemon4Windows -ErrorAction SilentlyContinue))
    {
        if ($wcount -gt $wlimit)
        {
            Write-Error -Message "Installer timed out. Please try again."
            exit $null
        }
        Write-Host "Waiting..."
        $wcount++
        Start-Sleep -seconds 5
    }

    Write-Host "Installation successful."
}
end
{
    Write-Host "Starting service."
    Start-Service -Name GitDaemon4Windows

    $wcount = 0
    $wlimit = 5

    while (-not (Get-Service -Name GitDaemon4Windows -ErrorAction SilentlyContinue).Status -like "Running")
    {
        if ($wcount -gt $wlimit)
        {
            Write-Error -Message "Installer timed out. Please try again."
            exit $null
        }
        Write-Host "Waiting..."
        $wcount++
        Start-Sleep -seconds 5
    }

    Write-Host "Service started."
    Write-Host "Install completed..."

    exit 0
}