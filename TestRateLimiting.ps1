# Rate Limiting Test Script for OrderService API
# Make sure your API is running before executing this script

$baseUrl = "http://localhost:5000"

function Test-RateLimitPolicy {
    param(
        [string]$PolicyName,
        [string]$Endpoint,
        [string]$Method = "GET",
        [int]$RequestCount,
        [string]$Body = $null,
        [int]$DelayMs = 0
    )
    
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Testing Policy: $PolicyName" -ForegroundColor Cyan
    Write-Host "Endpoint: $Endpoint" -ForegroundColor Cyan
    Write-Host "Sending $RequestCount requests..." -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
    
    $successCount = 0
    $rateLimitedCount = 0
    $errorCount = 0
    
    for ($i = 1; $i -le $RequestCount; $i++) {
        try {
            $params = @{
                Uri = "$baseUrl$Endpoint"
                Method = $Method
                ContentType = "application/json"
                ErrorAction = "Stop"
                StatusCodeVariable = "statusCode"
            }
            
            if ($Body) {
                $params.Body = $Body
            }
            
            $response = Invoke-RestMethod @params
            $successCount++
            Write-Host "[$i] SUCCESS (200)" -ForegroundColor Green
            
            if ($DelayMs -gt 0) {
                Start-Sleep -Milliseconds $DelayMs
            }
        }
        catch {
            $statusCode = $_.Exception.Response.StatusCode.value__
            
            if ($statusCode -eq 429) {
                $rateLimitedCount++
                
                # Try to read the response body
                $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
                $errorBody = $reader.ReadToEnd() | ConvertFrom-Json
                $retryAfter = $_.Exception.Response.Headers["Retry-After"]
                
                Write-Host "[$i] RATE LIMITED (429) - Retry after: $retryAfter seconds" -ForegroundColor Yellow
                Write-Host "    Message: $($errorBody.message)" -ForegroundColor Yellow
            }
            else {
                $errorCount++
                Write-Host "[$i] ERROR ($statusCode)" -ForegroundColor Red
            }
        }
    }
    
    Write-Host "`nResults:" -ForegroundColor Cyan
    Write-Host "  Success: $successCount" -ForegroundColor Green
    Write-Host "  Rate Limited (429): $rateLimitedCount" -ForegroundColor Yellow
    Write-Host "  Errors: $errorCount" -ForegroundColor Red
}

# Test 1: Fixed Window Policy ("orders") - 10 requests/minute, queue=2
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  TEST 1: Fixed Window - Orders Endpoint                   ║" -ForegroundColor Magenta
Write-Host "║  Policy: 10 requests/minute + 2 queued                    ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

Test-RateLimitPolicy -PolicyName "orders" -Endpoint "/api/orders" -RequestCount 15

# Test 2: Token Bucket Policy ("reads") - 50 tokens, refill 10/10sec
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  TEST 2: Token Bucket - Products Endpoint                 ║" -ForegroundColor Magenta
Write-Host "║  Policy: 50 tokens, refill 10 every 10 seconds            ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

Test-RateLimitPolicy -PolicyName "reads" -Endpoint "/api/products" -RequestCount 60

Write-Host "`nWaiting 10 seconds for token refill..." -ForegroundColor Cyan
Start-Sleep -Seconds 10

Write-Host "`nTesting after refill (should get 10 more tokens):" -ForegroundColor Cyan
Test-RateLimitPolicy -PolicyName "reads" -Endpoint "/api/products" -RequestCount 15

# Test 3: Sliding Window Policy ("api") - 100 requests/minute, 6 segments
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  TEST 3: Sliding Window - Customers Endpoint              ║" -ForegroundColor Magenta
Write-Host "║  Policy: 100 requests/minute, 6 segments (10 sec each)    ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

Test-RateLimitPolicy -PolicyName "api" -Endpoint "/api/customers" -RequestCount 110

# Test 4: Test specific product endpoint (also uses "reads" policy)
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  TEST 4: Token Bucket - Specific Product by ID            ║" -ForegroundColor Magenta
Write-Host "║  Policy: Same 'reads' policy (tokens already depleted)    ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

# Get a product ID first
try {
    $products = Invoke-RestMethod -Uri "$baseUrl/api/products" -Method GET -ErrorAction SilentlyContinue
    if ($products -and $products.Count -gt 0) {
        $productId = $products[0].id
        Write-Host "Using Product ID: $productId" -ForegroundColor Cyan
        Test-RateLimitPolicy -PolicyName "reads" -Endpoint "/api/products/$productId" -RequestCount 10
    }
}
catch {
    Write-Host "Could not fetch product ID (tokens likely depleted)" -ForegroundColor Yellow
}

# Test 5: Rapid fire test to see queueing in action
Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  TEST 5: Burst Test - See Queueing Behavior               ║" -ForegroundColor Magenta
Write-Host "║  Sending 5 requests with 100ms delay to same endpoint     ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

Test-RateLimitPolicy -PolicyName "orders" -Endpoint "/api/orders" -RequestCount 5 -DelayMs 100

Write-Host "`n✅ Rate Limiting Tests Complete!" -ForegroundColor Green
Write-Host "`nKey Observations:" -ForegroundColor Cyan
Write-Host "  • Orders: Strict 10/min limit with 2 queue slots" -ForegroundColor White
Write-Host "  • Products: Burst-friendly with token refill" -ForegroundColor White
Write-Host "  • Customers: Smooth 100/min with sliding window" -ForegroundColor White
Write-Host "  • 429 responses include Retry-After header" -ForegroundColor White
