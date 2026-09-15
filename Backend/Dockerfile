FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ERP.API/ERP.API.csproj ERP.API/
COPY ERP.Application/ERP.Application.csproj ERP.Application/
COPY ERP.Domain/ERP.Domain.csproj ERP.Domain/
COPY ERP.Infrastructure/ERP.Infrastructure.csproj ERP.Infrastructure/

RUN dotnet restore ERP.API/ERP.API.csproj

COPY . .

RUN dotnet publish ERP.API/ERP.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "ERP.API.dll"]
