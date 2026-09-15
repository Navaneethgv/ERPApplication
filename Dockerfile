# =========================
# BUILD
# =========================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY Backend/ERP.API/ERP.API.csproj Backend/ERP.API/
COPY Backend/ERP.Application/ERP.Application.csproj Backend/ERP.Application/
COPY Backend/ERP.Domain/ERP.Domain.csproj Backend/ERP.Domain/
COPY Backend/ERP.Infrastructure/ERP.Infrastructure.csproj Backend/ERP.Infrastructure/

RUN dotnet restore Backend/ERP.API/ERP.API.csproj

COPY Backend/ Backend/

RUN dotnet publish Backend/ERP.API/ERP.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


# =========================
# FINAL
# =========================

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Install nginx
RUN apt-get update \
    && apt-get install -y nginx \
    && rm -rf /var/lib/apt/lists/*

# Backend
COPY --from=build /app/publish .

# Frontend
COPY Frontend/CompanyDashboard/ /usr/share/nginx/html/

# Nginx configuration
COPY nginx.conf /etc/nginx/sites-available/default

ENV ASPNETCORE_URLS=http://127.0.0.1:5000

EXPOSE 10000

CMD sh -c "dotnet ERP.API.dll & nginx -g 'daemon off;'"