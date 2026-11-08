$port = 5153

Start-Process ngrok -ArgumentList "http $port"

$url = $null
for ($i = 0; $i -lt 15 -and -not $url; $i++) {
    Start-Sleep -Seconds 1
    try {
        $url = (Invoke-RestMethod http://127.0.0.1:4040/api/tunnels).tunnels |
               Where-Object { $_.public_url -like "https://*" } |
               Select-Object -First 1 -ExpandProperty public_url
    } catch { }
}
if (-not $url) { Write-Host "Khong lay duoc dia chi ngrok"; exit 1 }
Write-Host "Ngrok: $url"

$env:Vnpay__ReturnUrl = "$url/Booking/VnpayReturn"

Start-Job { Start-Sleep -Seconds 8; Start-Process $using:url } | Out-Null

dotnet watch run --launch-profile httppowershell -ExecutionPolicy Bypass -File .\run.ps1