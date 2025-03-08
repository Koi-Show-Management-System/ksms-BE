FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["KSMS.API/KSMS.API.csproj", "KSMS.API/"]
COPY ["KSMS.Application/KSMS.Application.csproj", "KSMS.Application/"]
COPY ["KSMS.Domain/KSMS.Domain.csproj", "KSMS.Domain/"]
COPY ["KSMS.Infrastructure/KSMS.Infrastructure.csproj", "KSMS.Infrastructure/"]
RUN dotnet restore "KSMS.API/KSMS.API.csproj"
COPY . .
WORKDIR "/src/KSMS.API"
RUN dotnet build "KSMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KSMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KSMS.API.dll"]