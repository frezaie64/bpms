FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/BPMS.Shared/BPMS.Shared.csproj src/BPMS.Shared/
COPY src/BPMS.Modules.Identity/BPMS.Modules.Identity.csproj src/BPMS.Modules.Identity/
COPY src/BPMS.Modules.Tenancy/BPMS.Modules.Tenancy.csproj src/BPMS.Modules.Tenancy/
COPY src/BPMS.Api/BPMS.Api.csproj src/BPMS.Api/
RUN dotnet restore src/BPMS.Api/BPMS.Api.csproj

COPY . .
WORKDIR /src/src/BPMS.Api
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BPMS.Api.dll"]