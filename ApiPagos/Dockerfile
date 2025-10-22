# ===== build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solo csproj para aprovechar caché de restore
COPY MicroNet8Final.sln ./
COPY BuildingBlocks/BuildingBlocks.csproj BuildingBlocks/
COPY ApiPedidos/ApiPedidos.csproj ApiPedidos/
COPY ApiPagos/ApiPagos.csproj ApiPagos/
COPY ApiConsulta/ApiConsulta.csproj ApiConsulta/

# Restore del proyecto de ApiPagos (con sus referencias)
RUN dotnet restore ApiPagos/ApiPagos.csproj

# Copiar el resto del código
COPY . .

# Publish
RUN dotnet publish ApiPagos/ApiPagos.csproj -c Release -o /app/publish /p:UseAppHost=false

# ===== runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiPagos.dll"]
