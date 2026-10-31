param(
    [string]$GameDirectory = 'C:\SteamLibrary\steamapps\common\Pummel Party',
    [string]$DllPath = (Join-Path $PSScriptRoot '..\SchummelPartie\bin\Release\net48\SchummelPartie.dll')
)

$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $GameDirectory 'MelonLoader\net6\Mono.Cecil.dll')
$modAssembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Resolve-Path -LiteralPath $DllPath).Path)
$gameAssembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $GameDirectory 'PummelParty_Data\Managed\Assembly-CSharp.dll'))

try {
    $patchCount = 0
    foreach ($modType in $modAssembly.MainModule.Types) {
        foreach ($attribute in $modType.CustomAttributes) {
            if ($attribute.AttributeType.FullName -ne 'HarmonyLib.HarmonyPatch') { continue }
            $targetTypeName = $attribute.ConstructorArguments[0].Value.FullName
            $targetMethodName = [string]$attribute.ConstructorArguments[1].Value
            $targetType = $gameAssembly.MainModule.GetType($targetTypeName)
            if ($null -eq $targetType -or @($targetType.Methods | Where-Object { $_.Name -eq $targetMethodName }).Count -ne 1) {
                throw "Missing or ambiguous Harmony target: $targetTypeName.$targetMethodName"
            }
            $patchCount++
        }
    }
    if ($patchCount -ne 10) { throw "Expected 10 gameplay and free camera patches, found $patchCount." }
    Write-Output "PASS: $patchCount Harmony patch targets exist in the installed game."

    $prefix = $modAssembly.MainModule.GetType('SchummelPartie.module.NoClipCamPatch').Methods |
        Where-Object { $_.Name -eq 'Prefix' }
    $prefixOpcodes = ($prefix.Body.Instructions | ForEach-Object { $_.OpCode.Code.ToString() }) -join ','
    if ($prefixOpcodes -ne 'Ldc_I4_0,Ret') { throw 'The unsafe stock F11 handler is not unconditionally bypassed.' }
    Write-Output 'PASS: The stock F11 handler is unconditionally bypassed.'

    $freeCamera = $modAssembly.MainModule.GetType('SchummelPartie.module.LocalFreeCamera')
    foreach ($method in $freeCamera.Methods) {
        if (!$method.HasBody) { continue }
        foreach ($instruction in $method.Body.Instructions) {
            if ([string]$instruction.Operand -match 'set_timeScale|::PauseGame\(|::UnpauseGame\(|::SendRPC\(|NoClipCam::disableComponents') {
                throw "Unsafe camera operation in $($method.Name): $instruction"
            }
        }
    }
    Write-Output 'PASS: The local camera never changes game time, pauses the game or sends RPCs.'

    foreach ($modType in $modAssembly.MainModule.Types) {
        foreach ($method in $modType.Methods) {
            if (!$method.HasBody) { continue }
            foreach ($instruction in $method.Body.Instructions) {
                if ([string]$instruction.Operand -match '::GiveItem\(|::GetRandomItem\(') {
                    throw "Unexpected item grant in $($modType.FullName).$($method.Name)"
                }
            }
        }
    }
    Write-Output 'PASS: The DLL contains no item grant or random item acquisition calls.'

    if ($modAssembly.MainModule.AssemblyReferences | Where-Object { $_.Name -eq 'System.Private.CoreLib' }) {
        throw 'The mod incorrectly depends on the desktop .NET runtime.'
    }
    $framework = $modAssembly.CustomAttributes | Where-Object { $_.AttributeType.Name -eq 'TargetFrameworkAttribute' }
    if ($framework.ConstructorArguments[0].Value -ne '.NETFramework,Version=v4.8') { throw 'Unexpected target framework.' }
    Write-Output 'PASS: The DLL targets .NET Framework 4.8 for the installed Mono game.'
}
finally {
    $modAssembly.Dispose()
    $gameAssembly.Dispose()
}
