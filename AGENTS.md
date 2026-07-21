# Agent Guide

This repository contains PowerShell Pro Tools: Visual Studio extensions, a VS Code extension, PowerShell cmdlets/modules, packaging/MSBuild helpers, host services, and Windows Forms designer components.

## Repository layout

- `PowerShellTools.sln` is the main Visual Studio solution.
- `PowerShellTools\` and `PowerShellTools.2022\` contain the Visual Studio extension projects.
- `vscode\powershellprotools\` contains the VS Code extension TypeScript package. Its compiled output goes to `out\`, and packaged VSIX files go to `kit\`.
- `PowerShellToolsPro.Cmdlets\` contains the PowerShell module/cmdlet build.
- `PowerShellToolsPro.Packager\`, `PowerShellToolsPro.MsBuild\`, and related `*.Test` projects cover packaging and MSBuild integration.
- `WinFormDesigner\`, `FormDesigner\`, and `FormDesigner.Generator\` contain Windows Forms designer code.
- `PowerShellProTools.Host\`, `IronmanPowerShellHost\`, and `HostInjection\` contain host/runtime pieces.
- `Build\` contains shared MSBuild settings, version files, signing helpers, and VS discovery utilities.
- `ThirdParty\` and checked-in template/sample assets should be treated as vendored or compatibility-sensitive unless the task explicitly targets them.

## Prerequisites and environment

Work on Windows. The full repository depends on Visual Studio 2022 with .NET desktop development and Visual Studio extension development workloads, MSBuild, .NET SDK 6.x, PowerShell 7, Node.js, NuGet, and npm.

Some projects still target .NET Framework (`net472`, `v4.7.2`, `v4.6.2`, or older) while newer components target `netstandard2.0` or `net6.0`. Avoid broad framework, package, or TypeScript upgrades unless the task specifically calls for them.

Signing is handled by Ironman Software during release. Development changes should not add private certificates, secrets, or local signing assumptions. Existing signing scripts may no-op or use CI-provided credentials.

## Restore, build, and test

Prefer targeted validation for the component you changed.

- Restore the solution:
  ```powershell
  dotnet restore .\PowerShellTools.sln
  nuget restore .\PowerShellTools.sln
  ```
- Build the Visual Studio solution, matching CI:
  ```powershell
  msbuild .\PowerShellTools.sln -p:Configuration=Release -p:Platform="Any CPU"
  ```
- Build the WinForms designer path used by CI:
  ```powershell
  msbuild .\PowerShellTools.sln -t:rebuild -p:Configuration=Release -p:Platform=x64
  ```
- Build the host used by CI:
  ```powershell
  dotnet publish .\IronmanPowerShellHost\IronmanPowerShellHost.csproj -f net472 -c Release
  ```
- Build cmdlets/module output:
  ```powershell
  .\PowerShellToolsPro.Cmdlets\build.ps1
  ```
- Build the VS Code extension as CI does:
  ```powershell
  Import-Module .\Build\Modules\InvokeBuild\5.14.23\InvokeBuild.psd1 -Force
  Install-Module Microsoft.PowerShell.PlatyPS -RequiredVersion 1.0.1 -Scope CurrentUser -Force -AllowClobber
  Invoke-Build -File .\vscode\vscode.build.ps1
  ```
- For VS Code-only TypeScript changes, use the lighter commands from `vscode\powershellprotools`:
  ```powershell
  npm install
  npm run compile
  npm run lint
  npm test
  ```
- For SDK-style test projects, use targeted `dotnet test`, for example:
  ```powershell
  dotnet test .\PowerShellProTools.Host.Tests\PowerShellProTools.Host.Tests.csproj
  ```

Older .NET Framework test projects use xUnit but may need Visual Studio test infrastructure or restored `packages.config` dependencies. If a targeted test project cannot run with `dotnet test`, document the limitation and validate with the nearest build or CI-equivalent command.

## Coding conventions

- Match the surrounding project style. This repo mixes older C# project formats, SDK-style projects, PowerShell scripts, and TypeScript.
- Keep changes narrow. Many projects are tightly coupled through the solution, shared build files, and VSIX packaging.
- Preserve compatibility with the declared target framework of the project you are editing.
- Do not introduce nullable reference type assumptions or strict TypeScript assumptions globally; the VS Code extension has `"strict": false`.
- Prefer existing helpers, shared projects, and build properties before adding new infrastructure.
- Keep generated/build outputs (`bin\`, `obj\`, `out\`, `kit\`, VSIX artifacts, and restored package folders) out of commits unless the repository already tracks the specific file and the task requires changing it.
- When editing templates, sample scripts, or tests under `Assets`, preserve line endings and expected output formatting; many tests compare generated script text exactly.

## CI reference

GitHub Actions workflows in `.github\workflows\` are the best source of truth for full validation:

- `vs.yml` restores NuGet packages, prepares image manifests, publishes `IronmanPowerShellHost`, and builds `PowerShellTools.sln`.
- `vscode.ci.yml` restores packages, builds the WinForms designer, then runs `Invoke-Build -File .\vscode\vscode.build.ps1`.
- `cmdlets.ci.yml` restores packages, publishes the host, rebuilds the solution for x64, and runs `PowerShellToolsPro.Cmdlets\build.ps1`.

Use these workflows to choose the smallest local command that exercises the changed surface.
