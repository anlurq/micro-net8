# ===== build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MicroNet8Final.sln ./
COPY BuildingBlocks/BuildingBlocks.csproj BuildingBlocks/
COPY ApiPedidos/ApiPedidos.csproj ApiPedidos/
COPY ApiPagos/ApiPagos.csproj ApiPagos/
COPY ApiConsulta/ApiConsulta.csproj ApiConsulta/

RUN dotnet restore ApiConsulta/ApiConsulta.csproj

COPY . .
RUN dotnet publish ApiConsulta/ApiConsulta.csproj -c Release -o /app/publish /p:UseAppHost=false

# ===== runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiConsulta.dll"]
