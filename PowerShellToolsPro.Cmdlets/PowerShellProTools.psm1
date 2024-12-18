New-Alias -Name 'pad' -Value 'Show-PSScriptPad' -Force

$Platform = 'windows'
if ($IsLinux) {
    $Platform = 'linux'
}
elseif ($IsMacOS) {
    $Platform = 'mac'
}

$Runtime = "net472"
if ($IsCoreCLR) {
    if ($PSVersionTable.PSVersion.Major -eq 7 -and $PSVersionTable.PSVersion.Minor -ge 1) {
        $Runtime = 'net5.0'
    } 
    elseif ($PSVersionTable.PSVersion.Major -eq 7 -and $PSVersionTable.PSVersion.Minor -eq 0) {
        $Runtime = 'netcoreapp3.1'
    } 
}

function Expand-Object {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory, ValueFromPipeline)]
        [object]$InputObject
    )

    Process {
        if ($InputObject -eq $null) {
            return
        }
        $InputObject | Get-Member -MemberType Properties | ForEach-Object {
            try {
                $Value = $InputObject.($_.Name)
                $Node = [Terminal.Gui.Trees.TreeNode]::new("$($_.Name) = $Value")

                if ($Value -ne $null) {
                    $Children = Expand-Object -InputObject $Value
                    foreach ($child in $Children) {
                        $Node.Children.Add($child)
                    }
                }

                $Node
            }
            catch {
                Write-Host $_
            }
        }

    }
}

# SIG # Begin signature block
# DWt4BcXyh6cRKmeE4SEcAhQqDx5YUf86vl5y2e53MO1yQvQ8JRgPMjAyNDEyMTAx
# NzUxMjlaMAMCAQECCGoCYc/0LFNsoGGkXzBdMQswCQYDVQQGEwJCRTEZMBcGA1UE
# CgwQR2xvYmFsU2lnbiBudi1zYTEzMDEGA1UEAwwqR2xvYmFsc2lnbiBUU0EgZm9y
# IENvZGVTaWduMSAtIFI2IC0gMjAyMzExoIISVDCCBYMwggNroAMCAQICDkXmuwOD
# M8OFZUjm/0VRMA0GCSqGSIb3DQEBDAUAMEwxIDAeBgNVBAsTF0dsb2JhbFNpZ24g
# Um9vdCBDQSAtIFI2MRMwEQYDVQQKEwpHbG9iYWxTaWduMRMwEQYDVQQDEwpHbG9i
# YWxTaWduMB4XDTE0MTIxMDAwMDAwMFoXDTM0MTIxMDAwMDAwMFowTDEgMB4GA1UE
# CxMXR2xvYmFsU2lnbiBSb290IENBIC0gUjYxEzARBgNVBAoTCkdsb2JhbFNpZ24x
# EzARBgNVBAMTCkdsb2JhbFNpZ24wggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIK
# AoICAQCVB+hzymb57BTKezz3DQjxtEULLIK0SMbrWzyug7hBkjMUpG9/6SrMxrCI
# a8W2idHGsv8UzlEUIexK3RtaxtaH7k06FQbtZGYLkoDKRN5zlE7zp4l/T3hjCMgS
# UG1CZi9NuXkoTVIaihqAtxmBDn7EirxkTCEcQ2jXPTyKxbJm1ZCatzEGxb7ibTIG
# ph75ueuqo7i/voJjUNDwGInf5A959eqiHyrScC5757yTu21T4kh8jBAHOP9msndh
# fuDqjDyqtKT285VKEgdt/Yyyic/QoGF3yFh0sNQjOvddOsqi250J3l1ELZDxgc1X
# kvp+vFAEYzTfa5MYvms2sjnkrCQ2t/DvthwTV5O23rL44oW3c6K4NapF8uCdNqFv
# VIrxclZuLojFUUJEFZTuo8U4lptOTloLR/MGNkl3MLxxN+Wm7CEIdfzmYRY/d9XZ
# kZeECmzUAk10wBTt/Tn7g/JeFKEEsAvp/u6P4W4LsgizYWYJarEGOmWWWcDwNf3J
# 2iiNGhGHcIEKqJp1HZ46hgUAntuA1iX53AWeJ1lMdjlb6vmlodiDD9H/3zAR+YXP
# M0j1ym1kFCx6WE/TSwhJxZVkGmMOeT31s4zKWK2cQkV5bg6HGVxUsWW2v4yb3BPp
# DW+4LtxnbsmLEbWEFIoAGXCDeZGXkdQaJ783HjIH2BRjPChMrwIDAQABo2MwYTAO
# BgNVHQ8BAf8EBAMCAQYwDwYDVR0TAQH/BAUwAwEB/zAdBgNVHQ4EFgQUrmwFo5MT
# 4qLn4tcc1sfwf8hnU6AwHwYDVR0jBBgwFoAUrmwFo5MT4qLn4tcc1sfwf8hnU6Aw
# DQYJKoZIhvcNAQEMBQADggIBAIMl7ejR/ZVSzZ7ABKCRaeZc0ITe3K2iT+hHeNZl
# mKlbqDyHfAKK0W63FnPmX8BUmNV0vsHN4hGRrSMYPd3hckSWtJVewHuOmXgWQxNW
# V7Oiszu1d9xAcqyj65s1PrEIIaHnxEM3eTK+teecLEy8QymZjjDTrCHg4x362Acz
# dlQAIiq5TSAucGja5VP8g1zTnfL/RAxEZvLS471GABptArolXY2hMVHdVEYcTduZ
# lu8aHARcphXveOB5/l3bPqpMVf2aFalv4ab733Aw6cPuQkbtwpMFifp9Y3s/0HGB
# fADomK4OeDTDJfuvCp8ga907E48SjOJBGkh6c6B3ace2XH+CyB7+WBsoK6hsrV5t
# wAXSe7frgP4lN/4Cm2isQl3D7vXM3PBQddI2aZzmewTfbgZptt4KCUhZh+t7FGB6
# ZKppQ++Rx0zsGN1s71MtjJnhXvJyPs9UyL1n7KQPTEX/07kwIwdMjxC/hpbZmVq0
# mVccpMy7FYlTuiwFD+TEnhmxGDTVTJ267fcfrySVBHioA7vugeXaX3yLSqGQdCWn
# sz5LyCxWvcfI7zjiXJLwefechLp0LWEBIH5+0fJPB1lfiy1DUutGDJTh9WZHeXfV
# VFsfrSQ3y0VaTqBESMjYsJnFFYQJ9tZJScBluOYacW6gqPGC6EU+bNYC1wpngwVa
# yaQQMIIGWTCCBEGgAwIBAgINAewckkDe/S5AXXxHdDANBgkqhkiG9w0BAQwFADBM
# MSAwHgYDVQQLExdHbG9iYWxTaWduIFJvb3QgQ0EgLSBSNjETMBEGA1UEChMKR2xv
# YmFsU2lnbjETMBEGA1UEAxMKR2xvYmFsU2lnbjAeFw0xODA2MjAwMDAwMDBaFw0z
# NDEyMTAwMDAwMDBaMFsxCzAJBgNVBAYTAkJFMRkwFwYDVQQKExBHbG9iYWxTaWdu
# IG52LXNhMTEwLwYDVQQDEyhHbG9iYWxTaWduIFRpbWVzdGFtcGluZyBDQSAtIFNI
# QTM4NCAtIEc0MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEA8ALiMCP6
# 4BvhmnSzr3WDX6lHUsdhOmN8OSN5bXT8MeR0EhmW+s4nYluuB4on7lejxDXtszTH
# rMMM64BmbdEoSsEsu7lw8nKujPeZWl12rr9EqHxBJI6PusVP/zZBq6ct/XhOQ4j+
# kxkX2e4xz7yKO25qxIjw7pf23PMYoEuZHA6HpybhiMmg5ZninvScTD9dW+y279Jl
# z0ULVD2xVFMHi5luuFSZiqgxkjvyen38DljfgWrhsGweZYIq1CHHlP5CljvxC7F/
# f0aYDoc9emXr0VapLr37WD21hfpTmU1bdO1yS6INgjcZDNCr6lrB7w/Vmbk/9E81
# 8ZwP0zcTUtklNO2W7/hn6gi+j0l6/5Cx1PcpFdf5DV3Wh0MedMRwKLSAe70qm7uE
# 4Q6sbw25tfZtVv6KHQk+JA5nJsf8sg2glLCylMx75mf+pliy1NhBEsFV/W6Rxbux
# TAhLntRCBm8bGNU26mSuzv31BebiZtAOBSGssREGIxnk+wU0ROoIrp1JZxGLguWt
# WoanZv0zAwHemSX5cW7pnF0CTGA8zwKPAf1y7pLxpxLeQhJN7Kkm5XcCrA5XDAnR
# YZ4miPzIsk3bZPBFn7rBP1Sj2HYClWxqjcoiXPYMBOMp+kuwHNM3dITZHWarNHOP
# Hn18XpbWPRmwl+qMUJFtr1eGfhA3HWsaFN8CAwEAAaOCASkwggElMA4GA1UdDwEB
# /wQEAwIBhjASBgNVHRMBAf8ECDAGAQH/AgEAMB0GA1UdDgQWBBTqFsZp5+PLV0U5
# M6TwQL7Qw71lljAfBgNVHSMEGDAWgBSubAWjkxPioufi1xzWx/B/yGdToDA+Bggr
# BgEFBQcBAQQyMDAwLgYIKwYBBQUHMAGGImh0dHA6Ly9vY3NwMi5nbG9iYWxzaWdu
# LmNvbS9yb290cjYwNgYDVR0fBC8wLTAroCmgJ4YlaHR0cDovL2NybC5nbG9iYWxz
# aWduLmNvbS9yb290LXI2LmNybDBHBgNVHSAEQDA+MDwGBFUdIAAwNDAyBggrBgEF
# BQcCARYmaHR0cHM6Ly93d3cuZ2xvYmFsc2lnbi5jb20vcmVwb3NpdG9yeS8wDQYJ
# KoZIhvcNAQEMBQADggIBAH/iiNlXZytCX4GnCQu6xLsoGFbWTL/bGwdwxvsLCa0A
# OmAzHznGFmsZQEklCB7km/fWpA2PHpbyhqIX3kG/T+G8q83uwCOMxoX+SxUk+RhE
# 7B/CpKzQss/swlZlHb1/9t6CyLefYdO1RkiYlwJnehaVSttixtCzAsw0SEVV3ezp
# Sp9eFO1yEHF2cNIPlvPqN1eUkRiv3I2ZOBlYwqmhfqJuFSbqtPl/KufnSGRpL9Ka
# oXL29yRLdFp9coY1swJXH4uc/LusTN763lNMg/0SsbZJVU91naxvSsguarnKiMMS
# ME6yCHOfXqHWmc7pfUuWLMwWaxjN5Fk3hgks4kXWss1ugnWl2o0et1sviC49ffHy
# kTAFnM57fKDFrK9RBvARxx0wxVFWYOh8lT0i49UKJFMnl4D6SIknLHniPOWbHuOq
# hIKJPsBK9SH+YhDtHTD89szqSCd8i3VCf2vL86VrlR8EWDQKie2CUOTRe6jJ5r5I
# qitV2Y23JSAOG1Gg1GOqg+pscmFKyfpDxMZXxZ22PLCLsLkcMe+97xTYFEBsIB3C
# LegLxo1tjLZx7VIh/j72n585Gq6s0i96ILH0rKod4i0UnfqWah3GPMrz2Ry/U02k
# R1l8lcRDQfkl4iwQfoH5DZSnffK1CfXYYHJAUJUg1ENEvvqglecgWbZ4xqRqqiKb
# MIIGbDCCBFSgAwIBAgIQAZvq3shNa4/3bDqfLgEkFjANBgkqhkiG9w0BAQsFADBb
# MQswCQYDVQQGEwJCRTEZMBcGA1UEChMQR2xvYmFsU2lnbiBudi1zYTExMC8GA1UE
# AxMoR2xvYmFsU2lnbiBUaW1lc3RhbXBpbmcgQ0EgLSBTSEEzODQgLSBHNDAeFw0y
# MzExMDcxNzEzNDBaFw0zNDEyMDkxNzEzNDBaMF0xCzAJBgNVBAYTAkJFMRkwFwYD
# VQQKDBBHbG9iYWxTaWduIG52LXNhMTMwMQYDVQQDDCpHbG9iYWxzaWduIFRTQSBm
# b3IgQ29kZVNpZ24xIC0gUjYgLSAyMDIzMTEwggGiMA0GCSqGSIb3DQEBAQUAA4IB
# jwAwggGKAoIBgQDqhDdQaDyVhbVIVHGX84RyyZ2A0wjcWbu/y02QaUeJ5SzY8ahq
# fmA3SXObuJizFODvjJNwILX1K9dmBXU/eoRQqVIP4Nbnb9QcNS7gLykko453JSfg
# uPN9ZHUMmkLlxp3i2Idzi++gjZd+FqDEskbPYMbPD4FQgN1P6eRGojSnPhjHa7S2
# +77aqVZ+f5xdNj1jNCFLyNKjExo+L1Fodfl//HCEEWUzErNGZSiEoDPAkp3tuB+J
# Y9AeeyxbsAT0vXJjzrAxIQECanKyIizv6yo7ENXxmvQmoiz8UwEEeSNpUdNdvnOE
# y5uDiG0g0iDOgatqIkt5kELmgkKvA+vU0NqDesMtSPHNRZG3EUHdF+9A/BKissBZ
# qlSXTm3Det2GhtQ7N0c46AM8Mb9MM3tD7PDQ/2Wmn5g90sa6WSJsEFgRxQTM6KKY
# sB0z0POVHfjtcbQLQdVkeaK8YC7DesnecDcSFj4Cj104EbAEiWeLpS0g27jW9Zz1
# D7yIvycsw/kgosECAwEAAaOCAagwggGkMA4GA1UdDwEB/wQEAwIHgDAWBgNVHSUB
# Af8EDDAKBggrBgEFBQcDCDAdBgNVHQ4EFgQU+U6rvrj4W//CxwP0Qe6c+NZ0fT8w
# VgYDVR0gBE8wTTAIBgZngQwBBAIwQQYJKwYBBAGgMgEeMDQwMgYIKwYBBQUHAgEW
# Jmh0dHBzOi8vd3d3Lmdsb2JhbHNpZ24uY29tL3JlcG9zaXRvcnkvMAwGA1UdEwEB
# /wQCMAAwgZAGCCsGAQUFBwEBBIGDMIGAMDkGCCsGAQUFBzABhi1odHRwOi8vb2Nz
# cC5nbG9iYWxzaWduLmNvbS9jYS9nc3RzYWNhc2hhMzg0ZzQwQwYIKwYBBQUHMAKG
# N2h0dHA6Ly9zZWN1cmUuZ2xvYmFsc2lnbi5jb20vY2FjZXJ0L2dzdHNhY2FzaGEz
# ODRnNC5jcnQwHwYDVR0jBBgwFoAU6hbGaefjy1dFOTOk8EC+0MO9ZZYwQQYDVR0f
# BDowODA2oDSgMoYwaHR0cDovL2NybC5nbG9iYWxzaWduLmNvbS9jYS9nc3RzYWNh
# c2hhMzg0ZzQuY3JsMA0GCSqGSIb3DQEBCwUAA4ICAQCV9GfPk5pDkoMXdtYgVbyJ
# /+RfsuUVGreXEUAuAEG+xT8koBtHBzMETLS0Yv/G7tMMwjSglFw3mOtE+F/4JwoC
# vh7RJB0jyy8cqyd6s6nFLwzk5ZDwFSQeoxUZ+O+D6E+7aJ4jb309DXFwYTJ8Rm7y
# j9Gc6ENN+oPh/8TgPv7eV98Ga/NqBoFoqwx8KvePQv/Ac2nbHKcWTyYZWHPtlF7k
# GqIQggcyO8qev905ddS8i1Q/flCVNJ+SOSRTGINQ1FsvZrR7/lsp6x7FBf2jPfuh
# sPMreQ+xS6zDo9mj3CycKyOW9noqu63FV7aEtmOzYJgR7MbWZm8igxyKWAP0j+yc
# TtDf7s5hIdO1lVRJOyFRPaEjhZLCW/J6lISYCo8kViKKBPq0molINfhv64GGEVPA
# k3s6tNQ0zGLWNWGOplD7iHQe1zE1IR4Rv6ujZFOOVOJzZqT7ZmPhN8nEa/srM4sR
# w3SQgYPIT4bK3UqX236QUCWhYeyGmzZSB8s+YlXsM6bluBr63SQuDPkyiTFqNZX+
# 9tH/F4quIwkrIa5i7vfuN+ewybMV7Xaat/3n/uFo3kTBCnThlchX+xTrHlrflnXF
# ZRn39NWNIa9jtuv04hdGn7fXB25qnSv+6dJxTtK+T2LTSdZgX0DjdpkgRNhKXLNd
# Gtniqr9NWQAXnmBnOkcSoDGCA0kwggNFAgEBMG8wWzELMAkGA1UEBhMCQkUxGTAX
# BgNVBAoTEEdsb2JhbFNpZ24gbnYtc2ExMTAvBgNVBAMTKEdsb2JhbFNpZ24gVGlt
# ZXN0YW1waW5nIENBIC0gU0hBMzg0IC0gRzQCEAGb6t7ITWuP92w6ny4BJBYwCwYJ
# YIZIAWUDBAIBoIIBLTAaBgkqhkiG9w0BCQMxDQYLKoZIhvcNAQkQAQQwKwYJKoZI
# hvcNAQk0MR4wHDALBglghkgBZQMEAgGhDQYJKoZIhvcNAQELBQAwLwYJKoZIhvcN
# AQkEMSIEIPHRDqQJ1LDOx0fuXJL0SFYwJN8hzLyrirGjtTrFel25MIGwBgsqhkiG
# 9w0BCRACLzGBoDCBnTCBmjCBlwQgOoh6lRteuSpe4U9su3aCN6VF0BBb8EURveJf
# gqkW0egwczBfpF0wWzELMAkGA1UEBhMCQkUxGTAXBgNVBAoTEEdsb2JhbFNpZ24g
# bnYtc2ExMTAvBgNVBAMTKEdsb2JhbFNpZ24gVGltZXN0YW1waW5nIENBIC0gU0hB
# Mzg0IC0gRzQCEAGb6t7ITWuP92w6ny4BJBYwDQYJKoZIhvcNAQELBQAEggGAuHJl
# ek3P0mOKG+ulg8V1fw9abk+Dom/J+susPRVNIe25sgQ85CygtNSUIMan693uTl7w
# wBsr10kwvuzYlIHNpCtPTXDkCaC9Fp1biQ1VfJp97oQO1RwBsRCZnX7RjZImgwYs
# 39MEOj5nRZXfZYfn39uTUzqPUixtJPqTrswFiG+/mlPrGsYc1yvInmrrvdGjDd9v
# s5he5JV4ksGagMeu1Kp4KrxI26xTkCWUAr6NjY9+qigMIo40luEmtzIMkwdpRj0+
# tiu7eppvET6zD7zXmzieTsciyGKx/qzGsPp5nnaWMqvqpj/9PdMuPwM8Xuh+Sigj
# S6SOB8D1CswZ8jUQZBaWRvXfKqLUNv2rJgo5nSD1pblScqR0EtK7tb0WmOf2ct4/
# /ETnhuHJwPkZOAlYv5h/1XvxbNIEHQUGaR9cbmHpB21ISvBdQxDWzJ6iEVKkyne0
# 9VfgOlQeFqZ1o6vwRNHbxPLHCm7E/55mcv5ywjU+CdpWOhn7Vp57m8p5Uj+I
# SIG # End signature block
