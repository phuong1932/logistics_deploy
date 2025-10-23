#!/bin/bash
# Script kiểm tra Docker configuration
# Chạy script này để verify tất cả Dockerfile và docker-compose

echo "========================================"
echo "   DOCKER CONFIGURATION CHECK"
echo "========================================"
echo ""

# Kiểm tra Docker có được cài đặt không
echo "1. Kiểm tra Docker..."
if command -v docker &> /dev/null; then
    docker_version=$(docker --version)
    echo "   ✓ $docker_version"
else
    echo "   ✗ Docker chưa được cài đặt hoặc chưa chạy!"
    exit 1
fi

# Kiểm tra Docker Compose
echo ""
echo "2. Kiểm tra Docker Compose..."
if command -v docker-compose &> /dev/null; then
    compose_version=$(docker-compose --version)
    echo "   ✓ $compose_version"
else
    echo "   ✗ Docker Compose chưa được cài đặt!"
    exit 1
fi

# Kiểm tra các Dockerfile
echo ""
echo "3. Kiểm tra Dockerfiles..."

declare -a dockerfiles=(
    "LogisticsAPI/logistic_web.api/Dockerfile:API Service"
    "LogisticsWebApp/Dockerfile:Web App"
    "LogisticsAppHost/Dockerfile:AppHost"
)

for item in "${dockerfiles[@]}"; do
    IFS=':' read -r path name <<< "$item"
    if [ -f "$path" ]; then
        echo "   ✓ $name: $path"
    else
        echo "   ✗ $name: KHÔNG TÌM THẤY"
    fi
done

# Kiểm tra docker-compose.yml
echo ""
echo "4. Kiểm tra docker-compose.yml..."
if [ -f "docker-compose.yml" ]; then
    echo "   ✓ docker-compose.yml tồn tại"
    
    # Validate docker-compose file
    echo ""
    echo "5. Validate docker-compose.yml..."
    if docker-compose config > /dev/null 2>&1; then
        echo "   ✓ docker-compose.yml hợp lệ"
    else
        echo "   ✗ docker-compose.yml có lỗi!"
    fi
else
    echo "   ✗ docker-compose.yml KHÔNG TÌM THẤY"
fi

# Kiểm tra .dockerignore
echo ""
echo "6. Kiểm tra .dockerignore..."
if [ -f ".dockerignore" ]; then
    echo "   ✓ .dockerignore tồn tại"
else
    echo "   ! .dockerignore không tồn tại (khuyến nghị tạo)"
fi

# Thống kê
echo ""
echo "========================================"
echo "   TÓM TẮT"
echo "========================================"
echo "Tất cả các file đã được kiểm tra!"
echo ""
echo "Để build và chạy:"
echo "  docker-compose build"
echo "  docker-compose up"
echo ""
echo "Xem hướng dẫn chi tiết: DOCKER_README.md"

