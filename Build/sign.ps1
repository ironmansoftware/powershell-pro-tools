param($Path)

$OpenAuthenticode = Import-Module OpenAuthenticode -PassThru -ErrorAction Ignore
if ($null -eq $OpenAuthenticode) {
    Install-Module OpenAuthenticode -Force -Scope CurrentUser -AllowClobber
}

$key = Get-OpenAuthenticodeAzKey -Vault ims-hms2 -Certificate Global-Sign-Cert -ErrorAction SilentlyContinue

if ($null -eq $Key) {
    Write-Warning "No signing key available. Skipping signing."
    return
}

$signParams = @{
    Key             = $key
    TimeStampServer = "http://timestamp.globalsign.com/tsa/r6advanced1"
    HashAlgorithm   = 'SHA256'
}

Set-OpenAuthenticodeSignature -FilePath $Path @signParams