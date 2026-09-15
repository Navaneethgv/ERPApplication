FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Backend/ERP.API/ERP.API.csproj Backend/ERP.API/
COPY Backend/ERP.Application/ERP.Application.csproj Backend/ERP.Application/
COPY Backend/ERP.Domain/ERP.Domain.csproj Backend/ERP.Domain/
COPY Backend/ERP.Infrastructure/ERP.Infrastructure.csproj Backend/ERP.Infrastructure/

RUN dotnet restore Backend/ERP.API/ERP.API.csproj

COPY . .

RUN dotnet publish Backend/ERP.API/ERP.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "ERP.API.dll"]