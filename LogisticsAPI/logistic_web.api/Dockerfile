# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file và các project files
COPY ["logistic_web.api/logistic_web.api.csproj", "logistic_web.api/"]
COPY ["logistic_web.application/logistic_web.application.csproj", "logistic_web.application/"]
COPY ["logistic_web.infrastructure/logistic_web.infrastructure.csproj", "logistic_web.infrastructure/"]

# Restore dependencies cho tất cả projects
RUN dotnet restore "logistic_web.api/logistic_web.api.csproj"

# Copy toàn bộ source code
COPY . .

# Xóa appsettings.json của infrastructure và application để tránh conflict
RUN rm -f /src/logistic_web.infrastructure/appsettings*.json || true
RUN rm -f /src/logistic_web.application/appsettings*.json || true

# Build và publish
WORKDIR "/src/logistic_web.api"
RUN dotnet build "logistic_web.api.csproj" -c Release -o /app/build
RUN dotnet publish "logistic_web.api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy kết quả publish từ stage build
COPY --from=build /app/publish .

# Expose port
EXPOSE 80
EXPOSE 443

# Entrypoint
ENTRYPOINT ["dotnet", "logistic_web.api.dll"]

