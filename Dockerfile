# Stage 1: Build Backend (Dotnet)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-api

WORKDIR /app

# Sao chép file .csproj và các dependencies
COPY api/KSMS.API/KSMS.API.csproj api/KSMS.API/
COPY api/KSMS.Application/KSMS.Application.csproj api/KSMS.Application/
COPY api/KSMS.Domain/KSMS.Domain.csproj api/KSMS.Domain/
COPY api/KSMS.Infrastructure/KSMS.Infrastructure.csproj api/KSMS.Infrastructure/

# Cài đặt các dependencies
RUN dotnet restore api/KSMS.API/KSMS.API.csproj

# Sao chép toàn bộ mã nguồn vào container
COPY api/ ./

# Build ứng dụng backend
RUN dotnet publish api/KSMS.API/KSMS.API.csproj -c Release -o /app/publish-api

# Stage 2: Tạo Image cho API (sử dụng image chính thức .NET Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final-api

WORKDIR /app

# Sao chép ứng dụng đã build vào container
COPY --from=build-api /app/publish-api .

# Khởi động API
ENTRYPOINT ["dotnet", "KSMS.API.dll"]

EXPOSE 5000
# Stage 1: Build Backend (Dotnet)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-api

WORKDIR /app

# Sao chép file .csproj và các dependencies
COPY api/KSMS.API/KSMS.API.csproj api/KSMS.API/
COPY api/KSMS.Application/KSMS.Application.csproj api/KSMS.Application/
COPY api/KSMS.Domain/KSMS.Domain.csproj api/KSMS.Domain/
COPY api/KSMS.Infrastructure/KSMS.Infrastructure.csproj api/KSMS.Infrastructure/

# Cài đặt các dependencies
RUN dotnet restore api/KSMS.API/KSMS.API.csproj

# Sao chép toàn bộ mã nguồn vào container
COPY api/ ./

# Build ứng dụng backend
RUN dotnet publish api/KSMS.API/KSMS.API.csproj -c Release -o /app/publish-api

# Stage 2: Tạo Image cho API (sử dụng image chính thức .NET Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final-api

WORKDIR /app

# Sao chép ứng dụng đã build vào container
COPY --from=build-api /app/publish-api .

# Khởi động API
ENTRYPOINT ["dotnet", "KSMS.API.dll"]

EXPOSE 5000
