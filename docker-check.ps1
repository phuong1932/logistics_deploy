# Script kiểm tra Docker configuration
# Chạy script này để verify tất cả Dockerfile và docker-compose

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   DOCKER CONFIGURATION CHECK" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra Docker có được cài đặt không
Write-Host "1. Kiểm tra Docker..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version
    Write-Host "   ✓ $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Docker chưa được cài đặt hoặc chưa chạy!" -ForegroundColor Red
    exit 1
}

# Kiểm tra Docker Compose
Write-Host ""
Write-Host "2. Kiểm tra Docker Compose..." -ForegroundColor Yellow
try {
    $composeVersion = docker-compose --version
    Write-Host "   ✓ $composeVersion" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Docker Compose chưa được cài đặt!" -ForegroundColor Red
    exit 1
}

# Kiểm tra các Dockerfile
Write-Host ""
Write-Host "3. Kiểm tra Dockerfiles..." -ForegroundColor Yellow

$dockerfiles = @(
    @{ Path = "LogisticsAPI\logistic_web.api\Dockerfile"; Name = "API Service" },
    @{ Path = "LogisticsWebApp\Dockerfile"; Name = "Web App" },
    @{ Path = "LogisticsAppHost\Dockerfile"; Name = "AppHost" }
)

foreach ($dockerfile in $dockerfiles) {
    if (Test-Path $dockerfile.Path) {
        Write-Host "   ✓ $($dockerfile.Name): $($dockerfile.Path)" -ForegroundColor Green
    } else {
        Write-Host "   ✗ $($dockerfile.Name): KHÔNG TÌM THẤY" -ForegroundColor Red
    }
}

# Kiểm tra docker-compose.yml
Write-Host ""
Write-Host "4. Kiểm tra docker-compose.yml..." -ForegroundColor Yellow
if (Test-Path "docker-compose.yml") {
    Write-Host "   ✓ docker-compose.yml tồn tại" -ForegroundColor Green
    
    # Validate docker-compose file
    Write-Host ""
    Write-Host "5. Validate docker-compose.yml..." -ForegroundColor Yellow
    try {
        $composeConfig = docker-compose config 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✓ docker-compose.yml hợp lệ" -ForegroundColor Green
        } else {
            Write-Host "   ✗ docker-compose.yml có lỗi!" -ForegroundColor Red
        }
    } catch {
        Write-Host "   ✗ docker-compose.yml có lỗi!" -ForegroundColor Red
    }
} else {
    Write-Host "   ✗ docker-compose.yml KHÔNG TÌM THẤY" -ForegroundColor Red
}

# Kiểm tra .dockerignore
Write-Host ""
Write-Host "6. Kiểm tra .dockerignore..." -ForegroundColor Yellow
if (Test-Path ".dockerignore") {
    Write-Host "   ✓ .dockerignore tồn tại" -ForegroundColor Green
} else {
    Write-Host "   ! .dockerignore không tồn tại (khuyến nghị tạo)" -ForegroundColor Yellow
}

# Thống kê
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   TÓM TẮT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Tất cả các file đã được kiểm tra!" -ForegroundColor Green
Write-Host ""
Write-Host "Để build và chạy:" -ForegroundColor Yellow
Write-Host "  docker-compose build" -ForegroundColor White
Write-Host "  docker-compose up" -ForegroundColor White
Write-Host ""
Write-Host "Xem hướng dẫn chi tiết: DOCKER_README.md" -ForegroundColor Cyan

