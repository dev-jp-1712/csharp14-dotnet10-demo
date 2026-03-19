# Rate Limiting Configuration Summary
# Shows what policies are applied to which endpoints

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     RATE LIMITING CONFIGURATION SUMMARY                   ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

$config = @(
    [PSCustomObject]@{
        Controller = "OrdersController"
        Policy = "orders (Fixed Window)"
        Limit = "10 requests/minute"
        Queue = "2 requests"
        Endpoints = @(
            "GET  /api/orders",
            "GET  /api/orders/customer/{id}",
            "GET  /api/orders/{id}",
            "POST /api/orders",
            "POST /api/orders/{id}/pay",
            "POST /api/orders/{id}/ship",
            "POST /api/orders/{id}/deliver",
            "POST /api/orders/{id}/cancel"
        )
    },
    [PSCustomObject]@{
        Controller = "ProductsController"
        Policy = "reads (Token Bucket)"
        Limit = "50 tokens"
        Queue = "3 requests"
        Endpoints = @(
            "GET  /api/products",
            "GET  /api/products/{id}"
        )
        RefillRate = "10 tokens every 10 seconds"
    },
    [PSCustomObject]@{
        Controller = "CustomersController"
        Policy = "api (Sliding Window)"
        Limit = "100 requests/minute"
        Queue = "5 requests"
        Endpoints = @(
            "GET  /api/customers",
            "GET  /api/customers/{id}"
        )
        Segments = "6 segments × 10 seconds"
    }
)

foreach ($item in $config) {
    Write-Host "📦 $($item.Controller)" -ForegroundColor Yellow
    Write-Host "   Policy:    $($item.Policy)" -ForegroundColor White
    Write-Host "   Limit:     $($item.Limit)" -ForegroundColor White
    Write-Host "   Queue:     $($item.Queue)" -ForegroundColor White
    
    if ($item.RefillRate) {
        Write-Host "   Refill:    $($item.RefillRate)" -ForegroundColor White
    }
    
    if ($item.Segments) {
        Write-Host "   Segments:  $($item.Segments)" -ForegroundColor White
    }
    
    Write-Host "   Endpoints:" -ForegroundColor Gray
    foreach ($endpoint in $item.Endpoints) {
        Write-Host "      • $endpoint" -ForegroundColor Gray
    }
    Write-Host ""
}

Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     POLICY DETAILS                                         ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

Write-Host "🔒 Fixed Window (orders)" -ForegroundColor Yellow
Write-Host "   Strict time-based limit. Resets exactly at window boundaries." -ForegroundColor Gray
Write-Host "   Best for: Write operations with strict rate controls`n" -ForegroundColor Gray

Write-Host "🪣 Token Bucket (reads)" -ForegroundColor Yellow
Write-Host "   Allows burst traffic. Tokens refill over time." -ForegroundColor Gray
Write-Host "   Best for: Read operations with occasional spikes`n" -ForegroundColor Gray

Write-Host "🎚️  Sliding Window (api)" -ForegroundColor Yellow
Write-Host "   Smooth rate limiting across rolling time periods." -ForegroundColor Gray
Write-Host "   Best for: General API access with flexible limits`n" -ForegroundColor Gray

Write-Host "⚙️  Concurrency (concurrent - Available but not applied)" -ForegroundColor Yellow
Write-Host "   Limits simultaneous connections." -ForegroundColor Gray
Write-Host "   Best for: Resource-intensive operations (20 concurrent max)`n" -ForegroundColor Gray

Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║     QUICK START                                            ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Green

Write-Host "1. Start API:" -ForegroundColor Cyan
Write-Host "   dotnet run --project src\OrderService\OrderService.csproj`n" -ForegroundColor White

Write-Host "2. Quick Test:" -ForegroundColor Cyan
Write-Host "   .\QuickTest.ps1`n" -ForegroundColor White

Write-Host "3. Detailed Test:" -ForegroundColor Cyan
Write-Host "   .\TestRateLimiting.ps1`n" -ForegroundColor White

Write-Host "4. Read Guide:" -ForegroundColor Cyan
Write-Host "   Open RATE_LIMITING_GUIDE.md`n" -ForegroundColor White

Write-Host "✅ Everything is configured and ready to test!`n" -ForegroundColor Green
