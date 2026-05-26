param(
    [string]$BaseUrl = "http://localhost:8311",
    [string]$AdminUsername = "admin",
    [string]$AdminPassword = "Admin12345!",
    [string]$UserUsername = "user",
    [string]$UserPassword = "User12345!"
)

$ErrorActionPreference = "Stop"
$timestamp = [int][double]::Parse((Get-Date -UFormat %s))

function Get-HttpCode {
    param([string[]]$CurlArgs)

    $baseArgs = @("-s", "-o", "NUL", "-w", "%{http_code}")
    $code = & curl.exe @baseArgs @CurlArgs
    return ($code | Out-String).Trim()
}

function Add-Result {
    param(
        [System.Collections.Generic.List[object]]$Results,
        [string]$Label,
        [string]$Actual,
        [string]$Expected
    )

    $ok = $Actual -eq $Expected
    $Results.Add([PSCustomObject]@{
            Label = $Label
            Actual = $Actual
            Expected = $Expected
            Ok = $ok
        })
}

function Get-Token {
    param(
        [string]$Username,
        [string]$Password
    )

    $body = @{ username = $Username; password = $Password } | ConvertTo-Json
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method Post -Body $body -ContentType "application/json"
    if (-not $response.accessToken) {
        throw "No se pudo obtener token para usuario '$Username'."
    }

    return [string]$response.accessToken
}

$tmpDir = Join-Path $env:TEMP "sillas3cantos-auth-$timestamp"
New-Item -ItemType Directory -Path $tmpDir -Force | Out-Null

try {
    $adminToken = Get-Token -Username $AdminUsername -Password $AdminPassword
    $userToken = Get-Token -Username $UserUsername -Password $UserPassword

    $catNoAuthPath = Join-Path $tmpDir "cat-noauth.json"
    $prodNoAuthPath = Join-Path $tmpDir "prod-noauth.json"
    $catUserPath = Join-Path $tmpDir "cat-user.json"
    $marcaUserPath = Join-Path $tmpDir "marca-user.json"
    $prodUserPath = Join-Path $tmpDir "prod-user-invalid-fk.json"
    $userNoAuthPath = Join-Path $tmpDir "user-noauth.json"
    $userByUserPath = Join-Path $tmpDir "user-by-user.json"
    $userBySuperPath = Join-Path $tmpDir "user-by-superadmin.json"

    @{ nombre = "cat-noauth-$timestamp" } | ConvertTo-Json | Set-Content -LiteralPath $catNoAuthPath -Encoding utf8
    @{
        nombre = "prod-noauth-$timestamp"
        descripcion = "test sin auth"
        precio = 9.99
        stock = 2
        categoriaId = 999999
        marcaId = 999999
    } | ConvertTo-Json | Set-Content -LiteralPath $prodNoAuthPath -Encoding utf8
    @{ nombre = "cat-user-$timestamp" } | ConvertTo-Json | Set-Content -LiteralPath $catUserPath -Encoding utf8
    @{ nombre = "marca-user-$timestamp" } | ConvertTo-Json | Set-Content -LiteralPath $marcaUserPath -Encoding utf8
    @{
        nombre = "prod-user-$timestamp"
        descripcion = "test auth usuario"
        precio = 10.50
        stock = 3
        categoriaId = 999999
        marcaId = 999999
    } | ConvertTo-Json | Set-Content -LiteralPath $prodUserPath -Encoding utf8
    @{
        username = "u.noauth.$timestamp"
        password = "User12345!"
        role = "User"
        email = "u.noauth.$timestamp@mail.com"
    } | ConvertTo-Json | Set-Content -LiteralPath $userNoAuthPath -Encoding utf8
    @{
        username = "u.user.$timestamp"
        password = "User12345!"
        role = "User"
        email = "u.user.$timestamp@mail.com"
    } | ConvertTo-Json | Set-Content -LiteralPath $userByUserPath -Encoding utf8
    @{
        username = "u.super.$timestamp"
        password = "User12345!"
        role = "User"
        email = "u.super.$timestamp@mail.com"
    } | ConvertTo-Json | Set-Content -LiteralPath $userBySuperPath -Encoding utf8

    $results = [System.Collections.Generic.List[object]]::new()

    Add-Result -Results $results -Label "GET /api/productos (publico)" `
        -Actual (Get-HttpCode @("$BaseUrl/api/productos")) -Expected "200"

    Add-Result -Results $results -Label "GET /api/categorias (publico)" `
        -Actual (Get-HttpCode @("$BaseUrl/api/categorias")) -Expected "200"

    Add-Result -Results $results -Label "POST /api/categorias sin token" `
        -Actual (Get-HttpCode @("-X", "POST", "$BaseUrl/api/categorias", "-H", "Content-Type: application/json", "--data-binary", "@$catNoAuthPath")) -Expected "401"

    Add-Result -Results $results -Label "POST /api/productos sin token" `
        -Actual (Get-HttpCode @("-X", "POST", "$BaseUrl/api/productos", "-H", "Content-Type: application/json", "--data-binary", "@$prodNoAuthPath")) -Expected "401"

    Add-Result -Results $results -Label "POST /api/usuarios sin token" `
        -Actual (Get-HttpCode @("-X", "POST", "$BaseUrl/api/usuarios", "-H", "Content-Type: application/json", "--data-binary", "@$userNoAuthPath")) -Expected "401"

    Add-Result -Results $results -Label "POST /api/categorias con User" `
        -Actual (Get-HttpCode @("-X", "POST", "$BaseUrl/api/categorias", "-H", "Authorization: Bearer $userToken", "-H", "Content-Type: application/json", "--data-binary", "@$catUserPath")) -Expected "201"

    Add-Result -Results $results -Label "POST /api/marcas con User" `
        -Actual (Get-HttpCode @("-X", "POST", "$BaseUrl/api/marcas", "-H", "Authorization: Bearer $userToken", "-H", "Content-Type: application/json", "--data-binary", "@$marcaUserPath")) -Expected "201"

    Add-Result -Results $results -Label "POST /api/productos con User (FK invalida)" `
        -Actual (Get-HttpCode @("-X", "POST", "$BaseUrl/api/productos", "-H", "Authorization: Bearer $userToken", "-H", "Content-Type: application/json", "--data-binary", "@$prodUserPath")) -Expected "400"

    Add-Result -Results $results -Label "POST /api/usuarios con User" `
        -Actual (Get-HttpCode @("-X", "POST", "$BaseUrl/api/usuarios", "-H", "Authorization: Bearer $userToken", "-H", "Content-Type: application/json", "--data-binary", "@$userByUserPath")) -Expected "403"

    Add-Result -Results $results -Label "POST /api/usuarios con SuperAdmin" `
        -Actual (Get-HttpCode @("-X", "POST", "$BaseUrl/api/usuarios", "-H", "Authorization: Bearer $adminToken", "-H", "Content-Type: application/json", "--data-binary", "@$userBySuperPath")) -Expected "201"

    Add-Result -Results $results -Label "GET /api/usuarios (publico)" `
        -Actual (Get-HttpCode @("$BaseUrl/api/usuarios")) -Expected "200"

    Write-Output "Login admin OK (token_len=$($adminToken.Length))"
    Write-Output "Login user OK (token_len=$($userToken.Length))"
    Write-Output ""

    foreach ($result in $results) {
        $status = if ($result.Ok) { "OK" } else { "FAIL" }
        Write-Output ("[{0}] {1}: {2} (esperado {3})" -f $status, $result.Label, $result.Actual, $result.Expected)
    }

    if ($results.Where({ -not $_.Ok }).Count -gt 0) {
        exit 1
    }
}
finally {
    Remove-Item -LiteralPath $tmpDir -Recurse -Force -ErrorAction SilentlyContinue
}
