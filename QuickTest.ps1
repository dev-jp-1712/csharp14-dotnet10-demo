# Quick Rate Limiting Test - Simple version
# Run your API first: dotnet run --project src\OrderService\OrderService.csproj

$baseUrl = "http://localhost:5000"

Write-Host "🚀 Quick Rate Limiting Test" -ForegroundColor Cyan
Write-Host "Make sure your API is running on $baseUrl`n" -ForegroundColor Yellow

# Quick test function
function QuickTest {
    param([string]$Name, [string]$Url, [int]$Count)
    
    Write-Host "`n📊 Testing: $Name ($Count requests)" -ForegroundColor Magenta
    $success = 0
    $limited = 0
    
    1..$Count | ForEach-Object {
        try {
            Invoke-RestMethod -Uri $Url -Method GET -ErrorAction Stop | Out-Null
            $success++
            Write-Host "✓" -NoNewline -ForegroundColor Green
        }
        catch {
            if ($_.Exception.Response.StatusCode.value__ -eq 429) {
                $limited++
                Write-Host "⚠" -NoNewline -ForegroundColor Yellow
            }
            else {
                Write-Host "✗" -NoNewline -ForegroundColor Red
            }
        }
    }
    
    Write-Host "`nSuccess: $success | Rate Limited: $limited" -ForegroundColor Cyan
}

# Test each endpoint
QuickTest "Orders (Fixed Window: 10/min)" "$baseUrl/api/orders" 15
QuickTest "Products (Token Bucket: 50 tokens)" "$baseUrl/api/products" 60
QuickTest "Customers (Sliding Window: 100/min)" "$baseUrl/api/customers" 110

Write-Host "`n✅ Done! Check for yellow ⚠ symbols = rate limited (429)" -ForegroundColor Green
