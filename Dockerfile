# Use the official .NET SDK 8 image as the build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files first for better layer caching
COPY ksms-BE.sln .
COPY KSMS.API/*.csproj KSMS.API/
COPY KSMS.Application/*.csproj KSMS.Application/
COPY KSMS.Domain/*.csproj KSMS.Domain/
COPY KSMS.Infrastructure/*.csproj KSMS.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the code
COPY . .

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the application
FROM build AS publish
RUN dotnet publish KSMS.API/KSMS.API.csproj -c Release -o /app/publish --no-build

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

EXPOSE 80
ENTRYPOINT ["dotnet", "KSMS.API.dll"]