#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Rinsen.Outback.Gui/Rinsen.Outback.Gui.csproj", "src/Rinsen.Outback.Gui/"]
COPY ["src/Rinsen.IdentityProvider/Rinsen.IdentityProvider.csproj", "src/Rinsen.IdentityProvider/"]
RUN dotnet restore "src/Rinsen.Outback.Gui/Rinsen.Outback.Gui.csproj"
COPY . .
WORKDIR "/src/src/Rinsen.Outback.Gui"
RUN dotnet build "Rinsen.Outback.Gui.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Rinsen.Outback.Gui.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Rinsen.Outback.Gui.dll"]