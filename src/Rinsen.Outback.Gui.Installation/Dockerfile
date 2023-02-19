#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Rinsen.InnovationBoost.Installation/Rinsen.InnovationBoost.Installation.csproj", "src/Rinsen.InnovationBoost.Installation/"]
COPY ["src/Rinsen.IdentityProvider/Rinsen.IdentityProvider.csproj", "src/Rinsen.IdentityProvider/"]
RUN dotnet restore "src/Rinsen.InnovationBoost.Installation/Rinsen.InnovationBoost.Installation.csproj"
COPY . .
WORKDIR "/src/src/Rinsen.InnovationBoost.Installation"
RUN dotnet build "Rinsen.InnovationBoost.Installation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Rinsen.InnovationBoost.Installation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rinsen.InnovationBoost.Installation.dll"]