FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["KSMS.API/KSMS.API.csproj", "KSMS.API/"]
COPY ["KSMS.Application/KSMS.Application.csproj", "KSMS.Application/"]
COPY ["KSMS.Domain/KSMS.Domain.csproj", "KSMS.Domain/"]
COPY ["KSMS.Infrastructure/KSMS.Infrastructure.csproj", "KSMS.Infrastructure/"]
RUN dotnet restore "KSMS.API/KSMS.API.csproj"

COPY . .
WORKDIR "/src/KSMS.API"
RUN dotnet publish "KSMS.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KSMS.API.dll"]
