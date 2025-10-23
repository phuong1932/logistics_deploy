# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file và restore dependencies
COPY ["LogisticsWebApp.csproj", "./"]
RUN dotnet restore "LogisticsWebApp.csproj"

# Copy toàn bộ source code
COPY . .

# Build và publish
RUN dotnet build "LogisticsWebApp.csproj" -c Release -o /app/build
RUN dotnet publish "LogisticsWebApp.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy kết quả publish từ stage build
COPY --from=build /app/publish .

# Expose port
EXPOSE 80
EXPOSE 443

# Entrypoint
ENTRYPOINT ["dotnet", "LogisticsWebApp.dll"]

