#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Rinsen.InnovationBoost/Rinsen.InnovationBoost.csproj", "src/Rinsen.InnovationBoost/"]
COPY ["src/Rinsen.IdentityProvider/Rinsen.IdentityProvider.csproj", "src/Rinsen.IdentityProvider/"]
COPY ["src/Rinsen.Messaging/Rinsen.Messaging.csproj", "src/Rinsen.Messaging/"]
RUN dotnet restore "src/Rinsen.InnovationBoost/Rinsen.InnovationBoost.csproj"
COPY . .
WORKDIR "/src/src/Rinsen.InnovationBoost"
RUN dotnet build "Rinsen.InnovationBoost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Rinsen.InnovationBoost.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rinsen.InnovationBoost.dll"]