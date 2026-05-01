<#
PowerShell script para instalar o Terraform no Windows (usuário ou sistema).
- Detecta a última versão do Terraform via HashiCorp Checkpoint API
- Baixa o zip, extrai para um diretório (prefere C:\Tools\terraform se houver permissão)
- Adiciona o diretório ao PATH do usuário

Uso:
1) Abra PowerShell (pode ser sem admin; se quiser instalar em C:\Tools execute como Admin)
2) Executar: `.	ools\install-terraform.ps1` ou `PowerShell -ExecutionPolicy Bypass -File .\\infra\\scripts\\install-terraform.ps1`
#>

Set-StrictMode -Version Latest

try {
    Write-Host "Detectando a última versão do Terraform..."
    $checkpoint = Invoke-RestMethod -Uri 'https://checkpoint-api.hashicorp.com/v1/check/terraform' -UseBasicParsing -ErrorAction Stop
    $version = $checkpoint.current_version
} catch {
    Write-Warning "Não foi possível consultar a versão automática; usando versão fallback 1.5.7"
    $version = '1.5.7'
}

$arch = 'amd64'
$platform = 'windows'
$zipName = "terraform_${version}_${platform}_${arch}.zip"
$url = "https://releases.hashicorp.com/terraform/$version/$zipName"

# Escolha diretório de instalação: prefira C:\Tools se possível, senão usar pasta do usuário
$systemDir = 'C:\Tools\terraform'
$userDir = Join-Path $env:USERPROFILE 'Tools\\terraform'

$installDir = $null
if ((Test-Path $systemDir) -or ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    $installDir = $systemDir
} else {
    $installDir = $userDir
}

Write-Host "Instalando Terraform $version em: $installDir"

$tempZip = Join-Path $env:TEMP "terraform_download.zip"

try {
    Write-Host "Baixando $url ..."
    Invoke-WebRequest -Uri $url -OutFile $tempZip -UseBasicParsing -ErrorAction Stop
} catch {
    Write-Error "Falha ao baixar o Terraform de $url. Verifique conectividade e tente novamente."
    throw
}

# Criar diretório e extrair
New-Item -ItemType Directory -Force -Path $installDir | Out-Null

Add-Type -AssemblyName System.IO.Compression.FileSystem

try {
    [System.IO.Compression.ZipFile]::ExtractToDirectory($tempZip, $installDir)
} catch {
    Write-Error "Falha ao extrair o zip: $_"
    throw
}

# Atualizar PATH do usuário se necessário
$oldUserPath = [Environment]::GetEnvironmentVariable('Path', 'User')
if ($oldUserPath -notlike "*$installDir*") {
    $newUserPath = if ([string]::IsNullOrEmpty($oldUserPath)) { $installDir } else { "$oldUserPath;$installDir" }
    [Environment]::SetEnvironmentVariable('Path', $newUserPath, 'User')
    Write-Host "Adicionado $installDir ao PATH do usuário. Feche e reabra o terminal para atualizar o PATH." -ForegroundColor Green
} else {
    Write-Host "Diretório já estava no PATH do usuário." -ForegroundColor Yellow
}

# Atualizar sessão atual também
$env:Path = "$env:Path;$installDir"

Write-Host "Verificando instalação..."
try {
    & (Join-Path $installDir 'terraform.exe') -version
} catch {
    Write-Warning "Não foi possível executar terraform diretamente. Reinicie o terminal ou verifique permissão de execução."    
}

Write-Host "Instalação concluída. Para usar o Terraform em novos terminais, reinicie o PowerShell." -ForegroundColor Green
