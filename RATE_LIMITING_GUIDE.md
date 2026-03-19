# Rate Limiting Implementation & Testing Guide

## 📋 What Was Applied

### Controllers with Rate Limiting Policies

| Controller | Policy Applied | Limits |
|------------|----------------|--------|
| **OrdersController** | `orders` (Fixed Window) | 10 requests/minute + 2 queued |
| **ProductsController** | `reads` (Token Bucket) | 50 tokens, refill 10 every 10 seconds |
| **CustomersController** | `api` (Sliding Window) | 100 requests/minute across 6 segments |

### Changes Made

1. **OrdersController.cs**: Added `[EnableRateLimiting("orders")]`
2. **ProductsController.cs**: Added `[EnableRateLimiting("reads")]`
3. **CustomersController.cs**: Added `[EnableRateLimiting("api")]`

All controllers imported: `using Microsoft.AspNetCore.RateLimiting;`

---

## 🚀 How to Run & Test

### Step 1: Start Your API

```powershell
# Navigate to your workspace
cd C:\Users\jatin\Desktop\POC\CSharp14DotNet10Demo

# Run the API
dotnet run --project src\OrderService\OrderService.csproj
```

Your API should start on: `http://localhost:5000`

### Step 2: Run Quick Test

```powershell
# Simple visual test
.\QuickTest.ps1
```

**Expected Output:**
- ✓ = Success (200 OK)
- ⚠ = Rate Limited (429)
- ✗ = Error

### Step 3: Run Detailed Tests

```powershell
# Comprehensive test with detailed reporting
.\TestRateLimiting.ps1
```

**This will test:**
- Fixed Window behavior
- Token Bucket burst & refill
- Sliding Window smoothness
- Queue behavior
- Retry-After headers

---

## 🧪 Manual Testing with PowerShell

### Test 1: Orders Endpoint (Fixed Window)

```powershell
# Should succeed for first 10 requests, then 429
1..15 | ForEach-Object {
    try {
        Invoke-RestMethod -Uri "http://localhost:5000/api/orders" -Method GET
        Write-Host "Request $_ succeeded"
    }
    catch {
        Write-Host "Request $_ failed: $($_.Exception.Message)"
    }
}
```

### Test 2: Products Endpoint (Token Bucket)

```powershell
# Burst test - drains token bucket
1..55 | ForEach-Object {
    Invoke-RestMethod -Uri "http://localhost:5000/api/products" -Method GET
}

# Wait for refill
Start-Sleep -Seconds 10

# Should have 10 new tokens
1..15 | ForEach-Object {
    Invoke-RestMethod -Uri "http://localhost:5000/api/products" -Method GET
}
```

### Test 3: Customers Endpoint (Sliding Window)

```powershell
# Smooth rate limiting across 1-minute window
1..110 | ForEach-Object {
    Invoke-RestMethod -Uri "http://localhost:5000/api/customers" -Method GET
    Start-Sleep -Milliseconds 100
}
```

---

## 🔍 How to Verify Rate Limiting

### Check Response Headers

```powershell
$response = Invoke-WebRequest -Uri "http://localhost:5000/api/orders" -Method GET
$response.Headers
# Look for: Retry-After
```

### Inspect 429 Response Body

```powershell
try {
    # Send enough requests to trigger rate limit
    1..20 | ForEach-Object {
        Invoke-RestMethod -Uri "http://localhost:5000/api/orders"
    }
}
catch {
    $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
    $body = $reader.ReadToEnd()
    Write-Host $body -ForegroundColor Yellow
}
```

**Expected 429 Response:**
```json
{
  "error": "RateLimitExceeded",
  "message": "Too many requests. Please try again later.",
  "retryAfter": "60 seconds",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

---

## 📊 Testing with Browser/Postman

### Using Browser DevTools

1. Open `http://localhost:5000/api/products` in browser
2. Open DevTools (F12) → Network tab
3. Refresh page rapidly (Ctrl+R multiple times)
4. Look for **429 status codes**
5. Check **Retry-After header** in response

### Using Postman

1. Create a GET request to `http://localhost:5000/api/orders`
2. Use **Collection Runner** with 15 iterations
3. Set delay to 0ms
4. Run and observe:
   - First 10-12 requests: 200 OK
   - Remaining: 429 Too Many Requests

---

## 🎯 Expected Behavior by Endpoint

### `/api/orders` (Fixed Window - "orders" policy)

```
Requests 1-10:  ✓ 200 OK
Requests 11-12: ✓ 200 OK (queued)
Requests 13+:   ⚠ 429 Too Many Requests
After 60 sec:   Window resets, limits refresh
```

### `/api/products` (Token Bucket - "reads" policy)

```
Requests 1-50:  ✓ 200 OK (bucket drains)
Requests 51-53: ✓ 200 OK (queued)
Requests 54+:   ⚠ 429 Too Many Requests
After 10 sec:   +10 tokens available
After 20 sec:   +10 tokens available (total 20)
```

### `/api/customers` (Sliding Window - "api" policy)

```
Requests 1-100:   ✓ 200 OK
Requests 101-105: ✓ 200 OK (queued)
Requests 106+:    ⚠ 429 Too Many Requests
Sliding behavior: Limits spread across 6×10-second segments
```

---

## 🛠️ Testing with curl

### Basic Test

```bash
# Linux/Mac/Git Bash
for i in {1..15}; do
  curl -s -o /dev/null -w "Request $i: %{http_code}\n" http://localhost:5000/api/orders
done
```

### Check Headers

```bash
curl -i http://localhost:5000/api/orders
# Look for: Retry-After header
```

---

## 📈 Advanced Testing Scenarios

### Test Concurrency Limiter (Not Applied Yet)

If you want to test the "concurrent" policy:

```csharp
// Add to any controller
[EnableRateLimiting("concurrent")]
[HttpGet("heavy")]
public async Task<IActionResult> HeavyOperation()
{
    await Task.Delay(5000); // Simulate long-running operation
    return Ok();
}
```

Then test with parallel requests:

```powershell
$jobs = 1..25 | ForEach-Object {
    Start-Job -ScriptBlock {
        Invoke-RestMethod -Uri "http://localhost:5000/api/test/heavy"
    }
}
$jobs | Wait-Job | Receive-Job
```

### Test Different Endpoints Mix

```powershell
# Simulate real traffic pattern
while ($true) {
    Invoke-RestMethod -Uri "http://localhost:5000/api/products"
    Start-Sleep -Milliseconds 500
    Invoke-RestMethod -Uri "http://localhost:5000/api/customers"
    Start-Sleep -Milliseconds 300
    Invoke-RestMethod -Uri "http://localhost:5000/api/orders"
    Start-Sleep -Milliseconds 1000
}
```

---

## 🐛 Troubleshooting

### API Not Responding

```powershell
# Check if API is running
Test-NetConnection -ComputerName localhost -Port 5000
```

### No Rate Limiting Applied

1. Verify middleware order in `Program.cs`:
   ```csharp
   app.UseRateLimiter(); // Must be before app.MapControllers()
   ```

2. Check that policy is registered:
   ```csharp
   builder.Services.AddRateLimitingPolicies(); // In Program.cs
   ```

3. Verify controller attributes:
   ```csharp
   [EnableRateLimiting("orders")] // On controller or action
   ```

### Always Getting 429

- Check if you're testing too quickly after previous test
- Wait for window/bucket to reset
- Restart the API to reset all counters

---

## 📝 Summary

✅ **Applied Policies:**
- Orders: Fixed Window (strict limits)
- Products: Token Bucket (burst-friendly)
- Customers: Sliding Window (smooth)

✅ **Test Scripts Created:**
- `QuickTest.ps1` - Fast visual test
- `TestRateLimiting.ps1` - Comprehensive testing

✅ **Ready to Test:**
1. Start API: `dotnet run --project src\OrderService\OrderService.csproj`
2. Run tests: `.\QuickTest.ps1` or `.\TestRateLimiting.ps1`
3. Check for 429 responses and Retry-After headers
