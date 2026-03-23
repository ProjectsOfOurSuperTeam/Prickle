# Test florarium image generation
# Run from project root. Requires atlas.jpg and layout.jpg in current directory.
#
# 1. Log in via frontend or Keycloak
# 2. Copy access_token from browser DevTools / Application / Local Storage (or Keycloak token response)
# 3. Paste below:

$token = ""

if ([string]::IsNullOrWhiteSpace($token)) {
    $token = Read-Host "Paste your access token"
}

# Use curl.exe for API calls (avoids TLS/SSL issues with Invoke-RestMethod on localhost)
$projectsJson = curl.exe -s -k -H "Authorization: Bearer $token" "https://localhost:7107/api/projects"
$projects = $projectsJson | ConvertFrom-Json
$projectId = $projects.items[0].id
Write-Host "Using project ID: $projectId"

curl.exe -k -X POST "https://localhost:7107/api/projects/$projectId/generate-florarium-image" `
    -H "Authorization: Bearer $token" `
    -F "atlasImage=@atlas.jpg" `
    -F "layoutImage=@layout.jpg" `
    -o florarium.png

if ($LASTEXITCODE -eq 0) {
    Write-Host "Saved to florarium.png"
} else {
    Write-Host "Request failed (exit code: $LASTEXITCODE)"
}
