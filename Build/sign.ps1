param($Path)

$signingCode = $env:SIGNING_CODE
if ([string]::IsNullOrWhiteSpace($signingCode) -or $signingCode -eq 'false' -or $signingCode -eq '0') {
    Write-Warning "Code signing is disabled. Skipping signing."
    return
}

$OpenAuthenticode = Import-Module OpenAuthenticode -PassThru -ErrorAction Ignore
if ($null -eq $OpenAuthenticode) {
    Install-Module OpenAuthenticode -Force -Scope CurrentUser -AllowClobber
}

$key = Get-OpenAuthenticodeAzKey -Vault ims-hms2 -Certificate Global-Sign-Cert -ErrorAction SilentlyContinue

if ($null -eq $Key) {
    throw "Code signing was requested, but no signing key is available."
}

$signParams = @{
    Key             = $key
    TimeStampServer = "http://timestamp.globalsign.com/tsa/r6advanced1"
    HashAlgorithm   = 'SHA256'
}

Set-OpenAuthenticodeSignature -FilePath $Path @signParams