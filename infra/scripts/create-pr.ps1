<#
Script para criar branch, commitar mudanças, push e abrir PR usando gh CLI.
Uso:
  Abra PowerShell na raiz do repositório (ex: C:\dev\projects\ai-workflow-board)
  .\infra\scripts\create-pr.ps1 -BranchName "feature/infra-oidc" -Title "Infra: add Terraform S3/SQS + GitHub OIDC" -BaseBranch "main"

Pré-requisitos:
- git configurado e origin apontando para o repo GitHub
- gh CLI autenticado (`gh auth login`)

Parâmetros:
-BranchName (string) Nome da branch a criar (padrão: feature/infra-oidc)
-Title (string) Título do PR
-Body (string) Corpo da mensagem do PR
-BaseBranch (string) Branch alvo do PR (padrão: main)
#>
param(
    [string]$BranchName = "feature/infra-oidc",
    [string]$Title = "Infra: add Terraform S3/SQS + GitHub OIDC",
    [string]$Body = "This PR adds Terraform infra for S3 and SQS, an IAM role for GitHub OIDC, and related CI workflow. Also adds S3 image store and wiring for worker queue.",
    [string]$BaseBranch = "main"
)

Write-Host "Verificando estado do git..."
$cwd = Resolve-Path .

# Ensure we are in repo root
if (-not (Test-Path .git)) {
    Write-Error "Não encontrei um repositório git no diretório atual. Abra o PowerShell na raiz do repositório e rode novamente."; exit 1
}

# Fetch remote
git fetch origin

# Create branch
$exists = git rev-parse --verify $BranchName 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "Branch $BranchName já existe localmente. Fazendo checkout..."
    git checkout $BranchName
} else {
    git checkout -b $BranchName
}

# Stage changes
git add -A

# Commit (if there are staged changes)
$diffIndex = git diff --cached --name-only
if ([string]::IsNullOrWhiteSpace($diffIndex)) {
    Write-Host "Não há mudanças a commitar. Pulando commit." -ForegroundColor Yellow
} else {
    git commit -m $Title
}

# Push
git push -u origin $BranchName

# Create PR via gh
$ghExists = Get-Command gh -ErrorAction SilentlyContinue
if (-not $ghExists) {
    Write-Host "gh CLI não encontrado. PR não será criado automaticamente. Instale gh e rode: gh pr create --title \"$Title\" --body \"$Body\" --base $BaseBranch --head $BranchName" -ForegroundColor Yellow
    exit 0
}

Write-Host "Criando Pull Request via gh..."
$prUrl = gh pr create --title "$Title" --body "$Body" --base $BaseBranch --head $BranchName --web
if ($LASTEXITCODE -ne 0) {
    Write-Error "Falha ao criar PR via gh. Você pode criar manualmente: gh pr create --title \"$Title\" --body \"$Body\" --base $BaseBranch --head $BranchName"
} else {
    Write-Host "PR criado — abrindo navegador..."
}
